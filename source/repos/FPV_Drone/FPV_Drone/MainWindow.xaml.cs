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
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FPV_Drone.Model;
using System.Data.Entity;

namespace FPV_Drone

{
    public partial class MainWindow : Window
    {
        SerialPort serialPort;
        string name_of_port = "COM15";
        private bool isPortOpen = false;
        private StreamWriter streamWriter;
        private StreamWriter streamWriter_2;
        private CancellationTokenSource cts;
        public int id_fly;
        public long id_1_;
        private long id_user_;
        private long id_drone_;

        public MainWindow(/*long id_user, long id_drone*/)
        {
            InitializeComponent();
          //  id_user_ = id_user;
            serialPort = new SerialPort(name_of_port);
            streamWriter = null;
            streamWriter_2 = null;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void InitializeFileWriting()
        {
            string filePath = "data.txt"; // Укажите путь, если нужно
            streamWriter = new StreamWriter(filePath, true); // 'true' для добавления в конец файла

           /* string filePath_2 = "flight.txt"; // Укажите путь, если нужно
            streamWriter_2 = new StreamWriter(filePath_2, false); // 'true' для добавления в конец файла*/
        }
        private void Write_flight()
        {
            using (StreamReader reader = new StreamReader("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt"))
            {
                string text = reader.ReadLine();
                Console.WriteLine(text);
                id_fly = Convert.ToInt32(text);
            }
            id_fly = id_fly + 1;
            GetLastFlightId();
            if (id_1_ <= id_fly)
            {
                using (StreamWriter writer = new StreamWriter("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt", false))
                {
                    writer.WriteLineAsync(id_fly.ToString());
                }
            }
            else {
                MessageBox.Show("Придется создать новый id_flight");
                try
                {
                    var dbContext = new FPV_dronEntities1();
                    var fligth = new Flight();

                    fligth.id_drone = id_drone_;
                    fligth.id_user = id_user_; 

                    dbContext.Flight.Add(fligth);
                    dbContext.SaveChanges();
                    MessageBox.Show("Новый полет успешно создан!");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                InitializeFileWriting();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии порта: {ex.Message}");
            }
        }
        

        private void ClosePort()
        {
            //   if (serialPort != null && serialPort.IsOpen)
           
                isPortOpen = false;
                serialPort.Close();
                MessageBox.Show("Порт закрыт!");
                data11.Text = string.Empty;
                start.IsEnabled = true;
                open_file1.IsEnabled = true;
                kill_file1.IsEnabled = true;

               if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter.Dispose();
                    streamWriter = null;
                }
                
            
        }

    /*    private async void StartReadingData()
        {
            cts = new CancellationTokenSource();
            while (isPortOpen)
            {
                if (serialPort.IsOpen)
                {
                    try
                    {
                        string data = await Task.Run(() => serialPort.ReadLine(), cts.Token);
                        data = Regex.Replace(data, @"\s+", " ").Trim();
                        Dispatcher.Invoke(() => data11.Text = data);
                        streamWriter?.WriteLine("=" + data + "=");
                        streamWriter?.Flush();
                    }
                    catch (OperationCanceledException) { /* Чтение отменено  }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка чтения: {ex.Message}");
                    }
                }
            }
        }*/


        /*  private async void StartReadingData()
          {
              cts = new CancellationTokenSource();
              while (isPortOpen)
              {
                  if (serialPort.IsOpen)
                  {
                      try
                      {
                          string data = await Task.Run(() => serialPort.ReadLine(), cts.Token);

                          Dispatcher.Invoke(() => data11.Text = data);
                     /    if (streamWriter != null)
                          {
                            //  data = Regex.Replace(data, @"\s+", " ");
                            //  data = data.Substring(0, data.Length - 1);
                              streamWriter.WriteLine(data); // Записываем строку
                              streamWriter.Flush(); // Сохраняем данные немедленно
                          }
                       // await Task.Delay(10);
                      }
                      catch (OperationCanceledException) { cts?.Cancel(); /* Обработка отмены  }
                      catch (IOException ex) { cts?.Cancel();  MessageBox.Show($"Ошибка других...: {ex.Message}"); /* Обработка других ошибок  }
                  }
                  else {
                      cts?.Cancel();
                  }
              }
          }*/
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
            Write_flight();
            OpenPort(name_of_port);
           // StartReadingData(); // Начинаем асинхронное чтение
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }

        private void End_port(object sender, RoutedEventArgs e)
        {
            
            ClosePort();
           // cts?.Cancel();
        }

        private void message(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("И че ты хочешь?", "11111111", MessageBoxButton.OK, MessageBoxImage.Error);
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
         /*   string path1 = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\flight.txt"; // Укажите путь к вашему файлу
            DeleteFile(path1);*/
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
            Window_login login = new Window_login();
            login.Top = 100;
            login.Left = 250;
            login.Show(); 
            this.Close();
        }

