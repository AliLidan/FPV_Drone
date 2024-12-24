
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FPV_Drone.Model;
using System.Windows.Controls;


namespace FPV_Drone.Pages
{
    public partial class Grath : Window
    {
        Users user4;
        private string fnd = "";//Строка поиска
        private List<FlightInfo> allFlights;
        private string servicesSortBy;
        private string DataPicker_text;
        DateTime today = new DateTime();
        DateTime last_week = new DateTime();
        bool is_nearly = false;
        bool is_admin = false;

        public Grath(Users user2, bool is_admin_1)
        {
            InitializeComponent();
            user4 = user2;
            is_admin = is_admin_1;

            Count_drones();
        }

        private void New_data_open(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker datePicker)
            {
                DataPicker_text = datePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty; // Форматирование даты
                FilterFlights(0); // Вызываем метод фильтрации
            }
        }

        private void TextBox_TextChanged_search(object sender, TextChangedEventArgs e)
        {
            fnd = ((TextBox)sender).Text;
            FilterFlights(0); // Вызываем метод фильтрации
        }


        private void CBSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                servicesSortBy = selectedItem.Content.ToString(); // Сохраняем выбранное значение сортировки
                FilterFlights(0); // Вызываем метод фильтрации
            }
        }

        private void FilterFlights(int diagramm)
        {
            if (diagramm == 0)
            {
                DateTime? selectedDate = null;

                // Попробуем получить значение из DatePicker
                if (DateTime.TryParse(DataPicker_text, out DateTime parsedDate))
                {
                    selectedDate = parsedDate.Date; // Убедимся, что у нас только дата (без времени)
                }

                // Начинаем с полного набора полетов
                var filteredFlights = allFlights.AsQueryable();

                // Фильтрация по строке поиска
                filteredFlights = filteredFlights.Where(f =>
                    f.FlightId.ToString().Contains(fnd) ||
                    f.ModelName.ToLower().Contains(fnd.ToLower()) ||
                    f.UserName.ToLower().Contains(fnd.ToLower()));

                // Фильтрация по выбранной дате
                if (selectedDate.HasValue)
                {
                    filteredFlights = filteredFlights.Where(f =>
                        (f.FlightDate_start.HasValue && f.FlightDate_start.Value.Date == selectedDate.Value) ||
                        (f.FlightDate_end.HasValue && f.FlightDate_end.Value.Date == selectedDate.Value));
                }

                // Фильтрация по недавним датам
                if (is_nearly)
                {
                    filteredFlights = filteredFlights.Where(f =>
                        (f.FlightDate_start.HasValue && f.FlightDate_start.Value.Date >= last_week.Date && f.FlightDate_start.Value.Date <= today.Date) ||
                        (f.FlightDate_end.HasValue && f.FlightDate_end.Value.Date >= last_week.Date && f.FlightDate_end.Value.Date <= today.Date));
                }

                // Преобразуем в список
                var flightList = filteredFlights.ToList();

                // Сортировка по выбранному критерию
                if (!string.IsNullOrEmpty(servicesSortBy))
                    flightList = SortServices(flightList, servicesSortBy);

                // Обновление DataGrid
                FlightsDataGrid.ItemsSource = flightList.Select(f => new
                {
                    f.FlightId,
                    f.ModelName,
                    f.UserName,
                    FlightDate = f.FlightDate?.ToString() ?? "Не указано",
                    FlightDate_start = f.FlightDate_start?.ToString() ?? "Не указано",
                    FlightDate_end = f.FlightDate_end?.ToString() ?? "Не указано",
                    FlightDuration = f.FlightDuration?.ToString() ?? "Не указано"
                }).ToList();

                // Сбрасываем флаг для следующей фильтрации
                is_nearly = false;
            }

            else { 
            
            
            }
        }





        private List<FlightInfo> SortServices(List<FlightInfo> services, string sortBy)
        {
            switch (sortBy)
            {
                case "По возрастанию":
                    return services.OrderBy(s => s.FlightDate).ToList();
                case "По убыванию":
                    return services.OrderByDescending(s => s.FlightDate).ToList();
                default:
                    return services;
            }
        }


        private List<KeyValuePair<string, int>> GetFlightCountsForDrones() // добавление инфы в график
        {
            using (var context = new FPV_dronEntities1())
            {
                if (is_admin == false) // пользователь 
                {
                    var query = from flight in context.Flight
                                join drone in context.Drone on flight.id_drone equals drone.id_drone
                                join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                                where droneUser.id_user == user4.id_user
                                group flight by drone.name_model into g
                                select new
                                {
                                    ModelName = g.Key,
                                    Count = g.Count()
                                };

                    return query.ToList().OrderByDescending(x => x.Count).Select(x => new KeyValuePair<string, int>(x.ModelName, x.Count)).ToList();
                }
                else
                {
                    var query = from flight in context.Flight
                                join drone in context.Drone on flight.id_drone equals drone.id_drone
                                join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                                group flight by drone.name_model into g
                                select new
                                {
                                    ModelName = g.Key,
                                    Count = g.Count()
                                };

                    return query.ToList().OrderByDescending(x => x.Count).Select(x => new KeyValuePair<string, int>(x.ModelName, x.Count)).ToList();

                }
            }
        }

        public void Count_drones() // главная функуия вывода + ретинга + всег-всего
        {
            var flightCounts = GetFlightCountsForDrones();
            var series = new SeriesCollection();

            foreach (var flight in flightCounts)
            {
                series.Add(new PieSeries
                {
                    Title = flight.Key,
                    Values = new ChartValues<double> { flight.Value },
                    DataLabels = true
                });
            }

            pieChart.Series = series;
            allFlights = GetAllFlights();// счет времени полета
            DisplayRating(flightCounts);
        }

        private List<FlightInfo> GetAllFlights() // счет времени полета + вывод нужностей
        {
            using (var context = new FPV_dronEntities1())
            {
                if (is_admin == false)
                {
                    var flights = from flight in context.Flight
                                  join drone in context.Drone on flight.id_drone equals drone.id_drone
                                  where flight.id_user == user4.id_user // Фильтрация по текущему пользователю
                                  select new FlightInfo // Создаем новый объект FlightInfo
                                  {
                                      FlightId = flight.id_flight,
                                      ModelName = drone.name_model,
                                      UserName = user4.surname, // Замените на реальное значение
                                      FlightDate = flight.time_all, // Убедитесь, что это поле существует в Flight
                                      FlightDate_start = (from acsel in context.Acsel
                                                          where acsel.id_flight == flight.id_flight
                                                          orderby acsel.time_start
                                                          select (DateTime?)acsel.time_start).FirstOrDefault(),
                                      FlightDate_end = (from acsel in context.Acsel
                                                        where acsel.id_flight == flight.id_flight
                                                        orderby acsel.time_start descending
                                                        select (DateTime?)acsel.time_start).FirstOrDefault(),
                                  };

                    // Сначала получаем все полеты в список
                    var flightList = flights.ToList();

                    // Теперь рассчитываем длительность полета
                    foreach (var flightInfo in flightList)
                    {
                        // Вычисляем длину времени
                        if (flightInfo.FlightDate_start.HasValue && flightInfo.FlightDate_end.HasValue)
                        {
                            flightInfo.FlightDuration = flightInfo.FlightDate_end.Value - flightInfo.FlightDate_start.Value;
                        }
                        else
                        {
                            // Если одно из значений отсутствует, длительность будет null
                            flightInfo.FlightDuration = null;
                        }
                    }

                    return flightList; // Возвращаем список
                }
                else
                {
                                        var flights = from flight in context.Flight
                                  join drone in context.Drone on flight.id_drone equals drone.id_drone
                                  join users in context.Users on flight.id_user equals users.id_user
                                  //flight.id_user == user4.id_user // Фильтрация по текущему пользователю
                                  select new FlightInfo // Создаем новый объект FlightInfo
                                  {
                                      FlightId = flight.id_flight,
                                      ModelName = drone.name_model,
                                      UserName = users.surname, // Замените на реальное значение
                                      FlightDate = flight.time_all, // Убедитесь, что это поле существует в Flight
                                      FlightDate_start = (from acsel in context.Acsel
                                                          where acsel.id_flight == flight.id_flight
                                                          orderby acsel.time_start
                                                          select (DateTime?)acsel.time_start).FirstOrDefault(),
                                      FlightDate_end = (from acsel in context.Acsel
                                                        where acsel.id_flight == flight.id_flight
                                                        orderby acsel.time_start descending
                                                        select (DateTime?)acsel.time_start).FirstOrDefault(),
                                  };

                    // Сначала получаем все полеты в список
                    var flightList = flights.ToList();

                    // Теперь рассчитываем длительность полета
                    foreach (var flightInfo in flightList)
                    {
                        // Вычисляем длину времени
                        if (flightInfo.FlightDate_start.HasValue && flightInfo.FlightDate_end.HasValue)
                        {
                            flightInfo.FlightDuration = flightInfo.FlightDate_end.Value - flightInfo.FlightDate_start.Value;
                        }
                        else
                        {
                            // Если одно из значений отсутствует, длительность будет null
                            flightInfo.FlightDuration = null;
                        }
                    }

                    return flightList; // Возвращаем список
                }
            }
        }




        private void DisplayRating(List<KeyValuePair<string, int>> flightCounts) // рейтинг красивый вывод
        {                                                                        // заполнение слов с полетами + номер рейтинга
            RatingListBox.Items.Clear();
            int rank = 1;
            foreach (var item in flightCounts)
            {
                RatingListBox.Items.Add($"{rank}. {item.Key} - {item.Value} полетов");
                rank++;
            }
        }

        private void RatingListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RatingListBox.SelectedItem != null)
            {
                var selectedItem = RatingListBox.SelectedItem.ToString();
                var modelName = selectedItem.Split('-')[0].Trim();  // Извлечение имени модели из строки
                LoadFlightData(modelName);
            }
        }

        private void PieChart_DataClick(object sender, ChartPoint chartPoint)
        {
            var modelName = chartPoint.SeriesView.Title;  // Получение названия модели из выбранного сегмента
            LoadFlightData(modelName); // Загружаем данные о полетах для выбранной модели дрона
        }



        private void LoadFlightData(string modelName)
        {
            var flightData = GetFlightsForModel(modelName); // Метод для получения данных о полетах

            // Подготовьте данные для отображения, учитывая возможности null
            var displayData = flightData.Select(f => new
            {
                f.FlightId,
                f.ModelName,
                f.UserName,
                FlightDate = f.FlightDate?.ToString() ?? "Не указано",
                FlightDate_start = f.FlightDate_start?.ToString() ?? "Не указано",
                FlightDate_end = f.FlightDate_end?.ToString() ?? "Не указано",
                FlightDuration = f.FlightDuration?.ToString() ?? "Не указано"
            }).ToList();

            // Заполнение DataGrid
            FlightsDataGrid.ItemsSource = displayData;
        }

        private List<FlightInfo> GetFlightsForModel(string modelName) // для выводя в grid выбранную модель с графика 
        {
            using (var context = new FPV_dronEntities1())
            {
                if (is_admin == false) // обычный пользователь
                {
                    var flights = from flight in context.Flight
                                  join drone in context.Drone on flight.id_drone equals drone.id_drone
                                  where drone.name_model == modelName &&
                                        flight.id_user == user4.id_user // Фильтрация по текущему пользователю
                                  select new FlightInfo // Создаем новый объект FlightInfo
                                  {
                                      FlightId = flight.id_flight,
                                      ModelName = drone.name_model,
                                      UserName = user4.surname, // Замените на реальное значение
                                      FlightDate = flight.time_all, // Убедитесь, что это поле существует в Flight
                                      FlightDate_start = (from acsel in context.Acsel
                                                          where acsel.id_flight == flight.id_flight
                                                          orderby acsel.time_start
                                                          select (DateTime?)acsel.time_start).FirstOrDefault(),
                                      FlightDate_end = (from acsel in context.Acsel
                                                        where acsel.id_flight == flight.id_flight
                                                        orderby acsel.time_start descending
                                                        select (DateTime?)acsel.time_start).FirstOrDefault(),
                                  };
                    var flightList = flights.ToList();

                    // Теперь рассчитываем длительность полета
                    foreach (var flightInfo in flightList)
                    {
                        // Вычисляем длину времени
                        if (flightInfo.FlightDate_start.HasValue && flightInfo.FlightDate_end.HasValue)
                        {
                            flightInfo.FlightDuration = flightInfo.FlightDate_end.Value - flightInfo.FlightDate_start.Value;
                        }
                        else
                        {
                            // Если одно из значений отсутствует, длительность будет null
                            flightInfo.FlightDuration = null;
                        }
                    }

                    return flightList; // Возвращаем список
                }

                else
                {
                    var flights = from flight in context.Flight
                                  join drone in context.Drone on flight.id_drone equals drone.id_drone
                                  join users in context.Users on flight.id_user equals users.id_user
                                  where drone.name_model == modelName
                                  select new FlightInfo // Создаем новый объект FlightInfo
                                  {
                                      FlightId = flight.id_flight,
                                      ModelName = drone.name_model,
                                      UserName = users.surname, // Замените на реальное значение
                                      FlightDate = flight.time_all, // Убедитесь, что это поле существует в Flight
                                      FlightDate_start = (from acsel in context.Acsel
                                                          where acsel.id_flight == flight.id_flight
                                                          orderby acsel.time_start
                                                          select (DateTime?)acsel.time_start).FirstOrDefault(),
                                      FlightDate_end = (from acsel in context.Acsel
                                                        where acsel.id_flight == flight.id_flight
                                                        orderby acsel.time_start descending
                                                        select (DateTime?)acsel.time_start).FirstOrDefault(),
                                  };
                    var flightList = flights.ToList();

                    // Теперь рассчитываем длительность полета
                    foreach (var flightInfo in flightList)
                    {
                        // Вычисляем длину времени
                        if (flightInfo.FlightDate_start.HasValue && flightInfo.FlightDate_end.HasValue)
                        {
                            flightInfo.FlightDuration = flightInfo.FlightDate_end.Value - flightInfo.FlightDate_start.Value;
                        }
                        else
                        {
                            // Если одно из значений отсутствует, длительность будет null
                            flightInfo.FlightDuration = null;
                        }
                    }

                    return flightList; // Возвращаем список
                }

            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox nerly_t) 
            { 
                today = DateTime.Today;
                last_week = today.AddDays(-7);
                is_nearly = true;
                FilterFlights(0);
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox nerly_t)
            {
                is_nearly = false; // Убираем фильтрацию по недавним полетам
                FilterFlights(0); // Вызываем метод фильтрации для обновления списка
            }
        }
    }

    public class FlightInfo
    {
        public long FlightId { get; set; }
        public string ModelName { get; set; }
        public string UserName { get; set; }
        public TimeSpan? FlightDate { get; set; }
        public DateTime? FlightDate_start { get; set; }
        public DateTime? FlightDate_end { get; set; }

        public TimeSpan? FlightDuration { get; set; }

        // Добавьте другие поля, которые вам нужны для отображения
    }
}



