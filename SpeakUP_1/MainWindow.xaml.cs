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
        private string _loadedAudio;
        public MainWindow()
        {
            InitializeComponent();
        }
        int I = 1;
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
                for (int i = 0; i < 5; i++)
                {

                }
            }
        }

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
 