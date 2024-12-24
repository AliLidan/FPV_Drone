using System;
using System.Collections.Generic;
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
    public partial class new_user_drone : Page
    {
        Users user4;
        public new_user_drone(Users user2)
        {
            InitializeComponent();
            LoadComboBoxData();
            LoadComboBoxData_1();

            if (user2 != null)
            {
                user4 = user2;
                MessageBox.Show($"Добро пожаловать! Создание дрона для {user4.name}, {user4.surname}", "Window_welcome!", MessageBoxButton.OKCancel);
               // LoadItems(user2, name_mod);
            }
            else
            {
                user2 = new Users
                {
                    id_user = 12, // Проверьте, что пользователь с этим ID есть в базе
                };
              //  LoadItems(user4, name_mod);
            }
        }


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



        private void Parse_Valumes(Users user4, string name_model, long id_type_drone, TimeSpan time_flight, long id_type_control, int max_payload, string describe) {
            try
            {
                var dbContext = new FPV_dronEntities1();
                var drone = new Drone();
                var drone_user = new Drone_User();


                drone.name_model = name_model;
                drone.id_type_drone = tb_type_drone.SelectedIndex + 1;
                drone.time_flight = time_flight;
                drone.id_type_control = tb_type_control.SelectedIndex + 1;
                drone.max_payload = max_payload;
                drone.describe = describe;

                dbContext.Drone.Add(drone);
                dbContext.SaveChanges();
                MessageBox.Show("Дрон успешно создан");

                // var id_user_of_drone = dbContext.Drone.Where(p => p.name_model == drone.name_model && p.name_model == drone.name_model).FirstOrDefault();
                try
                {
                    long id_ = drone.id_drone;
                    long id_ur = user4.id_user;
                 //   MessageBox.Show($"Вот его id =  {id_}");
                 //   MessageBox.Show($"Вот id user =  {id_ur}");

                    drone_user.id_drone = id_;
                    drone_user.id_user = id_ur;

                    dbContext.Drone_User.Add(drone_user);
                    dbContext.SaveChanges();
                    MessageBox.Show("Связь дрона и пользователя создана!");
                }
                catch (Exception ex) {
                    //     MessageBox.Show($"Ля твоя ошибка парсинга.11 {ex}");
                    MessageBox.Show($" Введите данные!{ex}");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            NavigationService.Navigate(new user(user4));
        }

        private void ClearFields()
        {
            tb_name_model.Clear();
            tb_time_flight.Clear();
            tb_max_payload.Clear();
            tb_describe.Clear();
            tb_type_drone.SelectedIndex = -1;
            tb_type_control.SelectedIndex = -1;
        }

        private void btnSafe_Click_1(object sender, RoutedEventArgs e)
        {

            
                TimeSpan timeFlight = TimeSpan.Zero;
                try
                {

                    // Здесь вы можете попробовать распарсить время
                    if (!string.IsNullOrEmpty(tb_name_model.Text) || tb_type_drone.SelectedItem != null || tb_type_control.SelectedItem != null  || !string.IsNullOrEmpty(tb_max_payload.Text.Trim()))
                    {
                        var modelName = tb_name_model.Text.Trim();
                        var typeDrone = tb_type_drone.SelectedIndex + 1;
                        var controlType = tb_type_control.SelectedIndex + 1;
                        var description = tb_describe.Text.Trim();
                    if (!string.IsNullOrEmpty(modelName) && modelName.Length >= 3) // ПОКУМЕКАЙ НАД ПОРЯДКОМ!!! А ТО ВСЕ ВРЕМЯ ЛЕЗИТ ОШИБКА ДАТЫ....
                        {
                            if (!CheckModelNameExists(modelName))
                            {
                                if (tb_type_drone.SelectedItem != null)
                                {
                                    if (tb_type_control.SelectedItem != null)
                                    {
                                        if (!string.IsNullOrEmpty(tb_max_payload.Text.ToString()) )
                                        {
                                            var maxPayloadText = int.Parse(tb_max_payload.Text.Trim());
                                            
                                            if (TimeSpan.TryParse(tb_time_flight.Text.Trim(), out timeFlight) && (tb_time_flight.Text.Length == 8))
                                            {
                                                Parse_Valumes(user4, modelName, typeDrone, timeFlight, controlType, maxPayloadText, description);
                                            }
                                            else
                                            {
                                                tb_time_flight.Text = "";
                                                MessageBox.Show("Ошибка: Время должно быть в фориате hh:mm:ss! т.е максимальное значение 23:59:59", "Ошибка");
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show($"Ошибка: Введите нагрузку!", "Ошибка");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show($"Ошибка: Вы не выбрали тип контроллера!", "Ошибка");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show($"Ошибка: Вы не выбрали тип дрона!", "Ошибка");
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Ошибка: Такая модель уже есть!", "Ошибка");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка: Название должно быть длиннее 3-ех символов! Введите название модели!", "Ошибка");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: заполните поля!", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ля твоя ошибка парсинга.11 {ex}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // ClearFields(); // Если необходимо
            }
           
        private bool CheckModelNameExists(string modelName)
        {
            using (var dbContext = new FPV_dronEntities1())
            {
                return dbContext.Drone.Where(p => p.name_model == modelName).Any();
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

        private void InputField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                MessageBox.Show($"{ex}", "e x p e c t i o n ");
            }
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


// + СДЕЛАЙ РЕДАКТ И УДАЛЕНИЕ
// + А ПОТОМ ОБНОВЛЕНИЕ СТРАНИЧКИ И 
//   ЗАПИСЬ С АРДУИНО
//   А ПОТОМ БЕЗ КОНФЛИКТНУЮ ЗАПИСЬ IF_FLIGHTS (если вышли за диапозон id в flights)
// + НОРМ КНОПКИ (НА drone_users)!!!
// + ОРГАНИЗОВАТЬ ВВОД НОРМ ДАТЫ ЮЕЗ 99:99:99
// + ПОКУМЕКАЙ НАД ПОРЯДКОМ!!! А ТО ВСЕ ВРЕМЯ ЛЕЗИТ ОШИБКА ДАТЫ....
//   можно сортировку бахнуть, поиск... 
// +-  графики пользования :))))))



/*    
 *     TimeSpan timeFlight = TimeSpan.Zero;
try
{
    var modelName = tb_name_model.Text.Trim();
    var typeDrone = tb_type_drone.SelectedIndex + 1;
        timeFlight = TimeSpan.Parse(tb_time_flight.Text.Trim());
    var controlType = tb_type_control.SelectedIndex + 1;
    var maxPayloadText = int.Parse(tb_max_payload.Text.Trim());
    var description = tb_describe.Text.Trim();

    try {
        if (TimeSpan.TryParse(tb_time_flight.ToString(), out timeFlight))
        {
            MessageBox.Show("Защитай");
        }
        else {
            MessageBox.Show("Иди нахуй");
        }
        }
    catch (FormatException ex)
    {
        MessageBox.Show($"Введите время в формате HH:mm:ss {ex}");
    }


    if (!string.IsNullOrEmpty(modelName) || modelName.Length >= 3)
    {
        if (!CheckModelNameExists(modelName))
        {
            if (tb_type_drone.SelectedItem != null)
            {
                if (TimeSpan.TryParse(tb_time_flight.ToString(), out timeFlight))
                {
                    if (tb_type_control.SelectedItem != null)
                    {
                        if (!string.IsNullOrEmpty(maxPayloadText.ToString()) || maxPayloadText >= 0)
                        {
                            Parse_Valumes(modelName, typeDrone, timeFlight, controlType, maxPayloadText, description);
                        }
                        else {
                            MessageBox.Show($"Ошибка: нагрузка должна быть положительным числом или 0! Введите нагрузку!", "Ошибка");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка: Вы не выбрали тип контроллера!", "Ошибка");
                    }
                }
                else
                {
                    MessageBox.Show($"Ошибка: Введите время в формате: hh:mm:ss!", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show($"Ошибка: Вы не выбрали тип дрона!", "Ошибка");
            }
        }

        else
        {
            MessageBox.Show($"Ошибка: Такая модель уже есть!", "Ошибка");
        }

    }
    else
    {
        MessageBox.Show($"Ошибка: Название должно быть длиннее 3-ех символов! Введите название модели!", "Ошибка");
    }
}
catch (Exception ex)
{
    MessageBox.Show($"Ля твоя ошибка парсинга.11 {ex}", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);

}


//  ClearFields();
}*/

