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
using System.Collections.ObjectModel;

namespace FPV_Drone.Pages
{


    public partial class user : Page
    {
        Users user4;
      //  Type_Drone type_1 = new Type_Drone();

        public ObservableCollection<DroneUserInfo> DroneInfoList { get; set; }

        private string typeID = "";//Номер выбранного типа агента
        private string fnd = "";//Строка поиска
        bool is_admin = false;

        public user(Users user2)
        {
            InitializeComponent();
            DroneInfoList = new ObservableCollection<DroneUserInfo>();
            DataContext = this; // Устанавливаем DataContext для привязки данных
            user4 = user2;
            //long id_role_3 = user4.id_role;


            if (user4.id_role == 1 || user4.id_role == 4 || user4.id_role == 9 || user4.id_role == 10) {

                MessageBox.Show("Вы админ!", "Поздравляю");
                is_admin = true;
                LoadItems(user2, is_admin);
                tb_user.Text = $"{user2.name ?? "Имя не задано"} {user2.surname ?? "Фамилия не задана"}"; 
            }
           
            
            else {
                if (user4 != null)
                {
                    MessageBox.Show("1!", "Поздравляю");
                    is_admin = false;
                    LoadItems(user2, is_admin);
                    tb_user.Text = $"{user2.name ?? "Имя не задано"} {user2.surname ?? "Фамилия не задана"}";
                }
            }

            using (var context = new FPV_dronEntities1())
            {
                List<Model.Type_Drone> types = new List<Model.Type_Drone> { };
                types = context.Type_Drone.ToList();
                types.Add(new Model.Type_Drone { name = "Все типы" });
                tb_Type_drone.ItemsSource = types.OrderBy(Type_Drone => Type_Drone.id_type_drone);
            }
        }
        // 8 ноября

        private void LoadItems(Users user4, bool admin)
        {
            using (var context = new FPV_dronEntities1())
            {
                if (admin == true)
                {
                    var query = from drone in context.Drone
                                join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                                join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                                join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                                join user in context.Users on droneUser.id_user equals user.id_user
                                join role in context.Role on user.id_role equals role.id_role
                                orderby user.surname
                                select new DroneUserInfo
                                {
                                    Id = drone.id_drone,
                                    ModelName = drone.name_model,
                                    TypeName = typeDrone.name,
                                    ControlTypeName = controlType.name,
                                    UserName = user.name,
                                    UserSurname = user.surname,
                                    RoleName = role.name,
                                    ImagePath = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\Images\drone_vehicle_transport_icon_208510.ico",
                                    IsEmpty = false,
                                };
                    if (tb_Search != null)
                    {
                        var resultList = query.Where(Drone => Drone.ModelName.Contains(fnd) || Drone.ControlTypeName.Contains(fnd)).ToList(); // Выполняем запрос и извлекаем результаты

                        if (tb_Type_drone.SelectedItem != null && typeID != "Все типы")
                        {
                            var rest = resultList.Where((Drone => Drone.TypeName == typeID));
                            // MessageBox.Show("Увы сортировка не удалась!");
                            DroneInfoList.Clear(); // Очищаем предыдущие данные
                            foreach (var item in rest)
                            {
                                DroneInfoList.Add(item); // Добавляем загруженные данные в коллекцию
                            }
                        }
                        else
                        {
                            DroneInfoList.Clear(); // Очищаем предыдущие данные
                            foreach (var item in resultList)
                            {
                                DroneInfoList.Add(item); // Добавляем загруженные данные в коллекцию
                            }
                        }
                    }
                    else
                    {
                        var resultList = query.ToList(); // Выполняем запрос и извлекаем результаты
                        DroneInfoList.Clear(); // Очищаем предыдущие данные
                        foreach (var item in resultList)
                        {
                            DroneInfoList.Add(item); // Добавляем загруженные данные в коллекцию
                        }
                    }
                }
                // админ


                if (admin == false)
                {
                   
                        var query = from drone in context.Drone
                                    join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                                    join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                                    join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                                    join user in context.Users on droneUser.id_user equals user.id_user
                                    join role in context.Role on user.id_role equals role.id_role
                                    where user.id_user == user4.id_user
                                    orderby user.surname

                                    select new DroneUserInfo
                                    {
                                        ModelName = drone.name_model,
                                        TypeName = typeDrone.name,
                                        ControlTypeName = controlType.name,
                                        UserName = user.name,
                                        UserSurname = user.surname,
                                        RoleName = role.name,
                                        ImagePath = @"C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\Images\\drone_vehicle_transport_icon_208510.ico" // Сохраняем путь в строку

                                    };
                        if (tb_Search != null)
                        {
                            var resultList = query.Where(Drone => Drone.ModelName.Contains(fnd) || Drone.ControlTypeName.Contains(fnd)).ToList(); // Выполняем запрос и извлекаем результаты

                            if (tb_Type_drone.SelectedItem != null && typeID != "Все типы")
                            {
                                var rest = resultList.Where((Drone => Drone.TypeName == typeID));
                                // MessageBox.Show("Увы сортировка не удалась!");
                                DroneInfoList.Clear(); // Очищаем предыдущие данные
                                foreach (var item in rest)
                                {
                                    DroneInfoList.Add(item); // Добавляем загруженные данные в коллекцию
                                }
                            }
                            else
                            {
                                DroneInfoList.Clear(); // Очищаем предыдущие данные
                                foreach (var item in resultList)
                                {
                                    DroneInfoList.Add(item); // Добавляем загруженные данные в коллекцию
                                }
                            }
                        }
                        else
                        {
                            var resultList = query.ToList(); // Выполняем запрос и извлекаем результаты
                            DroneInfoList.Clear(); // Очищаем предыдущие данные
                            foreach (var item in resultList)
                            {
                                DroneInfoList.Add(item); // Добавляем загруженные данные в коллекцию
                            }
                        }
                } // обычный юзер

            }
        }
        

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            

