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
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    /// 

    public partial class regist : Page
    {

        public regist()
        {
            InitializeComponent();
            LoadComboBoxData();
        }
        public List<RoleItem> RoleItems { get; set; }
        private void LoadComboBoxData()
        {
            using (var context = new FPV_dronEntities1())
            {
                // Получаем список ролей из базы данных
                var roles = context.Role.Select(p => p.name).ToList();

                // Применяем логику подсказок в памяти
                var roleItems = roles.Select(role => new RoleItem
                {
                    Name = role,
                    Tooltip = GetTooltipForRole(role)
                }).ToList();

                // Устанавливаем источник данных для ComboBox
                list_role.ItemsSource = roleItems;
            }
        }

        private string GetTooltipForRole(string roleName)
        {
            
            switch (roleName)
                
            {
                case "Администратор":
                    return "Управляет системой, ведет отчетность" +
                        "\n о дронах и сертификатах, редактирует " +
                        "\n данные пользователей.";
                case "Пользователь":
                    return "Имеет доступ к основным функциям.";
                case "Модератор":
                    return "Следит за соблюдением правил, контролирует" +
                        "\n действия пользователей и обеспечивает порядок." +
                        "\n Права пользователя";
                case "Инструктор":
                    return "Обучает участников работе с дронами, " +
                        "\n фиксирует информацию о дронах " +
                        "\n и передает отчеты администратору.";
                case "Участник":
                    return "Обучающийся пользователь, " +
                        "\n просматривает дроны, записывает полеты" +
                        "\n и может менять информацию о дроне " +
                        "\n у администратора.";
                case "Подписчик":
                    return "Имеет доступ к общим данным" +
                        "\n о дронах и полетах, но не может" +
                        "\n вносить изменения.";
                case "Гость":
                    return "Просматривает общую информацию" +
                        "\n о приложении и дронах " +
                        "\n без регистрации или авторизации.";
                case "Суперадминистратор":
                    return "Обладает максимальными правами, " +
                        "\n управляет всем приложением и " +
                        "\n пользователями, имеет полный " +
                        "\n доступ к отчетности.";
                case "Менеджер":
                    return "Координирует работу с дронами, " +
                        "\n следит за расписанием и взаимодействует" +
                        "\n с участниками и инструкторами.";
                // Добавьте другие роли и их подсказки
                default:
                    return "Описание роли отсутствует.";
            }
        }

        private void btnSign_Click(object sender, RoutedEventArgs e)
        {
            string name = "", surname = "", addres = "", city = "", login = "", password = "";
            DateTime data_bithday = DateTime.Now;
            try
            {
                name = tb_name.Text;
                surname = tb_surname.Text;
                data_bithday = data_bithday1.SelectedDate.Value;
                addres = tb_addres.Text;
                city = tb_city.Text;
                login = tb_login.Text;
                password = tb_password.Text;
            }
            catch {
            }
           
                if (!string.IsNullOrEmpty(name) && name.Length >= 3)
                {
                    if (!string.IsNullOrEmpty(surname))
                    {
                        if (data_bithday1.SelectedDate.HasValue)
                        {
                            if (!string.IsNullOrEmpty(login))
                            {
                                if (password.Length >= 5)
                                {
                                    if (list_role.SelectedItem != null)
                                    {
                                        if (!CheckUserLoginExists(login))
                                        {
                                            try
                                            {
                                            var selectedRoleItem = list_role.SelectedItem as RoleItem;
                                            string[] validRoles = { "Администратор", "Редактор", "Суперадминистратор", "Менеджер" }; // Замените на ваши значения
                                           // MessageBox.Show($"{selectedRoleItem}");
                                            Console.WriteLine(selectedRoleItem.Name);

                                            if (validRoles.Contains(selectedRoleItem.Name))
                                            {
                                                    MessageBox_Dialog dialog = new MessageBox_Dialog();
                                                    dialog.Top = 250;
                                                    dialog.Left = 500;
                                                    bool? result = dialog.ShowDialog(); // Показываем диалог

                                                    if (result == true)
                                                    {
                                                        SaveUser(login, password, name, surname, addres, city, data_bithday);
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show("Пароль неверный или не введен! Выберите другую роль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                    }
                                                }
                                                else 
                                                {
                                                    SaveUser(login, password, name, surname, addres, city, data_bithday);
                                                }
                                            }
                                            catch { MessageBox.Show("Ошибка открытия диалога!"); }

                                        }
                                        else
                                        {
                                            MessageBox.Show("Пользователь с таким логином уже существует");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Пожалуйста, выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Пароль должен содержать не менее 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Логин не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пожалуйста, выберите дату рождения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Фамилия не может быть пустой", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Имя должно содержать не менее 3 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }



        private void SaveUser(string login, string password, string name, string surname, string addres , string city, DateTime data_bithday)
        {
            try
            {
                var dbContext = new FPV_dronEntities1();
                var user = new Users();
                var logins = new Login();
                logins.login1 = login;
                logins.password = password;

                dbContext.Login.Add(logins);

                var id_log = logins.id_login;

                //login_1 = dbContext.Login.Where(p => p.login1 == Login).FirstOrDefault();

                user.name = name;
                user.surname = surname;
                user.address = addres;
                user.city = city;
                user.bithday = data_bithday;
                user.city = city;
                user.id_role = list_role.SelectedIndex + 1;
                user.id_login = id_log;

                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                MessageBox.Show("Пользователь успешно зарегистрирован","", MessageBoxButton.OK, MessageBoxImage.Information);
                

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try {
                Window_login login_ = new Window_login();
                login_.Top = 100;
                login_.Left = 250;
                login_.Show();
                Window.GetWindow(this).Close();
            }
            catch
            {
                MessageBox.Show("УПС не могу закрыть это окно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CheckUserLoginExists(string login)
        {
            using (var dbContext = new FPV_dronEntities1())
            {
                return dbContext.Login.Where(p => p.login1 == login).Any();
            }
        }
    }

    public class RoleItem
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
    }


}

