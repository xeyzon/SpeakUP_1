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
using System.Windows.Shapes;

namespace SpeakUP_1
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public string ResultData { get; private set; }
        public Login()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TB_Me_GotFocus(object sender, RoutedEventArgs e)
        {
            TB_Me.Text = "";
            this.ResultData = TB_Me.Text;

        }

        private void Save_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ResultData = TB_Me.Text;
            this.DialogResult = true;
        }
    }
}
