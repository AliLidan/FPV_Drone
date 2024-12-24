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
using System.Windows.Navigation;
using System.IO.Ports;
using FPV_Drone.Model;

using System.Threading;
using System.IO;

using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Management;
using System.ComponentModel;

namespace FPV_Drone.Pages
{
    public partial class Arduino_write : Window
    {
        Users user4;

        SerialPort serialPort;
        private bool isPortOpen = false;
        private StreamWriter streamWriter;
        private StreamWriter streamWriter_2;
        private CancellationTokenSource cts;
        public int id_fly;
        public long id_1_;
        private long id_drone_;
        public event Action WindowClosed;
        public List<Arduino_Micro> ArduinoDevices { get; set; }
        bool is_open_port = false;

        public Arduino_write(Users user2)
        {
            InitializeComponent();
            // ArduinoDevices = new List<Arduino_Micro>();
            // LoadComPorts();
            user4 = user2;
            LoadItems(user4);
            LoadComPorts_1();

            try
            {
                serialPort = new SerialPort(name_of_Port());
                
                streamWriter = null;
                streamWriter_2 = null;
                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
            catch
            {
                MessageBox.Show("Подключенных портов нет!", "Нет соединения с Ардуино.");
            }

            this.Closing += Arduino_write_Closing; // Подписка на событие закрытия для кнопки "Полеты"
            Is_load_text();
        }

        // =============================   Без описания о блютузе (оч хорошо работает)  + (иницилизация)==============================================

        private void LoadComPorts_1() // список доступных COM-портов
        {

            // Получаем список доступных COM-портов
            string[] ports = SerialPort.GetPortNames();
            string selectedPort = GetConnectedPort_(); // Метод для определения, какой порт подключён

            // Заполняем ComboBox доступными портами
            port.Items.Clear(); // Очищаем старые элементы ComboBox
            foreach (string portName in ports)
            {
                // Если порт подключён, выделяем его
                if (portName == selectedPort)
                {
                    port.Items.Add($"{portName} (подключён)"); // Добавляем с пометкой
                }
                else
                {
                    port.Items.Add(portName); // Добавляем без пометки
                }
            }

            // Устанавливаем выбранный элемент ComboBox
            port.SelectedItem = selectedPort != null ? $"{selectedPort} (подключён)" : ports.Length > 0 ? ports[0] : null;
        }

        private string GetConnectedPort_()
        {
            // Здесь должна быть логика для определения, какой порт использует Arduino
            // Например, можно попробовать открыть каждый порт и проверить, есть ли на нём устройство
            foreach (string port in SerialPort.GetPortNames())
            {
                try
                {
                    using (SerialPort serialPort = new SerialPort(port))
                    {
                        serialPort.Open();
                        // Если устройство отвечает, возвращаем этот порт
                        // Здесь можно добавить дополнительные проверки, если нужно
                        return port; // Здесь лучше проверять более специфически
                    }
                }
                catch
                {
                    // Порт может быть занят или не отвечать, продолжаем
                }
            }
            return null; // Если ни один порт не подключён
        }



        // =============================   последний раз  (великолепно!) ==============================================

        private void GetPortDescription__2()
        {
            port.Items.Clear();
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e) =>
            {
                string[] ports = SerialPort.GetPortNames();
                var results = new List<string>();


                try
                {
                    foreach (var portName in ports)
                    {
                        string Description = "Нет описания"; // По умолчанию

                        if (GetConnectedPort__3(portName))
                        {
                            try
                            {
                                var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_SerialPort WHERE DeviceID = '{portName}'");
                                foreach (var port in searcher.Get())
                                {
                                    Description = port["Description"]?.ToString() ?? "Нет описания";
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка: {ex.Message}");
                            }

                            // Проверка на Bluetooth и установка сообщения соответствующим образом
                            if (Description.Contains("Bluetooth"))
                            {
                                results.Add($"{portName} (Bluetooth подключен)");
                            }
                            else
                            {
                                results.Add($"{portName} {Description} (подключен_доступен)");
                            }
                        }
                        else
                        {
                            results.Add($"{portName} (не подключен)");
                        }
                    }
                }
                catch (IOException ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }



                e.Result = results;
            };

            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                var Remember = port.SelectedItem;
                port.Items.Clear();
                var results = (List<string>)e.Result;

                foreach (var result in results)
                {
                    port.Items.Add(result);
                }
                port.SelectedItem = Remember;
            };

            backgroundWorker.RunWorkerAsync();
            port.Items.Clear();
        }