/* выводит кто сколько летал
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FPV_Drone.Model;

namespace FPV_Drone.Pages
{
    public partial class Grath : Window
    {
        Users user4;

        public Grath(Users user2)
        {
            InitializeComponent();
            // Данные для диаграммы
            user4 = user2;
            Count_drones();
        }

        private Dictionary<string, int> GetFlightCountsForDrones()
        {
            using (var context = new FPV_dronEntities1())
            {
                var query = from flight in context.Flight
                            join drone in context.Drone on flight.id_drone equals drone.id_drone
                            join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                            where droneUser.id_user == user4.id_user
                            group flight by drone.name_model into g
                            select new
                            {
                                ModelName = g.Key,
                                Count = g.Count()
                            };

                var flightCounts = new Dictionary<string, int>();

                foreach (var item in query.ToList())
                {
                    flightCounts.Add(item.ModelName, item.Count);
                }

                return flightCounts;
            }
        }

        public void Count_drones()
        {
            var flightCounts = GetFlightCountsForDrones(); // Получаем количество полетов по моделям дронов
            var series = new SeriesCollection();

            foreach (var flight in flightCounts)
            {
                series.Add(new PieSeries
                {
                    Title = flight.Key,
                    Values = new ChartValues<double> { flight.Value },
                    DataLabels = true
                });
            }

            pieChart.Series = series; // Обновляем диаграмму
        }
    }
}

*/



