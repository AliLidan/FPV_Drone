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

namespace windows_go_back
{
    /// <summary>
    /// Логика взаимодействия для Frame_1.xaml
    /// </summary>
    public partial class Frame_1 : Page
    {
        public Frame_1()
        {
            InitializeComponent();
        }

        private void backing_0(object sender, RoutedEventArgs e)
        {
            // _mainFrame.NavigationService.Navigate(new MainWindow());
            //_mainFrame.NavigationService.Navigate(new Uri(h));
            //   ‪_mainFrame.NavigationService.Navigate(new Uri(""));
        //    _mainFrame.NavigationService.Navigate(new Uri("https://hd12.22lordfilm.fun/1054-venom-f25.html"));
           _mainFrame.NavigationService.Navigate(new Uri("Frame_2.xaml", UriKind.Relative));
     //    //  _mainFrame.NavigationService.Navigate(new Page1());
     //      _mainFrame.NavigationService.Navigate(new Button());
           _mainFrame.NavigationService.Navigate("Hello world");
        }
    }
}