        private void write_in_baz_click(object sender, RoutedEventArgs e)
        {
            string filePath = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt";

            List<string> lines = new List<string>();
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
                    foreach (var item in lines)
                    {
                        var values = item.Split(',');

                        if (values.Length >= 5)
                        {
                            try
                            {
                                var acsel = new Acsel();
                                acsel.id_flight = Convert.ToInt32(values[0].Trim());

                              
                                try { 
                                acsel.time_start = DateTime.Parse(values[1].Trim());
                                }

                                catch (FormatException ex)
                                {
                                    MessageBox.Show($"Ошибка формата гребанной даты.... {ex.Message}");
                                }
                                acsel.x = float.Parse(values[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                acsel.y = float.Parse(values[3].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                acsel.z = float.Parse(values[4].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                                // Подтверждаем правильность данных
                               // MessageBox.Show($"Данные: {acsel.id_flight}, {acsel.time_start}, {acsel.x}, {acsel.y}, {acsel.z}");

                                context.Acsel.Add(acsel);
                            }
                            catch (FormatException ex)
                            {
                                MessageBox.Show($"Ошибка формата: '{values[1]}' не является корректной датой. {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка: {ex.Message}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Чего-то не хватает в данных!");
                        }
                    }

                    context.SaveChanges();
                    MessageBox.Show("Данные успешно записаны в базу данных.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при чтении файла: {ex.Message}");
            }
        }



        /*   private void write_in_baz_click(object sender, RoutedEventArgs e)
           {
               /* List<string> lines = new List<string>();
                StreamReader r = new StreamReader("C:\\Users\\User\\source\\repos\\FPV_Drone\\FPV_Drone\\bin\\Debug\\flight.txt");
                string line; 
                while ((line = r.ReadLine()) != null)
                { 
                    lines.Add(line); 
                }

                eventList.ItemsSource = lines; // 

               string filePath = @"C:\Users\User\source\repos\FPV_Drone\FPV_Drone\bin\Debug\data.txt";

               // Считывание данных из файла
               List<string> lines = new List<string>();
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
                   try
                   {
                       // Создаем контекст базы данных
                       using (var context = new FPV_dronEntities1())
                       {

                           foreach (var item in lines)
                           {
                               var values = item.Split(','); // Предполагается, что данные разделены запятыми
                               MessageBox.Show(item);

                               if (values.Length >= 5) // Проверка на наличие всех нужных данных
                               {
                                   var acsel = new Acsel();
                                   {
                                       acsel.id_flight = Convert.ToInt32(values[0]); // Например, if_flight
                                       acsel.time_start = DateTime.Parse(values[1]); // Преобразование строки в DateTime
                                       acsel.x = float.Parse(values[2]);
                                       acsel.y = float.Parse(values[3]);
                                       acsel.z = float.Parse(values[4]);

                                       MessageBox.Show($"Что-то не так! {acsel.id_flight} \n { acsel.time_start} \n {acsel.x} \n {acsel.y} \n {acsel.z} \n ");
                                   };
                                   try
                                   {
                                       context.Acsel.Add(acsel); // Добавляем объект в контекст
                                   }
                                   catch (Exception ex)
                                   {
                                       MessageBox.Show($"Ошибка базы {ex.Message}");
                                   }
                               }
                               else {
                                   MessageBox.Show("Чего-то не хватает!");
                               }
                           }

                           context.SaveChanges(); // Сохраняем изменения в базе данных
                           MessageBox.Show("Данные успешно записаны в базу данных.");
                       }
                   }
                   catch (FormatException ex) {  MessageBox.Show($"Что-то не так! {ex.Message}"); }
               }
               catch (Exception ex)
               {
                   MessageBox.Show($"Произошла ошибка: {ex.Message}");
               }
           }
           */
    }
}

/*
{
/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    SerialPort serialPort;
    String name_of_port = "COM23";

    public MainWindow()
    {
        InitializeComponent();
        serialPort = new SerialPort(name_of_port);
        serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
    }



    private void OpenPort(string portName)
    {
        try
        {
            serialPort.Open();
            MessageBox.Show("Порт открыт успешно!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии порта: {ex.Message}");
        }
    }

    private void ClosePort()
    {

        try
        {
            serialPort.Close()
            serialPort.Dispose();

            MessageBox.Show("Порт закрыт успешно!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при закрытии порта: {ex.Message}");
        }
    }


    private void Stert_port(object sender, RoutedEventArgs e)
    {
        OpenPort(name_of_port);
    }

    private void End_port(object sender, RoutedEventArgs e)
    {
        ClosePort();
    }




    private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
    {
        if (serialPort.IsOpen == true)
        {

            try
            {
                string data = serialPort.ReadLine(); // Читайте данные (например, построчно)
                Dispatcher.Invoke(() =>
                {
                    data11.Text = data; // Обновляем текст в UI
                });
            }
            catch (Exception ex)
            {
                // Может быть полезно записать в лог или отобразить сообщение
                Console.WriteLine($"Ошибка чтения: {ex.Message}");
            }
        }
        else {
            MessageBox.Show("не читаем т.к. порт закрыт!", "Заголовок", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }
    }

    private void message(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("И че ты хочешь?", "11111111", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
}
*/
// Получаем строку для даты
//  string dateString = values[1].Trim();

// Попробуем парсить с явным указанием формата
/*  try
  {
      acsel.time_start = DateTime.ParseExact(dateString, "dd.MM.yyyy HH:mm:ss",
      System.Globalization.CultureInfo.InvariantCulture);

  }
  catch (FormatException ex)
  {
      MessageBox.Show($"Ошибка формата даты: '{dateString}' не распознана как корректная дата. {ex.Message}");
      continue; // Пропустить текущую итерацию, если дата некорректна
  }
  */
