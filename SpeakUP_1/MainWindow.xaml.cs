using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace SpeakUP_1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Переменная P - прогресс в словах паразитах
        // Переменная T - прогресс в темпе речи
        int P = 1;
        int T = -1;
        string imagePath = "\\SpeakUP_1\\SpeakUP_1\\Component 1 (15).png";
        private string _loadedAudio;
        public MainWindow()
        {
            InitializeComponent();
        }
        int I = 1;
        int Y = 0;
        int otstup = 115;
        private void LoginB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Login login = new Login();
            bool? dialogResult = login.ShowDialog();
            if (dialogResult == true)
            {
                string TB_Me = login.ResultData;
            }
            else if (dialogResult == false)
            {
                MessageBox.Show("Вы не рассказали о себе!");
            }
            else
            {
                MessageBox.Show("Вы не рассказали о себе!");
            }
        }
        
        private void STOP_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (I == 0)
            {
                STOP.IsEnabled = false;
                STOP.Margin = new Thickness(1000, 1000, 0, 0);
                I = 1;
                REC.IsEnabled = true;
                REC.Margin = new Thickness(11, 0, 0, 10);
                for (int i=0 ; i < 1; i ++)
                {
                    if (Y < 4)
                    {
                        if ((P > 0) && (T > 0))
                        {
                            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Component 1 (15).png", UriKind.Absolute));
                            var img = new Image
                            {
                                Source = bitmap,
                                Width = 244,
                                Height = 105,
                                Stretch = Stretch.Fill,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, (otstup * Y) + 10, 0, 0)

                            };
                            W.Children.Add(img);
                            Y++;
                            P = -1;
                        }
                        if ((P < 0) && (T > 0))
                        {
                            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Component 1 (16).png", UriKind.Absolute));
                            var img = new Image
                            {
                                Source = bitmap,
                                Width = 244,
                                Height = 105,
                                Stretch = Stretch.Fill,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, (otstup * Y) + 10, 0, 0)

                            };
                            W.Children.Add(img);
                            Y++;
                        }
                        if ((P > 0) && (T < 0))
                        {
                            var bitmap = new BitmapImage(new Uri("pack://application:,,,/Component 1 (17).png", UriKind.Absolute));
                            var img = new Image
                            {
                                Source = bitmap,
                                Width = 244,
                                Height = 105,
                                Stretch = Stretch.Fill,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(0, (otstup * Y) + 10, 0, 0)
                            };
                            W.Children.Add(img);
                            Y++;
                        }
                        if ((P < 0) && (T < 0))
                        {
                            MessageBox.Show("Попробуйте заново! Ваши результаты стали хуже");

                        }
                    }
                    else
                    {
                        MessageBox.Show("Попробуйте еще раз!");
                    }
                }
            } }

        private void REC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (I == 1)
            {
                REC.IsEnabled = false;
                REC.Margin = new Thickness(1000, 1000, 0, 0);
                I = 0;
                STOP.IsEnabled = true;
                STOP.Margin = new Thickness(11, 0, 0, 10);
            }
        }

        private void File_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Аудиозаписи (*.mp3;*.wav)|*.mp3;*.wav|Все файлы (*.*)|*.*";
            openFileDialog.Title = "Выберите аудиозапись";
            bool? result = openFileDialog.ShowDialog();
            string selectedFilePath = openFileDialog.FileName;
            if (result == true)
            {
                MessageBox.Show($"Файл успешно выбран: {selectedFilePath}");
                _loadedAudio = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Файл не был выбран.");
            }
        }
    }
}
 