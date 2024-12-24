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
using FPV_Drone.Pages;
using FPV_Drone.Model;

namespace FPV_Drone.Pages
{
    /// <summary>
    /// Логика взаимодействия для admin.xaml
    /// </summary>
    public partial class admin : Page
    {
        Users user4 = new Users();
        public admin(Users user3)
        {
            InitializeComponent();
            if (user3 != null)
            {
                user4 = user3;
            }
            else {
                MessageBox.Show("ты лох сразу","");
            }
        }

        private void log_Click(object sender, RoutedEventArgs e)
        {
            
            NavigationService.Navigate(new user(user4));
        }

        private void reg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new regist());
        }

        private void guest_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new guest());
        }
    }
}
