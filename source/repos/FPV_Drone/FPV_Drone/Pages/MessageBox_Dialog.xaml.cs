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

namespace FPV_Drone.Pages
{
    /// <summary>
    /// Логика взаимодействия для MessageBox_Dialog.xaml
    /// </summary>
    public partial class MessageBox_Dialog : Window
    {
        public MessageBox_Dialog()
        {
            InitializeComponent();
        }

        private void btn_Ok(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(tb_password.Password)) {
                string pas_user = tb_password.Password; // txtInput — это TextBox на вашей форме
                if (pas_user == "0000")
                {
                    close(true);
                }
                else
                {
                    close(false);
                }
            }
        }

        private void btn_Close(object sender, RoutedEventArgs e)
        {
            close(false);
        }

        public void close(bool mode)
        {
            this.DialogResult = mode;
            this.Close();
        }
    }
}
