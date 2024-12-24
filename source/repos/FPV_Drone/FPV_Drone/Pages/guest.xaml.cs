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
using FPV_Drone.Model;

namespace FPV_Drone.Pages
{
    /// <summary>
    /// Логика взаимодействия для guest.xaml
    /// </summary>
    public partial class guest : Page
    {
        public guest()
        {
            InitializeComponent();
            LoadData();
        }



    private void LoadData()
        {
            using (FPV_dronEntities1 dbContext = new FPV_dronEntities1())
            {
                var roles = dbContext.Type_Drone.ToList(); // Получаем все записи из таблицы People
                dg_guest.ItemsSource = roles; // Устанавливаем источник данных для DataGrid
            }
        }
    }
}