        private bool GetConnectedPort__3(String name_port)
        {
            // Здесь должна быть логика для определения, какой порт использует Arduino
            // Например, можно попробовать открыть каждый порт и проверить, есть ли на нём устройство

            try
            {
                using (SerialPort serialPort = new SerialPort(name_port))
                {
                    serialPort.Open();
                    // Если устройство отвечает, возвращаем этот порт
                    return true;
                }
            }
            catch
            {
                return false; // Если ни один порт не подключён
            }
        }


        private void myComboBox_DropDownOpened(object sender, EventArgs e)
        {
            //ArduinoDevices = new List<Arduino_Micro>();
            //LoadComPorts();
            // LoadComPorts_1();
            GetPortDescription__2();
        }


        private void Arduino_write_Closing(object sender, System.ComponentModel.CancelEventArgs e) // для кнопки "полеты"
        {
            WindowClosed?.Invoke(); // Вызываем событие при закрытии
        }

        public void Is_load_text() // закгрузка txt картинуи если есть файлы
        {
            string filePath = "C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\data.txt";
            if (File.Exists(filePath))
            {
                pic_txt.Visibility = Visibility.Visible;
            }
            else
            {

                pic_txt.Visibility = Visibility.Hidden;
            }

        }



        // =============================   С описанием о блютузе (оч медленно)  ==============================================
        /* private void  LoadComPorts()
         {
             string[] ports = SerialPort.GetPortNames(); // Получаем доступные COM-порты
             port.Items.Clear(); // Очищаем ComboBox перед заполнением


             foreach (var portic in ports)
             {
                 Arduino_Micro device = new Arduino_Micro
                 {
                     Name_port = portic,
                     Description = "бе-юе", // GetPortDescription(portic), // Получение описания порта
                     Connected_ = IsPortConnected(portic) // Проверка, подключено ли устройство
                 };

                 ArduinoDevices.Add(device); // Добавляем в список устройств

                 // Добавляем элемент в ComboBox
                 string displayText = $"{device.Name_port} ({device.Description})";
                 if (device.Connected_)
                 {
                     displayText += " - подключен";
                 }
                 port.Items.Add(displayText); // Добавляем текст в ComboBox
             }

             // Устанавливаем выбранный элемент ComboBox на первый доступный порт, если он есть
             if (port.Items.Count > 0)
             {
                 port.SelectedIndex = 0; // Выбираем первый доступный порт по умолчанию
             }
         }


         private string GetPortDescription(string portName)
         {
             try
             {
                 // Используем WMI для получения описания порта
                 var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_SerialPort WHERE DeviceID = '{portName}'");
                 foreach (var port in searcher.Get())
                 {
                     return port["Description"]?.ToString() ?? "Нет описания"; // Возвращаем описание или "Нет описания"
                 }
             }
             catch (Exception ex)
             {
                 // Обработка ошибок, если что-то пойдет не так
                 return $"Ошибка: {ex.Message}";
             }

             return "Неизвестный порт"; // Если не удалось получить описание
         }

         private bool IsPortConnected(string portName)
         {
             // Здесь можно добавить более сложную логику для проверки подключения Arduino
             return true; // Заглушка: просто возвращаем true
         }
         */


        /* // недочет просто порты и просто все ардуино и просто все подключены
        private void LoadComPorts()
        {
            // Получаем список доступных COM-портов
            var ports = SerialPort.GetPortNames();

            // Заполняем ComboBox доступными портами с информацией об устройствах
            port.Items.Clear();

            foreach (var porttic in ports)
            {
                string deviceInfo = GetDeviceInfo(porttic);

                // Проверяем, является ли устройство Arduino
                bool isArduinoConnected = IsArduinoDevice(porttic, deviceInfo);

                // Добавляем информацию о порте в ComboBox
                if (deviceInfo != null)
                {
                    port.Items.Add($"{porttic} ({deviceInfo}{(isArduinoConnected ? ", подключено" : "")})");
                }
                else
                {
                    port.Items.Add($"{porttic}{(isArduinoConnected ? " (подключено)" : "")}");
                }
            }

            // Устанавливаем выбранный элемент ComboBox на первый доступный порт, если он есть
            if (port.Items.Count > 0)
            {
                port.SelectedIndex = 0;
            }
        }

        private bool IsArduinoDevice(string portName, string deviceInfo)
        {
            // Проверьте, содержит ли информация о устройстве "Arduino"
            return deviceInfo != null && deviceInfo.IndexOf("Arduino", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string GetDeviceInfo(string portName)
        {
            // Заглушка для получения информации об устройстве
            // Возвращаем здесь "Arduino" для демонстрации
            return "Arduino"; // Или null, если информации нет
        }
        */