            var border = sender as Border; // Получаем Border, на который кликнули
            if (border != null)
            {
                var droneUserInfo = border.DataContext as DroneUserInfo;
                if (droneUserInfo != null)
                {
                    if (droneUserInfo.IsEmpty)
                    {
                        // Переход на страницу создания нового дрона
                        NavigationService.Navigate(new new_user_drone(user4));
                    }
                    else
                    {
                        // Выполняем действия с выбранным объектом
                        MessageBox.Show($"Вы выбрали дрон: {droneUserInfo.ModelName} ({droneUserInfo.TypeName})");
                        NavigationService.Navigate(new user_drone(user4, droneUserInfo.ModelName, is_admin));
                    }
                }
            }
        }

        private void TextBox_TextChanged_search(object sender, TextChangedEventArgs e)
        {
            fnd = ((TextBox)sender).Text;
            DroneInfoList.Clear();
            LoadItems(user4, is_admin);
        }

        private void Type_SelectionChanged_serach(object sender, SelectionChangedEventArgs e)
        {
            typeID = ((Model.Type_Drone)tb_Type_drone.SelectedItem).name;
            DroneInfoList.Clear();
            LoadItems(user4, is_admin);
        }
        // Получаем значение clicked card
        /*    var border = sender as Border; // Получаем Border, на который кликнули
            if (border != null)
            {
                // Получаем DataContext (т.е. объект DroneUserInfo) для Border
                var droneUserInfo = border.DataContext as DroneUserInfo;
                if (droneUserInfo != null)
                {
                    // Выполните действия с выбранным объектом, например, откройте детальную информацию
                    MessageBox.Show($"Вы выбрали дрон: {droneUserInfo.ModelName} ({droneUserInfo.TypeName})");
                    NavigationService.Navigate(new user_drone(user4, droneUserInfo.ModelName.ToString()));
                    // Здесь вы можете выполнить дополнительные действия, такие как:
                    // 1. Переход на другую страницу
                    // 2. Открытие окна с деталями
                    // 3. Другие действия с объектом
                }
            }
        }
        */

        public class DroneUserInfo
        {
            public long Id { get; set; }
            public string ModelName { get; set; }
            public string TypeName { get; set; }
            public string ControlTypeName { get; set; }
            public string UserName { get; set; }
            public string UserSurname { get; set; }
            public string RoleName { get; set; }
            public string ImagePath { get; set; }
            public bool IsEmpty { get; set; }
        }

        private void btnChange(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new new_user_drone(user4));
        }


        private void btnGraf(object sender, RoutedEventArgs e)
        {
            Grath grath = new Grath(user4, is_admin);
            grath.Show(); // Открывает новое окно
        }

      /*  private void btnFlight(object sender, RoutedEventArgs e)
        {
            btnArd.IsEnabled = false;
            Arduino_write arduino_window = new Arduino_write(user4);
            arduino_window.Show(); // Открывает новое окно
        }*/

        private void btnFlight(object sender, RoutedEventArgs e)
        {
            btnArd.IsEnabled = false; // Делаем кнопку неактивной
            Arduino_write arduino_window = new Arduino_write(user4); // Передаем текущий экземпляр
            arduino_window.WindowClosed += OnArduinoWindowClosed; // Подписка на событие
            arduino_window.Show(); // Открываем новое окно

        }

        private void OnArduinoWindowClosed()
        {
            btnArd.IsEnabled = true; // Делает кнопку снова активной
        }

    }
}

/// <summary>
/// Логика взаимодействия для login.xaml
/// </summary>
/*   public partial class user : Page
   {
       public user()
       {
           InitializeComponent();
       }
       private void LoadItems()
       {
           using (var context = new FPV_dronEntities1()) // Замените YourDbContext на имя вашего контекста базы данных
           {
               var query = from drone in context.Drone
                           join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                           join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                           join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                           join user in context.Users on droneUser.id_user equals user.id_user
                           join role in context.Role on user.id_role equals role.id_role
                           orderby user.surname
                           select new
                           {
                               ModelName = drone.name_model,
                               TypeName = typeDrone.name,
                               ControlTypeName = controlType.name,
                               UserName = user.name,
                               UserSurname = user.surname,
                               RoleName = role.name
                           };

               var resultList = query.ToList(); // Выполняем запрос и извлекаем результаты
           }
       }
   }
}*/
