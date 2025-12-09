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

        // Переменные логики (как было у вас)
        int P = 1;
        int T = -1;
        int I = 1;
        int Y = 0;
        int otstup = 115;
        private string _loadedAudio;

        public MainWindow()
        {
            InitializeComponent();

            // Подписываемся на события загрузки и закрытия
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed; // <--- ВАЖНО: Убивает процесс при выходе
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeVoskAsync();
        }

        // Логика правильного закрытия программы
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
            catch { /* Игнорируем ошибки при выходе */ }
            finally
            {
                // ГАРАНТИРОВАННО УБИВАЕМ ПРОЦЕСС
                Environment.Exit(0);
            }
        }

        private async Task InitializeVoskAsync()
        {
            T1.Text = "⏳ Инициализация Vosk... Ждите.";
            REC.IsEnabled = false;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Проверьте, как точно называется папка: ModelVosk или ModelVosk2 ?
            // Я оставил ModelVosk2, как было в вашем втором методе. Если папка называется ModelVosk, поменяйте тут.
            string modelPath = System.IO.Path.Combine(baseDir, "ModelVosk");

            try
            {
                await Task.Run(() =>
                {
                    if (!Directory.Exists(modelPath))
                    {
                        // Пытаемся найти альтернативное имя, если первой нет
                        string altPath = System.IO.Path.Combine(baseDir, "ModelVosk");
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
                MessageBox.Show($"Ошибка Vosk: {ex.Message}\nПроверьте папку ModelVosk в папке Debug/Release!");
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

                    // Визуал
                    REC.IsEnabled = false;
                    REC.Margin = new Thickness(1000, 1000, 0, 0); // Прячем кнопку (ваш стиль)

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

        private void STOP_MouseUp(object sender, MouseButtonEventArgs e)
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
                    _recognizer.Dispose(); // Важно освободить
                    _recognizer = null;
                }

                // Возвращаем интерфейс
                T1.Text = $"✅ ЗАПИСЬ ЗАВЕРШЕНА. Итог:\n\n{_accumulatedText}";

                STOP.IsEnabled = false;
                STOP.Margin = new Thickness(1000, 1000, 0, 0);

                REC.IsEnabled = true;
                REC.Margin = new Thickness(11, 0, 0, 10);
                I = 1;

                // Ваша логика с картинками
                AddResultImage();
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
        //
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

        // Вынес вашу логику картинок в отдельный метод для чистоты
        private void AddResultImage()
        {
            if (Y >= 4)
            {
                MessageBox.Show("Попробуйте еще раз! Места нет.");
                return;
            }

            string uriSource = "";

            if (P > 0 && T > 0) uriSource = "pack://application:,,,/Component 1 (15).png";
            else if (P < 0 && T > 0) uriSource = "pack://application:,,,/Component 1 (16).png";
            else if (P > 0 && T < 0) uriSource = "pack://application:,,,/Component 1 (17).png";
            else if (P < 0 && T < 0)
            {
                MessageBox.Show("Результаты стали хуже. Попробуйте заново!");
                return;
            }

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
                    W.Children.Add(img); // Убедитесь, что Grid/StackPanel называется 'W' в XAML
                    Y++;

                    // Сброс логики, как у вас было
                    if (uriSource.Contains("15")) P = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось загрузить картинку: " + ex.Message);
                }
            }
        }

        private void LoginB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Login login = new Login();
            bool? dialogResult = login.ShowDialog();
            if (dialogResult == true)
            {
                // string TB_Me = login.ResultData; // Логика использования данных
            }
            else
            {
                MessageBox.Show("Вы не рассказали о себе!");
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
            }
        }
    }
}