        /*    private void LoadComPorts()
            {
                // Получаем список доступных COM-портов
                var ports = SerialPort.GetPortNames();

                // Заполняем ComboBox доступными портами с информацией об устройствах
                port.Items.Clear();


                foreach (var porttic in ports)
                {
                    string deviceInfo = GetDeviceInfo(porttic);
                    if (deviceInfo != null)
                    {
                        port.Items.Add($"{porttic} ({deviceInfo})");
                    }
                    else
                    {
                        port.Items.Add(porttic); // Если не удалось получить информацию о устройстве
                    }
                }

                // Устанавливаем выбранный элемент ComboBox на первый доступный порт, если он есть

            }

            private string GetDeviceInfo(string portName)
            {
                // Используем WMI для получения информации о устройстве, подключенном к COM-порту
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_SerialPort WHERE DeviceID = '{portName}'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Description"]?.ToString(); // Возвращаем описание устройства
                    }
                }
                return null; // Если информация не найдена
            }

            private string GetConnectedPort()
            {
                // Здесь должна быть логика для определения, какой порт использует Arduino
                // Например, можно попробовать открыть каждый порт и проверить, есть ли на нём устройство
                foreach (string port in SerialPort.GetPortNames())
                {
                    try
                    {
                        using (SerialPort serialPort = new SerialPort(port))
                        {
                            serialPort.Open();
                            // Если устройство отвечает, возвращаем этот порт
                            // Здесь можно добавить дополнительные проверки, если нужно
                            return port; // Здесь лучше проверять более специфически
                        }
                    }
                    catch
                    {
                        // Порт может быть занят или не отвечать, продолжаем
                    }
                }
                return null; // Если ни один порт не подключён
            }*/









