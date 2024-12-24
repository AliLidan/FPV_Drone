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

namespace FPV_Drone
{
    public partial class Window_login : Window
    {
        FPV_dronEntities1 dbContext = new FPV_dronEntities1();
        Login login_1 = new Login();
        Users user = new Users();

        public Window_login()
        {
            InitializeComponent();
            tb_login.Text = "user2";
            tb_password.Password = "23456789";
        }

        private void guest_Click(object sender, RoutedEventArgs e) // гость
        {
            Window_admin page_ = new Window_admin(new guest());
            page_.Top = 100;
            page_.Left = 250;
            page_.Show(); // Открывает главное окно
            this.Close();
        }

        private void regist_Click(object sender, RoutedEventArgs e) // регистрация
        {
            Window_admin page_ = new Window_admin(new regist());
            page_.Top = 100;
            page_.Left = 250;
            page_.Show(); // Открывает главное окно
            this.Close();
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(tb_login.Text) && !String.IsNullOrEmpty(tb_password.Password))
            {
                LoginUser();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль", "Предупреждение");

            }
        }


        private void LoginUser()
        {


            string Login = tb_login.Text.Trim();
            string pass = tb_password.Password.Trim();

            login_1 = dbContext.Login.Where(p => p.login1 == Login).FirstOrDefault();

            if (login_1 != null)
            {
                if (login_1?.password == pass)
                {
                    var login_user = login_1.id_login;
                    user = dbContext.Users.Where(p => p.id_login == login_user).FirstOrDefault();
                    LoadForm(user.id_role.ToString(), user);
                    tb_login.Text = "";
                }
                else
                {
                    MessageBox.Show("неверный пароль", "Предупреждение");
                    tb_password.Clear();
                }
            }
            else
            {
                MessageBox.Show("пользователя с логином '" + Login + "' не существует", "Предупреждение");
            }
        }


        private void LoadForm(string _role, Users user)
        {
            // админ - редактор - супераадмин - менеджер
            if (_role == "1" || _role == "5" || _role == "9" || _role == "10") {
                Window_admin page_ = new Window_admin(new admin(user));
                page_.Top = 100;
                page_.Left = 250;
                page_.Show(); // Открывает главное окно
                this.Close();
            }


            // пользователь - участник  - модератор
            if (_role == "2" || _role == "6" || _role == "3" )
            {
                Window_admin page_ = new Window_admin(new user(user));
                page_.Top = 100;
                page_.Left = 250;
                page_.Show(); // Открывает главное окно
                this.Close();
            }


            // просмоторщик - подпищик  - гость
            if (_role == "4" || _role == "7" || _role == "8")
            {
                Window_admin page_ = new Window_admin(new guest());
                page_.Top = 100;
                page_.Left = 250;
                page_.Show(); // Открывает главное окно
                this.Close();
            }

        }

        private void arduino_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Top = 100;
            mainWindow.Left = 250;
            mainWindow.Show(); // Открывает главное окно
            this.Close();
        }
    }
}