/*       public void Count_drones()
       {
           var droneCounts = GetDroneCounts();
           var series = new SeriesCollection();

           foreach (var drone in droneCounts)
           {
               series.Add(new PieSeries
               {
                   Title = drone.Key,
                   Values = new ChartValues<double> { drone.Value },
                   DataLabels = true
               });
           }

           pieChart.Series = series;
       }

       private Dictionary<string, int> GetDroneCounts()
       {
           // Используйте ваш контекст данных
           using (var context = new FPV_dronEntities1())
           {
               var query = from drone in context.Drone
                           join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                           where droneUser.id_user == user4.id_user
                           group drone by drone.name_model into g
                           select new
                           {
                               ModelName = g.Key,
                               Count = g.Count()
                           };

               var droneCounts = new Dictionary<string, int>();

               foreach (var item in query.ToList())
               {
                   droneCounts.Add(item.ModelName, item.Count);
               }

               return droneCounts;
           }
       }*/

/*     pieChart.Series = new SeriesCollection
     {
         new PieSeries
         {
             Title = "Сегмент 1",
             Values = new ChartValues<double> { 30 },
             DataLabels = true
         },
         new PieSeries
         {
             Title = "Сегмент 2",
             Values = new ChartValues<double> { 50 },
             DataLabels = true
         },
         new PieSeries
         {
             Title = "Сегмент 3",
             Values = new ChartValues<double> { 20 },
             DataLabels = true
         }
     };

 }
 public void Count_drones()
 {
     using (var context = new FPV_dronEntities1())
     {
         var query = from drone in context.Drone
                     join droneUser in context.Drone_User on drone.id_drone equals droneUser.id_drone
                     where droneUser.id_user == user4.id_user
                     group drone by drone.name_model into g
                     select new
                     {
                         ModelName = g.Key,
                         Count = g.Count()
                     };
         var resultList = query.ToList(); // Выполняем запрос и извлекаем результаты

 }*/
