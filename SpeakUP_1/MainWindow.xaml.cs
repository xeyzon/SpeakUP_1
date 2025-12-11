using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Vosk;
using NAudio.Wave;
using Newtonsoft.Json.Linq;

namespace SpeakUP_1
{
    public partial class MainWindow : Window
    {
        private Model _model;
        private VoskRecognizer _recognizer;
        private WaveInEvent _waveIn;

        private string _accumulatedText = "";

        // <--- NEW: Переменная для хранения роли пользователя
        private string _userRole = "Неизвестный спикер";

        // <--- NEW: Экземпляр нашего сервиса GigaChat
        private GigaChatService _gigaChatService = new GigaChatService();

        // Переменные логики
        int P = 1;
        int T = -1;
        int I = 1;
        int Y = 0;
        int otstup = 115;
        private string _loadedAudio;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeVoskAsync();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                }
                if (_recognizer != null) _recognizer.Dispose();
                if (_model != null) _model.Dispose();
            }
            catch { }
            finally
            {
                Environment.Exit(0);
            }
        }

        private async Task InitializeVoskAsync()
        {
            T1.Text = "⏳ Инициализация Vosk... Ждите.";
            REC.IsEnabled = false;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string modelPath = System.IO.Path.Combine(baseDir, "ModelVosk"); // Проверьте имя папки

            try
            {
                await Task.Run(() =>
                {
                    if (!Directory.Exists(modelPath))
                    {
                        string altPath = System.IO.Path.Combine(baseDir, "ModelVosk2"); // Проверка альтернативы
                        if (Directory.Exists(altPath)) modelPath = altPath;
                        else throw new DirectoryNotFoundException($"Папка модели не найдена: {modelPath}");
                    }

                    Vosk.Vosk.SetLogLevel(0);
                    _model = new Model(modelPath);
                });

                T1.Text = "✅ Vosk готов. Нажмите 'Начать запись'.";
                REC.IsEnabled = true;
            }
            catch (Exception ex)
            {
                T1.Text = "ОШИБКА ЗАГРУЗКИ";
                MessageBox.Show($"Ошибка Vosk: {ex.Message}");
            }
        }

        private void REC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (I == 1 && _model != null)
            {
                try
                {
                    _accumulatedText = "";
                    _recognizer = new VoskRecognizer(_model, 16000.0f);
                    _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
                    _waveIn.DataAvailable += WaveIn_DataAvailable;
                    _waveIn.StartRecording();

                    REC.IsEnabled = false;
                    REC.Margin = new Thickness(1000, 1000, 0, 0);

                    STOP.IsEnabled = true;
                    STOP.Margin = new Thickness(11, 0, 0, 10);

                    T1.Text = "🎧 Слушаю...";
                    I = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка микрофона: " + ex.Message);
                    REC.IsEnabled = true;
                }
            }
        }

        // <--- NEW: Добавили 'async' к сигнатуре метода
        private async void STOP_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (I == 0)
            {
                // Останавливаем запись
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                    _waveIn = null;
                }

                // Достаем остатки текста
                if (_recognizer != null)
                {
                    var finalJson = _recognizer.Result();
                    ProcessResult(finalJson, isPartial: false);
                    _recognizer.Dispose();
                    _recognizer = null;
                }

                T1.Text = $"✅ ЗАПИСЬ ЗАВЕРШЕНА. Итог:\n{_accumulatedText}";

                // Возвращаем интерфейс
                STOP.IsEnabled = false;
                STOP.Margin = new Thickness(1000, 1000, 0, 0);
                REC.IsEnabled = true;
                REC.Margin = new Thickness(11, 0, 0, 10);
                I = 1;

                AddResultImage();

                // <--- NEW: Вызов GigaChat для анализа
                // Проверяем, есть ли текст для анализа
                if (!string.IsNullOrWhiteSpace(_accumulatedText) && _accumulatedText.Length > 10)
                {
                    T1.Text += "\n\n GigaChat думает...";

                    // Вызываем сервис
                    string aiAdvice = await _gigaChatService.SendRequestAsync(_userRole, _accumulatedText);


                    // Выводим результат. Лучше сделать отдельный TextBox для советов, 
                    // но пока добавим к основному тексту
                    T1.Text += $"\n\n СОВЕТ :\n{aiAdvice}";
                    T1.ScrollToEnd();
                }
                else
                {
                    T1.Text += "\n\n(Текст слишком короткий для анализа ИИ)";
                }
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_recognizer != null)
                {
                    if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                    {
                        ProcessResult(_recognizer.Result(), false);
                    }
                    else
                    {
                        ProcessResult(_recognizer.PartialResult(), true);
                    }
                }
            });
        }

        private void ProcessResult(string json, bool isPartial)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var jsonObj = JObject.Parse(json);
                string text = jsonObj[isPartial ? "partial" : "text"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(text)) return;

                if (isPartial)
                {
                    T1.Text = _accumulatedText + text + "...";
                }
                else
                {
                    _accumulatedText += text + ". ";
                    T1.Text = _accumulatedText;
                }
                T1.ScrollToEnd();
            }
            catch { }
        }

        private void AddResultImage()
        {
            // Ваш код добавления картинок (без изменений)
            if (Y >= 4) { MessageBox.Show("Попробуйте еще раз! Места нет."); return; }

            string uriSource = "";
            if (P > 0 && T > 0) uriSource = "pack://application:,,,/Component 1 (15).png";
            else if (P < 0 && T > 0) uriSource = "pack://application:,,,/Component 1 (16).png";
            else if (P > 0 && T < 0) uriSource = "pack://application:,,,/Component 1 (17).png";
            else if (P < 0 && T < 0) { MessageBox.Show("Результаты хуже. Заново!"); return; }

            if (!string.IsNullOrEmpty(uriSource))
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(uriSource, UriKind.Absolute));
                    var img = new Image
                    {
                        Source = bitmap,
                        Width = 244,
                        Height = 105,
                        Stretch = System.Windows.Media.Stretch.Fill,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(70, (otstup * Y) + 10, 0, 0)
                    };
                    W.Children.Add(img);
                    Y++;
                    if (uriSource.Contains("15")) P = -1;
                }
                catch (Exception ex) { MessageBox.Show("Ошибка картинки: " + ex.Message); }
            }
        }

        // Фрагмент из MainWindow.cs
        private void LoginB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Login login = new Login();
            bool? dialogResult = login.ShowDialog();

            if (dialogResult == true)
            {
                // <--- NEW: Сохраняем данные пользователя
                _userRole = login.ResultData;

                // 🟢 ИСПРАВЛЕНИЕ: Передаем актуальное значение в сервис GigaChat
                _gigaChatService.UserRole = _userRole;

                MessageBox.Show($"Роль записана: {_userRole}");
            }
            else
            {
                MessageBox.Show("Вы не рассказали о себе! Анализ будет общим.");
                _userRole = "Спикер";

                // 🟢 ИСПРАВЛЕНИЕ: Передаем значение по умолчанию в сервис GigaChat
                _gigaChatService.UserRole = _userRole;
            }
        }

        private void File_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Аудио (*.mp3;*.wav)|*.mp3;*.wav";
            if (openFileDialog.ShowDialog() == true)
            {
                _loadedAudio = openFileDialog.FileName;
                MessageBox.Show($"Файл: {_loadedAudio}");
                // Тут пока нет логики распознавания файла, но для нее подход тот же:
                // Распознали Vosk -> Получили текст -> Вызвали _gigaChatService.SendRequestAsync
            }
        }
    }
}