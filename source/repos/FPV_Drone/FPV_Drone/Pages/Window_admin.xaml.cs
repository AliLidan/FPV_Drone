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
    /// Логика взаимодействия для Window_admin.xaml
    /// </summary>
    public partial class Window_admin : Window
    {
        private string _currentPageName;
        public Window_admin(Page page)
        {
            InitializeComponent();
            MainFrame.Navigate(page);
            _currentPageName = page.ToString();
        }

        private void btn_back(object sender, RoutedEventArgs e)
        {
            try
            {
                // MainFrame.
                _currentPageName = MainFrame.Content.ToString();
                // MessageBox.Show($"_currentPageName = {_currentPageName}");
                if (_currentPageName == "FPV_Drone.Pages.user")
                {

                    Window_login login = new Window_login();
                    login.Top = 100;
                    login.Left = 250;
                    login.Show();
                    this.Close();

                }
                else { 
                    MainFrame.GoBack();
                }
               
            }
            catch 
            {
                Window_login login = new Window_login();
                login.Top = 100;
                login.Left = 250;
                login.Show();
                this.Close();
            }
        }
    }
    
}
