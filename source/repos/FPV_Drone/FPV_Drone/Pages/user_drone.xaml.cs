using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для user_drone.xaml
    /// </summary>
    public partial class user_drone : Page
    {
        FPV_dronEntities1 dbContext = new FPV_dronEntities1();
        Users user4;
        long drone_id_;
        string old_name;
        long drone_user_;
        int recordCount;

        public user_drone(Users user2, string name_mod, bool iz_admin)
        {
            InitializeComponent();
            LoadComboBoxData();
            LoadComboBoxData_1();

            if (user2 != null && iz_admin == false)
            {
                MessageBox.Show("i all_ user");
                LoadItems(user2, name_mod, iz_admin);
                user4 = user2;
            }
            else
            {
                MessageBox.Show("i all _ admin");
                LoadItems(user2, name_mod, iz_admin);
                user4 = user2;
            }
        }

        // загрузка в комбобоксы + описание. здесь создается 
        //      одноразовые листы + дети класса

        private void LoadComboBoxData()  // тип дрона
        {
            using (var context = new FPV_dronEntities1())
            {
                var roles = context.Type_Drone.ToList();

                // Применяем логику подсказок в памяти
                var roleItems = roles.Select(role => new Type_Drone_Item
                {
                    Name = role.name, // Используем имя
                    Tooltip = FormatText(role.descript_type) // Теперь правильно получаем описание
                }).ToList();

                // Теперь устанавливаем источник данных для ComboBox с объектами Type_Drone_Item
                tb_type_drone.ItemsSource = roleItems;
            }
        }
        


        private void LoadComboBoxData_1()  // тип контроллера
        {
            using (var context = new FPV_dronEntities1())
            {
                var roles = context.Control_Type.ToList();

                // Применяем логику подсказок в памяти
                var roleItems = roles.Select(role => new Type_Control_Item
                {
                    Name = role.name, // Используем имя
                    Tooltip = FormatText(role.descript_type) // Теперь правильно получаем описание
                }).ToList();

                // Теперь устанавливаем источник данных для ComboBox с объектами Type_Drone_Item
                tb_type_control.ItemsSource = roleItems;
            }
        }

       

        public string FormatText(string input)
        {
            // Разбиваем входной текст на слова
            var words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Используем StringBuilder для построения итогового текста
            StringBuilder formattedText = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                // Добавляем слово
                formattedText.Append(words[i] + " ");

                // Каждые 4 слова добавляем новую строку
                if ((i + 1) % 4 == 0)
                {
                    formattedText.AppendLine(); // Переход на новую строку
                }
            }

            return formattedText.ToString().Trim(); // Возвращаем итоговый текст без лишних пробелов
        } // здесь разбитие текста по 4 слова

        private void LoadData(string droneName1)
        {

              var query = from drone in dbContext.Drone
                          join flight in dbContext.Flight on drone.id_drone equals flight.id_drone
                          join acsel in dbContext.Acsel on flight.id_flight equals acsel.id_flight
                          join droneUser in dbContext.Drone_User on drone.id_drone equals droneUser.id_drone
                          join user in dbContext.Users on droneUser.id_user equals user.id_user
                          where drone.name_model == droneName1

                          orderby drone.name_model
                          select new FlightData
                          {
                              ModelName = drone.name_model,
                              IdFlight = flight.id_flight,
                              UserName = user.name,
                              UserSurname = user.surname,
                              TimeStart = acsel.time_start.ToString(),
                              X = (float)acsel.x,
                              Y = (float)acsel.y,
                              Z = (float)acsel.z
                          };

            var results = query.ToList();
              
            grid_tb.ItemsSource = results;
            recordCount = results.Count;
            MessageBox.Show($"Количество записей: {recordCount}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void LoadItems(Users user4, string droneName, bool iz_admin)
        {
            // Очищаем поля перед загрузкой данных
            tb_name_model.Text = "";
            tb_type_drone.Text = "";
            tb_time_flight.Text = "";
            tb_type_control.Text = "";
            tb_max_payload.Text = "";
            tb_describe.Text = "";

            if (iz_admin == false) //обычный юзер
            {
                // Задаем параметры запроса
                string droneName1 = droneName; // Название дрона
                long userId = user4.id_user; // ID пользователя


                var query = from drone in dbContext.Drone
                            join droneUser in dbContext.Drone_User on drone.id_drone equals droneUser.id_drone
                            where drone.name_model == droneName && droneUser.id_user == user4.id_user // Добавляем проверку на пользователя
                            select drone;

                var result = query.FirstOrDefault();

                if (result != null)
                {

                    LoadData(droneName1);

                    //   MessageBox.Show($"Searching for User ID: {user4.id_user}, Drone Name: ={droneName}=");
                    // Выполняем запрос к базе данных
                    var query2 = from drone in dbContext.Drone
                                 join droneUser in dbContext.Drone_User on drone.id_drone equals droneUser.id_drone
                                 join typeDrone in dbContext.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                                 join controlType in dbContext.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                                 join user in dbContext.Users on droneUser.id_user equals user.id_user
                                 where user.id_user == userId && drone.name_model == droneName1
                                 select new
                                 {

                                     drone.id_drone,
                                     drone.name_model,
                                     Type = drone.id_type_drone, //typeDrone.id_type_drone,
                                     drone.time_flight,
                                     Controller = drone.id_type_control, //controlType.name,
                                     drone.max_payload,
                                     drone.describe,
                                     UserName = user.name,
                                     UserSurname = user.surname,
                                     id_user_dron = droneUser.id_drone_user
                                 };

                    var result2 = query2.FirstOrDefault(); // Получаем первый результат
                    drone_id_ = result2.id_drone;
                    old_name = result2.name_model;
                    drone_user_ = result2.id_user_dron;
                    //      MessageBox.Show(result.name_model);

                    if (result != null)
                    {
                        // Заполняем текстовые поля данными из результата

                        tb_name_model.Text = result2.name_model.ToString();
                        tb_type_drone.SelectedIndex = (int)(result2.Type) - 1;
                        tb_time_flight.Text = result2.time_flight.ToString();
                        tb_type_control.SelectedIndex = (int)(result2.Controller) - 1;
                        tb_max_payload.Text = result2.max_payload.ToString();
                        tb_describe.Text = result2.describe.ToString();
                    }
                }
                else
                {
                    MessageBox.Show("Дрон не найден для указанного пользователя.");

                }
            }

            if (iz_admin == true) {
                // Задаем параметры запроса
                string droneName1 = droneName; // Название дрона
                long userId = user4.id_user; // ID пользователя


                var query = from drone in dbContext.Drone
                            join droneUser in dbContext.Drone_User on drone.id_drone equals droneUser.id_drone
                            where drone.name_model == droneName // Добавляем проверку на пользователя
                            select drone;

                var result = query.FirstOrDefault();

                if (result != null)
                {

                    LoadData(droneName1);

                    //   MessageBox.Show($"Searching for User ID: {user4.id_user}, Drone Name: ={droneName}=");
                    // Выполняем запрос к базе данных
                    var query2 = from drone in dbContext.Drone
                                 join droneUser in dbContext.Drone_User on drone.id_drone equals droneUser.id_drone
                                 join typeDrone in dbContext.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                                 join controlType in dbContext.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                                 join user in dbContext.Users on droneUser.id_user equals user.id_user
                                 where drone.name_model == droneName1
                                 select new
                                 {

                                     drone.id_drone,
                                     drone.name_model,
                                     Type = drone.id_type_drone, //typeDrone.id_type_drone,
                                     drone.time_flight,
                                     Controller = drone.id_type_control, //controlType.name,
                                     drone.max_payload,
                                     drone.describe,
                                     UserName = user.name,
                                     UserSurname = user.surname,
                                     id_user_dron = droneUser.id_drone_user
                                 };

                    var result2 = query2.FirstOrDefault(); // Получаем первый результат
                    drone_id_ = result2.id_drone;
                    old_name = result2.name_model;
                    drone_user_ = result2.id_user_dron;
                    //      MessageBox.Show(result.name_model);

                    if (result != null)
                    {
                        // Заполняем текстовые поля данными из результата

                        tb_name_model.Text = result2.name_model.ToString();
                        tb_type_drone.SelectedIndex = (int)(result2.Type) - 1;
                        tb_time_flight.Text = result2.time_flight.ToString();
                        tb_type_control.SelectedIndex = (int)(result2.Controller) - 1;
                        tb_max_payload.Text = result2.max_payload.ToString();
                        tb_describe.Text = result2.describe.ToString();
                    }
                }
                else
                {
                    MessageBox.Show("Дрон не найден для указанного пользователя.");

                }
            }
        }
       
        private void NumberInputTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+"); // разрешаем только цифры
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TimeTextBox_PreviewTextInput_time(object sender, TextCompositionEventArgs e)
        {
            // Разрешить только ввод цифр и двоеточий
            e.Handled = !Regex.IsMatch(e.Text, @"[\d:]");
        }

        private void TimeTextBox_TextChanged_time(object sender, TextChangedEventArgs e)
        {
            btnchange.IsEnabled = true;

            try
            {

                if (tb_time_flight != null && tb_time_flight.Text.Length >= 2 && tb_time_flight.Text[2] != ':')
                {
                    tb_time_flight.Text = tb_time_flight.Text.Insert(2, ":");
                    tb_time_flight.SelectionStart = tb_time_flight.Text.Length; // Курсор в конец
                }

                if (tb_time_flight.Text.Length >= 5 && tb_time_flight.Text[5] != ':')
                {
                    tb_time_flight.Text = tb_time_flight.Text.Insert(5, ":");
                    tb_time_flight.SelectionStart = tb_time_flight.Text.Length; // Курсор в конец
                }
            }
            catch// (Exception ex)
            {
                // MessageBox.Show($" шип {ex}");
            }
        }

        private void InputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnchange.IsEnabled = true;
        }

        private void InputField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnchange.IsEnabled = true;
            try
            {
                if (sender is ComboBox comboBox)
                {
                    // Проверка для tb_type_control
                    if (comboBox == tb_type_control)
                    {                  
                        if (comboBox.SelectedItem is Type_Control_Item selectedControl) // 1.1
                        {
                            // Устанавливаем текст с описанием для контроллера
                            info_type_controller_1.Text = selectedControl.Tooltip;                          
                        }

                        // Если tb_type_drone также изменяется, проверьте это отдельно
                        if (tb_type_drone.SelectedItem is Type_Drone_Item selectedDrone) // 1.2
                        {
                            // Обновите информацию по дрону
                            info_type_dron_1.Text = selectedDrone.Tooltip;                           
                        }
                    }
                    // Проверка для tb_type_drone
                    else if (comboBox == tb_type_drone)
                    {                       

                        if (comboBox.SelectedItem is Type_Drone_Item selectedDrone) // 2.1
                        {
                            // Устанавливаем текст с описанием для дронов
                            info_type_dron_1.Text = selectedDrone.Tooltip;                        
                        }

                        // Если tb_type_control также изменяется, проверьте это отдельно
                        if (tb_type_control.SelectedItem is Type_Control_Item selectedControl) // 2.2
                        {                          
                            // Устанавливаем текст с описанием для контроллера
                            info_type_controller_1.Text = selectedControl.Tooltip;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}","e x p e c t i o n ");
            }
        }

 


        private void UpdateDroneData(long droneId, string modelName, int typeDroneId, TimeSpan timeFlight, int controlTypeId, int maxPayload, string description)
        {
            try
            {
                var drone = dbContext.Drone.Find(droneId); // Найдите дрон по ID

                if (drone != null)
                {
                    // Обновляем свойства
                    drone.name_model = modelName;
                    drone.id_type_drone = typeDroneId;
                    drone.time_flight = timeFlight;
                    drone.id_type_control = controlTypeId;
                    drone.max_payload = maxPayload;
                    drone.describe = description;

                    dbContext.SaveChanges(); // Сохраните изменения в базе данных
                    MessageBox.Show("Данные дрона успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new user(user4));
                }
                else
                {
                    MessageBox.Show("Дрон не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void btn_Update_Click_1(object sender, RoutedEventArgs e)
        {
            TimeSpan timeFlight = TimeSpan.Zero;
            try
            {
                // Здравствуйте! :)
               // long droneId = GetDroneIdForCurrentUser(); // Получите ID дрона, который нужно изменить.

                // Проверка заполненности полей
                if (!string.IsNullOrEmpty(tb_name_model.Text) || tb_type_drone.SelectedItem != null || tb_type_control.SelectedItem != null || !string.IsNullOrEmpty(tb_max_payload.Text.Trim()))
                {
                    var modelName = tb_name_model.Text.Trim();
                    var typeDrone = tb_type_drone.SelectedIndex + 1; // Нумерация в базе начинается с 1
                    var controlType = tb_type_control.SelectedIndex + 1; // Нумерация в базе начинается с 1
                    var description = tb_describe.Text.Trim();

                    // Проверки
                    if (!string.IsNullOrEmpty(modelName) && modelName.Length >= 3)
                    {
                        if (!CheckModelNameExists(modelName))
                        {
                            if (tb_type_drone.SelectedItem != null)
                            {
                                if (tb_type_control.SelectedItem != null)
                                {
                                    if (!string.IsNullOrEmpty(tb_max_payload.Text))
                                    {
                                        var maxPayloadText = int.Parse(tb_max_payload.Text.Trim());

                                        // Проверка формата времени
                                        if (TimeSpan.TryParse(tb_time_flight.Text.Trim(), out timeFlight) && (tb_time_flight.Text.Length == 8))
                                        {
                                            // Вызываем метод для обновления данных
                                            UpdateDroneData(drone_id_, modelName, typeDrone, timeFlight, controlType, maxPayloadText, description);
                                        }
                                        else
                                        {
                                            tb_time_flight.Text = "";
                                            MessageBox.Show("Ошибка: Время должно быть в формате hh:mm:ss! Максимальное значение 23:59:59.", "Ошибка");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Ошибка: Введите нагрузку!", "Ошибка");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка: Вы не выбрали тип контроллера!", "Ошибка");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Ошибка: Вы не выбрали тип дрона!", "Ошибка");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: Такая модель уже существует!", "Ошибка");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: Название должно быть длиннее 3-х символов! Введите название модели!", "Ошибка");
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: заполните поля!", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении операции: {ex.Message}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private bool CheckModelNameExists(string modelName)
        {
            if (old_name == modelName) {
                return false;
            }
            else { 
                using (var dbContext = new FPV_dronEntities1())
                {
                    return dbContext.Drone.Where(p => p.name_model == modelName).Any();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите продолжить?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Выполнение действий в зависимости от выбора пользователя
            if (result == MessageBoxResult.Yes)
            {
                // Действие, которое нужно выполнить, если пользователь выбрал "Да"
                MessageBox.Show("Хорошо, удаляю.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                   // MessageBox.Show("Хорошо in try");
                    // Проверьте, существует ли дрон
                    var drone = dbContext.Drone.Find(drone_id_);
                    var drone_us = dbContext.Drone_User.Find(drone_user_);



                    if (drone != null && drone_us != null)
                    {
                      //  MessageBox.Show("drone != null && drone_us != null");
                      //  MessageBox.Show($"recordCount = {recordCount }");
                        if (recordCount > 0)
                        {
                          //  MessageBox.Show("Я не могу удалить! У дрона есть записи о полетах и показания датчиков!");
                            MessageBoxResult res_mes = MessageBox.Show("Вы точно хотите удалить дрон, у которого есть полеты?", "Сообщение об удалении", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                            if (res_mes == MessageBoxResult.Yes)
                            {
                                
                             //   MessageBox.Show("Start searchi flights");
                                var id_flighthsss = dbContext.Flight
                                    .Where(di => di.id_user == user4.id_user && di.id_drone == drone.id_drone)
                                    .Select(di => di.id_flight)
                                    .ToList();

                                

                                List<Acsel> recordsToRemove1 = new List<Acsel>(); // лист удаления акселя

                                foreach (long id_kl in id_flighthsss)
                                {
                                    Console.WriteLine($"id_kl = {id_kl}");
                                    var items = dbContext.Acsel
                                     .Where(du => du.id_flight == id_kl)
                                     .ToList();
                                    recordsToRemove1.AddRange(items);
                                }

                                List<Flight> recordsToRemove = new List<Flight>(); // лист удаления полетов

                                foreach (long id_kl in id_flighthsss)
                                {
                                    Console.WriteLine($"id_kl = {id_kl}");
                                    var items = dbContext.Flight
                                     .Where(du => du.id_flight == id_kl)
                                     .ToList();
                                    recordsToRemove.AddRange(items);
                                }

                              //  MessageBox.Show("END searchi flights");
                                try
                                {
                                    MessageBox.Show("Удаляю данные о акселерометре.....");
                                    dbContext.Acsel.RemoveRange(recordsToRemove1);
                                    MessageBox.Show("Удаляю полет....");
                                    dbContext.Flight.RemoveRange(recordsToRemove);
                                }
                                catch(Exception ex) { 
                                    MessageBox.Show($"ex = {ex}"); 
                                }
                                dbContext.SaveChanges(); // Сохраните изменения в базе данных
                                
                                dbContext.Drone_User.Remove(drone_us);
                                dbContext.SaveChanges();
                                MessageBox.Show("Удаялю связь дрона.....");
                                // Удалите дрон
                                dbContext.Drone.Remove(drone);
                                dbContext.SaveChanges();

                               // MessageBox.Show("Дрон успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                                MessageBox.Show("Удаление успешно завершено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                                NavigationService.Navigate(new user(user4));
                             
                            }

                            else
                            {
                                MessageBox.Show("Отмена удаления");
                            }

                        }
                       
                        else
                        {
                              //MessageBox.Show("че-то не так ..... ушли в удаление.....");
                              // Удалите связь дрона
                              dbContext.Drone_User.Remove(drone_us);
                              dbContext.SaveChanges();
                              MessageBox.Show("Удаление связи дрона.....");
                              // Удалите дрон
                              dbContext.Drone.Remove(drone);
                              dbContext.SaveChanges();

                              MessageBox.Show("Дрон успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                              NavigationService.Navigate(new user(user4));
                        }

                    }
                    else
                    {
                        MessageBox.Show("Дрон не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else if (result == MessageBoxResult.No)
            {
                // Действие, которое нужно выполнить, если пользователь выбрал "Нет"
                MessageBox.Show("Удаление отменено ", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                // Здесь можно поместить код для отмены действия или другую логику
            }
        }

    }

    public class Type_Drone_Item
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
    }

    public class Type_Control_Item
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
    }


    public class FlightData
    {
        public string ModelName { get; set; }
        public long IdFlight { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string TimeStart { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}




/*    var query = from type_drone in dbContext.Type_Drone  // вывод типов дронов

    select new
    {
        id_type_drone = type_drone.id_type_drone,
        name = type_drone.name,
        descript_typeame = type_drone.descript_type
    };*/