        private void LoadItems(Users user4) // загрузка названий дронов в combobox
        {
            using (var context = new FPV_dronEntities1())
            {
                // Очищаем ComboBox перед заполнением новыми данными
                drone_name_.Items.Clear();

                try
                {
                    var query = from drone in context.Drone
                                join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                                join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                                join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                                join user in context.Users on droneUser.id_user equals user.id_user
                                join role in context.Role on user.id_role equals role.id_role
                                where user.id_user == user4.id_user
                                orderby user.surname

                                select drone.name_model;
                    foreach (var modelName in query.ToList())
                    {
                        drone_name_.Items.Add(modelName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибочка", $"{ex}");
                }
            }
        }



        private void InitializeFileWriting()
        {
            string filePath = "data.txt"; // Укажите путь, если нужно
            streamWriter = new StreamWriter(filePath, true); // 'true' для добавления в конец файла
            Is_load_text();
        }
        private void Write_flight()
        {
            // чтение с flight.txt  какой полет ID
            using (StreamReader reader = new StreamReader("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt"))
            {
                string text = reader.ReadLine();
                Console.WriteLine("из блокнота=" + text + "=");
                id_fly = Convert.ToInt32(text);
            }
            Console.WriteLine(id_fly.ToString()); // 13
            id_fly = id_fly + 1;
            Console.WriteLine(id_fly.ToString()); //14

            GetLastFlightId();  // берем из базы полсдедний id

            using (StreamWriter writer = new StreamWriter("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt", false))
            {
                writer.WriteLineAsync(id_fly.ToString());
            }

            // 12 < 15
            // если в базе нет, то создаем новую запись в базе в таблице Flight
            if (id_1_ <= id_fly)
            {
                Console.WriteLine("Создаем новый полет");
                try
                {
                    var dbContext = new FPV_dronEntities1();
                    var fligth = new Flight();

                    fligth.id_drone = id_drone_;
                    fligth.id_user = user4.id_user;

                    dbContext.Flight.Add(fligth);
                    dbContext.SaveChanges();
                    MessageBox.Show("Новый полет успешно создан!");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            // если нет, то создаем новую запись в базе в таблице Flight
            else
            {
                MessageBox.Show("Используем старые записи", "Ошибка");
            }
        }

        private async void GetLastFlightId()
        {
            using (var context = new FPV_dronEntities1())
            {
                var lastFlightId = await context.Flight
                    .OrderByDescending(f => f.id_flight) // Сортируем по убыванию
                    .Select(f => f.id_flight)            // Выбираем только ID рейса
                    .FirstOrDefaultAsync();              // Получаем первый элемент или null

                if (lastFlightId != null)
                {
                    Console.WriteLine($"Последний ID рейса: {lastFlightId}");
                    id_1_ = lastFlightId;
                }
                else
                {
                    MessageBox.Show("Нет записей в таблице.");
                }
            }
        }

        private void OpenPort(string portName)
        {
            try
            {

                serialPort.Open();
                isPortOpen = true; // Порт открыт
                MessageBox.Show("Порт открыт успешно!");
                Console.WriteLine("Порт открыт успешно!");
                start.IsEnabled = false;
                open_file1.IsEnabled = false;
                kill_file1.IsEnabled = false;
                write_in_baz.IsEnabled = false;
                port.IsEnabled = false;
                drone_name_.IsEnabled = false;
                InitializeFileWriting();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии порта: {ex.Message}");
            }
            
        }


        private void ClosePort()
        {
            if (serialPort != null)
            {
                isPortOpen = false;
                serialPort.Close();
                MessageBox.Show("Порт закрыт!");
                data11.Text = string.Empty;
                start.IsEnabled = true;
                open_file1.IsEnabled = true;
                kill_file1.IsEnabled = true;
                write_in_baz.IsEnabled = true;
                port.IsEnabled = true;
                drone_name_.IsEnabled = true;

                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
            else
            {
                MessageBox.Show("Порт не подключен! Его нельзя выключить");
            }


        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine(); // Читайте данные (например, построчно)

                if (isPortOpen == true)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        data11.Text = data;
                    }));
                }
                if (streamWriter != null)
                {
                    data = Regex.Replace(data, @"\s+", " ");
                    data = data.Substring(0, data.Length - 1);
                    streamWriter.WriteLine(id_fly.ToString() + ", " + DateTime.Now.ToString() + ", " + data); // Записываем строку
                    streamWriter.Flush(); // Сохраняем данные немедленно
                }

            }
            catch (IOException ioEx)
            {
                // Обработка исключения ввода-вывода
                Console.WriteLine($"Ошибка ввода-вывода: {ioEx.Message}");
            }
            catch (InvalidOperationException invOpEx)
            {
                // Обработка исключения при некорректном обращении к порту
                Console.WriteLine($"Некорректная операция: {invOpEx.Message}");
            }
            catch (Exception ex)
            {
                // Ловим все остальные исключения
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }

        private void Stert_port(object sender, RoutedEventArgs e)
        {
            if (port.SelectedIndex != -1)
            {
                if (drone_name_.SelectedIndex != -1)
                {
                    string open = name_of_Port();
                    Console.WriteLine($"{is_open_port} == {open} === Stert_port");

                    if (is_open_port) // Убрал `!= false`, так как стоит проверять только true
                    {
                        Console.WriteLine($"{is_open_port} {open} == попали внутрль");
                        Write_flight();
                        OpenPort(name_of_Port()); // Изменил вызов на переменную open
                        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    }
                    else
                    {
                        MessageBox.Show("Данный порт не подключен!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Выберете дрон!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Выберете порт!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void End_port(object sender, RoutedEventArgs e)
        {
            ClosePort();
        }


        private void open_file(object sender, RoutedEventArgs e)
        {
            string path = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt"; // Укажите путь к вашему файлу
            LaunchFile(path);
            string path1 = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\flight.txt"; // Укажите путь к вашему файлу
            LaunchFile(path1);

        }

        private void LaunchFile(string filePath)
        {
            try
            {
                Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске файла: {ex.Message}");
            }
        }

        private void kill_file(object sender, RoutedEventArgs e)
        {
            string path = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt"; // Укажите ваш путь
            DeleteFile(path);
            Is_load_text();
        }

        public void DeleteFile(string filePath)
        {
            // Проверяем, существует ли файл
            if (File.Exists(filePath))
            {


                try
                {
                    File.Delete(filePath); // Удаляем файл
                    MessageBox.Show("Файл успешно удалён!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении файла: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Файл не найден!");
            }
        }

        private void btn_back(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void write_in_baz_click(object sender, RoutedEventArgs e) // запись в базу с блокнота
        {
            string filePath = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt";

            List<string> lines = new List<string>();
            if (File.Exists(filePath))
            {
                if (new FileInfo(filePath).Length != 0)
                {
                    try
                    {
                        using (StreamReader r = new StreamReader(filePath))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }
                        }

                        using (var context = new FPV_dronEntities1())
                        {
                            bool is_done = false;
                            foreach (var item in lines)
                            {
                                var values = item.Split(',');

                                if (values.Length >= 5)
                                {
                                    try
                                    {
                                        var acsel = new Acsel();
                                        acsel.id_flight = Convert.ToInt32(values[0].Trim());


                                        try
                                        {
                                            acsel.time_start = DateTime.Parse(values[1].Trim());
                                        }

                                        catch (FormatException ex)
                                        {
                                            MessageBox.Show($"Ошибка формата даты.... {ex.Message}");
                                        }
                                        acsel.x = float.Parse(values[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        acsel.y = float.Parse(values[3].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        acsel.z = float.Parse(values[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                                        // Подтверждаем правильность данных
                                        // MessageBox.Show($"Данные: ;{acsel.id_flight};, ;{acsel.time_start};, ;{acsel.x};, ;{acsel.y};, ;{acsel.z};");

                                        context.Acsel.Add(acsel);
                                    }
                                    catch (FormatException ex)
                                    {
                                        MessageBox.Show($"Ошибка формата: '{values[1]}' не является корректной датой. {ex.Message}");
                                        is_done = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Ошибка: {ex.Message}");
                                        is_done = true;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Чего-то не хватает в данных!");
                                    MessageBox.Show("Запись не удалась!");
                                    is_done = true;
                                    break;
                                }
                            }

                            if (is_done != true)
                            {
                                context.SaveChanges();
                                MessageBox.Show("Данные успешно записаны в базу данных.");
                            }
                        }
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show($"Произошла ошибка при чтении файла: {ex.Message}");
                        Console.WriteLine(ex.InnerException?.Message);
                        Console.WriteLine(ex);
                    }
                }
                else 
                {
                    MessageBox.Show("Файл пустой! Запись невозможна");
                }
            }
            else
            {
                MessageBox.Show("Нет файла data.txt");
            }
        }


        String name_of_Port()
        {
            String name = port.SelectedItem.ToString();
            string[] words = name.Split(' ');
            Console.WriteLine("1 = " + words[0]);
            Console.WriteLine("2 = " + words[1]);
            name = words[0];

            is_open_port = true; // Предполагаем, что порт открыт, двигаемся дальше
            int i = 0;
            foreach (string word in words)
            {
                Console.WriteLine($"[{i}] = {word}");
                if (word == "(не" || word == "подключен)")
                {
                    is_open_port = false; // Порт не подключен
                    Console.WriteLine($"{is_open_port} ==== цикл");
                    break; // Выходим из цикла, так как уже известно состояние
                }
                i++;
            }
            Console.WriteLine($"{is_open_port} ==== Закончили");
            return name;
        }

        public void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string drone_name = drone_name_.SelectedItem.ToString();
            using (var context = new FPV_dronEntities1())
            {

                try
                {
                    var query = from drone in context.Drone
                                join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                                join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                                join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                                join user in context.Users on droneUser.id_user equals user.id_user
                                join role in context.Role on user.id_role equals role.id_role
                                where user.id_user == user4.id_user
                                where drone.name_model == drone_name

                                select drone.id_drone;
                    var result = query.FirstOrDefault(); // Получаем первый результат
                    id_drone_ = result;
                    // вывод id
                    Console.WriteLine($"Итак, id дрона = {id_drone_} Информация о id");
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Ошибочка ID = ", $"{ex}");
                }
            }
        }

        public void btn_info(object sender, RoutedEventArgs e)
        {
            string filePath = @"C:\Users\User\Downloads\videoplayback (4).mp4";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // важно для открытия файла с его ассоциированным приложением
                });
            }
            catch (Exception ex)
            {
                // обработка исключений
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
            }
        }
    }


    public class Arduino_Micro
    {
        public string Name_port { get; set; }
        public string Description { get; set; }
        public bool Connected_ { get; set; }
    }
}



/*
 * using System;
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
using System.Windows.Navigation;
using System.IO.Ports;
using FPV_Drone.Model;

using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace FPV_Drone.Pages
{
   public partial class Arduino_write : Window
   {
       Users user4;

       SerialPort serialPort;
       private bool isPortOpen = false;
       private StreamWriter streamWriter;
       private StreamWriter streamWriter_2;
       private CancellationTokenSource cts;
       public int id_fly;
       public long id_1_;
       private long id_drone_;
       public event Action WindowClosed;

       public Arduino_write(Users user2)
       {
           InitializeComponent();
           LoadComPorts();
           user4 = user2;
           LoadItems(user4);

           try
           {
               serialPort = new SerialPort(name_of_Port());
               streamWriter = null;
               streamWriter_2 = null;
               serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
           }
           catch {
               MessageBox.Show("Подключенных портов нет!","Нет соединения с Ардуино.");
           }
           this.Closing += Arduino_write_Closing; // Подписка на событие закрытия для кнопки "Полеты"
           Is_load_text();
       }

       private void Arduino_write_Closing(object sender, System.ComponentModel.CancelEventArgs e) // для кнопки "полеты"
       {
           WindowClosed?.Invoke(); // Вызываем событие при закрытии
       }

       public void Is_load_text() // закгрузка txt картинуи если есть файлы
       {
           string filePath = "C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\data.txt";
           if (File.Exists(filePath))
           {
               pic_txt.Visibility = Visibility.Visible;
           }
           else {

               pic_txt.Visibility = Visibility.Hidden;
           }

       }


       private void LoadComPorts() // список доступных COM-портов
       {

           // Получаем список доступных COM-портов
           string[] ports = SerialPort.GetPortNames();
           string selectedPort = GetConnectedPort(); // Метод для определения, какой порт подключён

           // Заполняем ComboBox доступными портами
           port.Items.Clear(); // Очищаем старые элементы ComboBox
           foreach (string portName in ports)
           {
               // Если порт подключён, выделяем его
               if (portName == selectedPort)
               {
                   port.Items.Add($"{portName} (подключён)"); // Добавляем с пометкой
               }
               else
               {
                   port.Items.Add(portName); // Добавляем без пометки
               }
           }

           // Устанавливаем выбранный элемент ComboBox
           port.SelectedItem = selectedPort != null ? $"{selectedPort} (подключён)" : ports.Length > 0 ? ports[0] : null;
       }

       private string GetConnectedPort()
       {
           // Здесь должна быть логика для определения, какой порт использует Arduino
           // Например, можно попробовать открыть каждый порт и проверить, есть ли на нём устройство
           foreach (string port in SerialPort.GetPortNames())
           {
               try
               {
                   using (SerialPort serialPort = new SerialPort(port))
                   {
                       serialPort.Open();
                       // Если устройство отвечает, возвращаем этот порт
                       // Здесь можно добавить дополнительные проверки, если нужно
                       return port; // Здесь лучше проверять более специфически
                   }
               }
               catch
               {
                   // Порт может быть занят или не отвечать, продолжаем
               }
           }
           return null; // Если ни один порт не подключён
       }


       private void LoadItems(Users user4) // загрузка названий дронов в combobox
       {
           using (var context = new FPV_dronEntities1())
           {
               // Очищаем ComboBox перед заполнением новыми данными
               drone_name_.Items.Clear();

               try
               {
                   var query = from drone in context.Drone
                               join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                               join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                               join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                               join user in context.Users on droneUser.id_user equals user.id_user
                               join role in context.Role on user.id_role equals role.id_role
                               where user.id_user == user4.id_user
                               orderby user.surname

                               select drone.name_model;
                   foreach (var modelName in query.ToList())
                   {
                       drone_name_.Items.Add(modelName);
                   }

               }
               catch (Exception ex)
               {
                   MessageBox.Show("Ошибочка", $"{ex}");
               }
           }
       }



       private void InitializeFileWriting()
       {
           string filePath = "data.txt"; // Укажите путь, если нужно
           streamWriter = new StreamWriter(filePath, true); // 'true' для добавления в конец файла
           Is_load_text();
       }
       private void Write_flight()
       { 
           // чтение с flight.txt  какой полет ID
           using (StreamReader reader = new StreamReader("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt"))
           {
               string text = reader.ReadLine();
               Console.WriteLine("из блокнота="+text+"=");
               id_fly = Convert.ToInt32(text);
           }
           MessageBox.Show(id_fly.ToString()); // 13
           id_fly = id_fly + 1;
           MessageBox.Show(id_fly.ToString()); //14

           GetLastFlightId();  // берем из базы полсдедний id

           using (StreamWriter writer = new StreamWriter("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt", false))
           {
               writer.WriteLineAsync(id_fly.ToString());
           }

           // 12 < 15
           // если в базе нет, то создаем новую запись в базе в таблице Flight
           if (id_1_ <= id_fly)
           {
               MessageBox.Show("Создаем новый полет");
               try
               {
                   var dbContext = new FPV_dronEntities1();
                   var fligth = new Flight();

                   fligth.id_drone = id_drone_;
                   fligth.id_user = user4.id_user;

                   dbContext.Flight.Add(fligth);
                   dbContext.SaveChanges();
                   MessageBox.Show("Новый полет успешно создан!");

               }
               catch (Exception ex)
               {
                   MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
               }
           }
           // если нет, то создаем новую запись в базе в таблице Flight
           else
           {
               MessageBox.Show("Используем старые записи", "Ошибка" );
           }
       }

       private async void GetLastFlightId()
       {
           using (var context = new FPV_dronEntities1())
           {
               var lastFlightId = await context.Flight
                   .OrderByDescending(f => f.id_flight) // Сортируем по убыванию
                   .Select(f => f.id_flight)            // Выбираем только ID рейса
                   .FirstOrDefaultAsync();              // Получаем первый элемент или null

               if (lastFlightId != null)
               {
                   MessageBox.Show($"Последний ID рейса: {lastFlightId}");
                   id_1_ = lastFlightId;
               }
               else
               {
                   MessageBox.Show("Нет записей в таблице.");
               }
           }
       }

       private void OpenPort(string portName)
       {
           try
           {
               serialPort.Open();
               isPortOpen = true; // Порт открыт
               MessageBox.Show("Порт открыт успешно!");
               start.IsEnabled = false;
               open_file1.IsEnabled = false;
               kill_file1.IsEnabled = false;
               write_in_baz.IsEnabled = false;
               InitializeFileWriting();
           }
           catch (Exception ex)
           {
               MessageBox.Show($"Ошибка при открытии порта: {ex.Message}");
           }
       }


       private void ClosePort()
       {
           if (serialPort != null)
           {
               isPortOpen = false;
               serialPort.Close();
               MessageBox.Show("Порт закрыт!");
               data11.Text = string.Empty;
               start.IsEnabled = true;
               open_file1.IsEnabled = true;
               kill_file1.IsEnabled = true;
               write_in_baz.IsEnabled = true;

               if (streamWriter != null)
               {
                   streamWriter.Close();
                   streamWriter.Dispose();
                   streamWriter = null;
               }
           }
           else {
               MessageBox.Show("Порт не подключен! Его нельзя выключить");
           }


       }


       private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
       {
           try
           {
               string data = serialPort.ReadLine(); // Читайте данные (например, построчно)

               if (isPortOpen == true)
               {
                   Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                   {
                       data11.Text = data;
                   }));
               }
               if (streamWriter != null)
               {
                   data = Regex.Replace(data, @"\s+", " ");
                   data = data.Substring(0, data.Length - 1);
                   streamWriter.WriteLine(id_fly.ToString() + ", " + DateTime.Now.ToString() + ", " + data); // Записываем строку
                   streamWriter.Flush(); // Сохраняем данные немедленно
               }

           }
           catch (IOException ioEx)
           {
               // Обработка исключения ввода-вывода
               Console.WriteLine($"Ошибка ввода-вывода: {ioEx.Message}");
           }
           catch (InvalidOperationException invOpEx)
           {
               // Обработка исключения при некорректном обращении к порту
               Console.WriteLine($"Некорректная операция: {invOpEx.Message}");
           }
           catch (Exception ex)
           {
               // Ловим все остальные исключения
               Console.WriteLine($"Произошла ошибка: {ex.Message}");
           }
       }

       private void Stert_port(object sender, RoutedEventArgs e)
       {

           if (port.SelectedIndex != -1)
           {
               if (drone_name_.SelectedIndex != -1)
               {
                   Write_flight();
                   OpenPort(name_of_Port());
                   serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
               }
               else {
                   MessageBox.Show("Выберете дрон!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
               }
           }
           else 
           {
               MessageBox.Show("Выберете порт!","", MessageBoxButton.OK, MessageBoxImage.Warning);
           }
       }

       private void End_port(object sender, RoutedEventArgs e)
       {
           ClosePort();
       }


       private void open_file(object sender, RoutedEventArgs e)
       {
           string path = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt"; // Укажите путь к вашему файлу
           LaunchFile(path);
           string path1 = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\flight.txt"; // Укажите путь к вашему файлу
           LaunchFile(path1);

       }

       private void LaunchFile(string filePath)
       {
           try
           {
               Process.Start(filePath);
           }
           catch (Exception ex)
           {
               MessageBox.Show($"Ошибка при запуске файла: {ex.Message}");
           }
       }

       private void kill_file(object sender, RoutedEventArgs e)
       {
           string path = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt"; // Укажите ваш путь
           DeleteFile(path);
           Is_load_text();
       }

       public void DeleteFile(string filePath)
       {
           // Проверяем, существует ли файл
           if (File.Exists(filePath))
           {
               try
               {
                   File.Delete(filePath); // Удаляем файл
                   MessageBox.Show("Файл успешно удалён!");
               }
               catch (Exception ex)
               {
                   MessageBox.Show($"Ошибка при удалении файла: {ex.Message}");
               }
           }
           else
           {
               MessageBox.Show("Файл не найден!");
           }
       }

       private void btn_back(object sender, RoutedEventArgs e)
       {
           this.Close();
       }

       private void write_in_baz_click(object sender, RoutedEventArgs e) // запись в базу с блокнота
       {
           string filePath = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt";

           List<string> lines = new List<string>();
           if (File.Exists(filePath))
           {
               try
               {
                   using (StreamReader r = new StreamReader(filePath))
                   {
                       string line;
                       while ((line = r.ReadLine()) != null)
                       {
                           lines.Add(line);
                       }
                   }

                   using (var context = new FPV_dronEntities1())
                   {
                       bool is_done = false;
                       foreach (var item in lines)
                       {
                           var values = item.Split(',');

                           if (values.Length >= 5)
                           {
                               try
                               {
                                   var acsel = new Acsel();
                                   acsel.id_flight = Convert.ToInt32(values[0].Trim());


                                   try
                                   {
                                       acsel.time_start = DateTime.Parse(values[1].Trim());
                                   }

                                   catch (FormatException ex)
                                   {
                                       MessageBox.Show($"Ошибка формата даты.... {ex.Message}");
                                   }
                                   acsel.x = float.Parse(values[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                   acsel.y = float.Parse(values[3].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                   acsel.z = float.Parse(values[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                                   // Подтверждаем правильность данных
                                   // MessageBox.Show($"Данные: ;{acsel.id_flight};, ;{acsel.time_start};, ;{acsel.x};, ;{acsel.y};, ;{acsel.z};");

                                   context.Acsel.Add(acsel);
                               }
                               catch (FormatException ex)
                               {
                                   MessageBox.Show($"Ошибка формата: '{values[1]}' не является корректной датой. {ex.Message}");
                                   is_done = true;
                               }
                               catch (Exception ex)
                               {
                                   MessageBox.Show($"Ошибка: {ex.Message}");
                                   is_done = true;
                               }
                           }
                           else
                           {
                               MessageBox.Show("Чего-то не хватает в данных!");
                               MessageBox.Show("Запись не удалась!");
                               is_done = true;
                               break;
                           }
                       }

                       if (is_done != true)
                       {
                           context.SaveChanges();
                           MessageBox.Show("Данные успешно записаны в базу данных.");
                       }
                   }
               }
               catch (DbUpdateException ex)
               {
                   MessageBox.Show($"Произошла ошибка при чтении файла: {ex.Message}");
                   Console.WriteLine(ex.InnerException?.Message);
                   Console.WriteLine(ex);
               }
           }
           else {
               MessageBox.Show("Нет файла data.txt");
           }
       } 


       String name_of_Port() {

           String name = port.SelectedItem.ToString();
           string[] words = name.Split(' ');
           Console.WriteLine("1 = " + words[0]);
           Console.WriteLine("2 = " + words[1]);
           name = words[0];

           return name;
       }

       public void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
           string drone_name = drone_name_.SelectedItem.ToString();
           using (var context = new FPV_dronEntities1())
           {

               try
               {
                   var query = from drone in context.Drone
                               join controlType in context.Control_Type on drone.id_type_control equals controlType.id_cnotrol_type
                               join typeDrone in context.Type_Drone on drone.id_type_drone equals typeDrone.id_type_drone
                               join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                               join user in context.Users on droneUser.id_user equals user.id_user
                               join role in context.Role on user.id_role equals role.id_role
                               where user.id_user == user4.id_user 
                               where drone.name_model == drone_name

                               select drone.id_drone;
                   var result = query.FirstOrDefault(); // Получаем первый результат
                   id_drone_ = result;
                   // вывод id
                   MessageBox.Show($"Итак, id дрона = { id_drone_}", "Информация о id"); 
               }

               catch (Exception ex)
               {
                   MessageBox.Show("Ошибочка ID = ",$"{ex}");
               }
           }
       }

   }
}


 * 
 */
