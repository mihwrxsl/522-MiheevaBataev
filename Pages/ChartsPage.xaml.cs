using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using System.Drawing; // Для Font и Color
using FontStyle = System.Drawing.FontStyle; // Явно указываем какую FontStyle используем!
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Linq; 

namespace _522_Miheeva.Pages
{
    public partial class ChartsPage : Page
    {
        private Chart _chart;

        public ChartsPage()
        {
            InitializeComponent();
            CreateChart();
            LoadUsers();
            LoadChartData();
        }

        private void CreateChart()
        {
            try
            {
                // Создаем хост для Windows Forms диаграммы
                var host = new WindowsFormsHost();
                _chart = new Chart();
                _chart.Size = new System.Drawing.Size(700, 400);

                // Настраиваем область диаграммы
                var chartArea = new ChartArea("MainArea");
                _chart.ChartAreas.Add(chartArea);

                // Настраиваем легенду
                var legend = new Legend();
                legend.Title = "Категории платежей";
                _chart.Legends.Add(legend);

                // Добавляем диаграмму в хост
                host.Child = _chart;

                // Добавляем хост в WPF Grid
                var mainGrid = (System.Windows.Controls.Grid)this.Content;
                var border = (Border)mainGrid.Children[1];
                var innerGrid = (System.Windows.Controls.Grid)border.Child;
                innerGrid.Children.Add(host);
                System.Windows.Controls.Grid.SetRow(host, 1);

                StatusText.Text = "🌸 Диаграмма создана!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания диаграммы: {ex.Message}", "Ошибка");
            }
        }

        private void LoadChartData()
        {
            try
            {
                if (_chart == null) return;

                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                SELECT 
                    c.Name as CategoryName, 
                    SUM(p.Price * p.Num) as TotalAmount,
                    COUNT(p.ID) as PaymentCount
                FROM Payments p
                INNER JOIN Categories c ON p.CategoryID = c.ID
                WHERE (@UserID = 0 OR p.UserID = @UserID)
                GROUP BY c.Name
                ORDER BY TotalAmount DESC";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        int selectedUserId = 0;
                        if (CmbUser.SelectedItem is User selectedUser && selectedUser.ID != 0)
                        {
                            selectedUserId = selectedUser.ID;
                        }
                        command.Parameters.AddWithValue("@UserID", selectedUserId);

                        using (var reader = command.ExecuteReader())
                        {
                            // Очищаем старые данные
                            _chart.Series.Clear();

                            // 🌟 СНАЧАЛА НАСТРАИВАЕМ ЦВЕТА ДИАГРАММЫ
                            _chart.Palette = ChartColorPalette.None;

                            // 🌸 НАША РОЗОВО-ЗОЛОТАЯ ПАЛИТРА
                            Color[] glamourColors = {
                        Color.DeepPink,      // 🌸 Ярко-розовый
                        Color.HotPink,       // 💖 Нежно-розовый
                        Color.Pink,          // 🎀 Светло-розовый
                        Color.LightPink,     // 💕 Очень светлый розовый
                        Color.Plum,          // 🍇 Сливовый
                        Color.Orchid,        // 🌺 Орхидея
                        Color.Violet,        // 💜 Фиолетовый
                        Color.Gold,          // 💛 Золотой
                        Color.LightGoldenrodYellow, // 💛 Светло-золотой
                        Color.MistyRose      // 🌸 Тумано-розовый
                    };
                            _chart.PaletteCustomColors = glamourColors;

                            // 🌟 КРАСИВЫЙ ФОН ДЛЯ ДИАГРАММЫ
                            _chart.BackColor = Color.LavenderBlush;
                            _chart.ChartAreas[0].BackColor = Color.LavenderBlush;

                            // Создаем новую серию данных
                            var series = new Series("Платежи по категориям");

                            // Выбираем тип диаграммы
                            if (CmbChartType.SelectedIndex == 0)
                                series.ChartType = SeriesChartType.Pie;
                            else if (CmbChartType.SelectedIndex == 1)
                                series.ChartType = SeriesChartType.Column;
                            else
                                series.ChartType = SeriesChartType.Line;

                            series.IsValueShownAsLabel = true;

                            // Умные подписи для разных типов диаграмм
                            if (series.ChartType == SeriesChartType.Pie)
                                series.Label = "#VALX: #VALY руб (#PERCENT{P0})";
                            else
                                series.Label = "#VALY руб";

                            // 🌟 СТИЛЬНЫЕ ПОДПИСИ
                            series.Font = new Font(new FontFamily("Segoe UI"), 9, FontStyle.Bold);
                            series.LabelForeColor = Color.DarkMagenta;

                            // 🌟 ДЛЯ КРУГОВОЙ ДИАГРАММЫ ДЕЛАЕМ ЕЩЕ КРАСИВЕЕ
                            if (series.ChartType == SeriesChartType.Pie)
                            {
                                series["PieLabelStyle"] = "Outside";
                                series["PieLineColor"] = "Gold";
                                series.BorderColor = Color.Gold;
                                series.BorderWidth = 2;
                            }

                            decimal totalAll = 0;

                            // Заполняем данными
                            while (reader.Read())
                            {
                                string categoryName = reader["CategoryName"].ToString();
                                decimal amount = (decimal)reader["TotalAmount"];
                                int count = (int)reader["PaymentCount"];

                                var dataPoint = new DataPoint();
                                dataPoint.SetValueXY(categoryName, (double)amount);
                                dataPoint.LabelToolTip = $"{categoryName}\nСумма: {amount:N2} руб\nПлатежей: {count}";
                                dataPoint.LegendText = $"{categoryName} - {amount:N0} руб";

                                series.Points.Add(dataPoint);
                                totalAll += amount;
                            }

                            // 🌟 ВОТ ТЕПЕРЬ САМОЕ ВАЖНОЕ - ДОБАВЛЯЕМ СЕРИЮ ПОСЛЕ ВСЕХ НАСТРОЕК!
                            _chart.Series.Add(series);

                            // Красивый статус
                            string userName = "всех пользователей";
                            if (CmbUser.SelectedItem is User user && user.ID != 0)
                            {
                                userName = user.FIO;
                            }

                            StatusText.Text = $"📊 Диаграмма для {userName}! Всего: {totalAll:N2} руб";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"❌ Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateChartType()
        {
            if (IsLoaded)
                LoadChartData(); // 👈 ПРОСТО ПЕРЕЗАГРУЖАЕМ ДАННЫЕ
        }

        // 🌸 ОБРАБОТЧИКИ СОБЫТИЙ

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }

        private void CmbChartType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                UpdateChartType();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadChartData();
            MessageBox.Show("🌸 Диаграмма обновлена!", "Обновление");
        }

        private void CmbUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить фильтрацию по пользователю позже
            if (IsLoaded)
                LoadChartData();
        }


        // 🌸 МЕТОД ДЛЯ ЗАГРУЗКИ ПОЛЬЗОВАТЕЛЕЙ
        private void LoadUsers()
        {
            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT ID, FIO FROM Users";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var users = new List<User>();
                        users.Add(new User { ID = 0, FIO = "👥 Все пользователи" }); // 👈 ДОБАВИМ ВСЕХ

                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                ID = (int)reader["ID"],
                                FIO = reader["FIO"].ToString()
                            });
                        }

                        CmbUser.ItemsSource = users;
                        CmbUser.SelectedIndex = 0; // 👈 ВЫБИРАЕМ "ВСЕХ" ПО УМОЛЧАНИЮ
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
            }
        }

        // 🌟 ВСПОМОГАТЕЛЬНЫЙ КЛАСС ДЛЯ ПОЛЬЗОВАТЕЛЕЙ
        public class User
        {
            public int ID { get; set; }
            public string FIO { get; set; }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем приложение Word
                var wordApp = new Word.Application();
                wordApp.Visible = false; // Сначала скрываем

                // Создаем документ
                Word.Document document = wordApp.Documents.Add();

                // Добавляем заголовок
                Word.Paragraph titleParagraph = document.Paragraphs.Add();
                Word.Range titleRange = titleParagraph.Range;
                titleRange.Text = "💖 GLAMOUR PAYMENTS - АНАЛИТИКА ПЛАТЕЖЕЙ";
                titleRange.Font.Size = 16;
                titleRange.Font.Bold = 1;
                titleRange.Font.Color = Word.WdColor.wdColorViolet;
                titleRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                titleRange.InsertParagraphAfter();

                // Добавляем дату отчета
                Word.Paragraph dateParagraph = document.Paragraphs.Add();
                Word.Range dateRange = dateParagraph.Range;
                dateRange.Text = $"Отчет создан: {DateTime.Now:dd.MM.yyyy HH:mm}";
                dateRange.Font.Size = 12;
                dateRange.Font.Italic = 1;
                dateRange.Font.Color = Word.WdColor.wdColorGray50;
                dateRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                dateRange.InsertParagraphAfter();

                document.Paragraphs.Add(); // Пустая строка

                // Получаем данные для отчета
                var chartData = GetChartDataForReport();

                if (chartData.Count > 0)
                {
                    // Создаем таблицу с данными
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;

                    // Таблица: строки = категории + заголовок, колонки = 3
                    Word.Table dataTable = document.Tables.Add(tableRange, chartData.Count + 1, 3);
                    dataTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    dataTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                    // Заголовок таблицы
                    dataTable.Cell(1, 1).Range.Text = "📊 Категория";
                    dataTable.Cell(1, 2).Range.Text = "💰 Сумма";
                    dataTable.Cell(1, 3).Range.Text = "📈 Доля";

                    // Форматируем заголовок
                    Word.Range headerRange = dataTable.Rows[1].Range;
                    headerRange.Bold = 1;
                    headerRange.Font.Color = Word.WdColor.wdColorWhite;
                    headerRange.Shading.BackgroundPatternColor = Word.WdColor.wdColorViolet;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    decimal totalAmount = chartData.Sum(x => x.Amount);

                    // Заполняем таблицу данными
                    for (int i = 0; i < chartData.Count; i++)
                    {
                        var category = chartData[i];
                        double percentage = totalAmount > 0 ? (double)(category.Amount / totalAmount * 100) : 0;

                        dataTable.Cell(i + 2, 1).Range.Text = category.CategoryName;
                        dataTable.Cell(i + 2, 2).Range.Text = $"{category.Amount:N2} руб.";
                        dataTable.Cell(i + 2, 3).Range.Text = $"{percentage:F1}%";

                        // Красим строки в разные цвета
                        if (i % 2 == 0)
                        {
                            dataTable.Rows[i + 2].Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorLightYellow;
                        }
                    }

                    document.Paragraphs.Add(); // Пустая строка

                    // Добавляем итоги
                    Word.Paragraph totalParagraph = document.Paragraphs.Add();
                    Word.Range totalRange = totalParagraph.Range;
                    totalRange.Text = $"💰 ОБЩАЯ СУММА: {totalAmount:N2} руб.";
                    totalRange.Font.Size = 14;
                    totalRange.Font.Bold = 1;
                    totalRange.Font.Color = Word.WdColor.wdColorDarkRed;
                    totalRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    totalRange.InsertParagraphAfter();

                    // Информация о пользователе
                    string userName = "все пользователи";
                    if (CmbUser.SelectedItem is User selectedUser && selectedUser.ID != 0)
                    {
                        userName = selectedUser.FIO;
                    }

                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = $"👤 Отчет для: {userName}";
                    userRange.Font.Size = 12;
                    userRange.Font.Color = Word.WdColor.wdColorDarkBlue;
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                }
                else
                {
                    Word.Paragraph noDataParagraph = document.Paragraphs.Add();
                    Word.Range noDataRange = noDataParagraph.Range;
                    noDataRange.Text = "📭 Нет данных для отображения";
                    noDataRange.Font.Size = 14;
                    noDataRange.Font.Color = Word.WdColor.wdColorOrange;
                    noDataRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                // Показываем документ
                wordApp.Visible = true;

                MessageBox.Show("🌸 Отчет успешно создан в Word!", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка при экспорте в Word: {ex.Message}", "Ошибка");
            }
        }

        // 📊 Вспомогательный метод для получения данных
        private List<CategoryData> GetChartDataForReport()
        {
            var result = new List<CategoryData>();

            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                SELECT 
                    c.Name as CategoryName, 
                    SUM(p.Price * p.Num) as TotalAmount
                FROM Payments p
                INNER JOIN Categories c ON p.CategoryID = c.ID
                WHERE (@UserID = 0 OR p.UserID = @UserID)
                GROUP BY c.Name
                ORDER BY TotalAmount DESC";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        int selectedUserId = 0;
                        if (CmbUser.SelectedItem is User selectedUser && selectedUser.ID != 0)
                        {
                            selectedUserId = selectedUser.ID;
                        }
                        command.Parameters.AddWithValue("@UserID", selectedUserId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new CategoryData
                                {
                                    CategoryName = reader["CategoryName"].ToString(),
                                    Amount = (decimal)reader["TotalAmount"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных: {ex.Message}", "Ошибка");
            }

            return result;
        }

        // 🏷️ Вспомогательный класс для данных категорий
        public class CategoryData
        {
            public string CategoryName { get; set; }
            public decimal Amount { get; set; }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем приложение Excel
                var excelApp = new Excel.Application();
                excelApp.Visible = false;

                // Создаем книгу и лист
                Excel.Workbook workbook = excelApp.Workbooks.Add();
                Excel.Worksheet worksheet = workbook.ActiveSheet;

                // Заголовок отчета
                worksheet.Cells[1, 1] = "💖 GLAMOUR PAYMENTS - АНАЛИТИКА ПЛАТЕЖЕЙ";
                Excel.Range titleRange = worksheet.Range["A1", "C1"];
                titleRange.Merge();
                titleRange.Font.Size = 16;
                titleRange.Font.Bold = true;
                titleRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkMagenta);
                titleRange.HorizontalAlignment = Excel.Constants.xlCenter;

                // Дата отчета
                worksheet.Cells[2, 1] = $"Отчет создан: {DateTime.Now:dd.MM.yyyy HH:mm}";
                Excel.Range dateRange = worksheet.Range["A2", "C2"];
                dateRange.Merge();
                dateRange.Font.Italic = true;
                dateRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Gray);
                dateRange.HorizontalAlignment = Excel.Constants.xlCenter;

                // Заголовки таблицы
                worksheet.Cells[4, 1] = "📊 Категория";
                worksheet.Cells[4, 2] = "💰 Сумма (руб.)";
                worksheet.Cells[4, 3] = "📈 Доля (%)";

                // Форматируем заголовки
                Excel.Range headerRange = worksheet.Range["A4", "C4"];
                headerRange.Font.Bold = true;
                headerRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                headerRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkOrchid);
                headerRange.HorizontalAlignment = Excel.Constants.xlCenter;

                // Получаем данные
                var chartData = GetChartDataForReport();
                decimal totalAmount = chartData.Sum(x => x.Amount);

                int row = 5;
                foreach (var category in chartData)
                {
                    double percentage = totalAmount > 0 ? (double)(category.Amount / totalAmount * 100) : 0;

                    worksheet.Cells[row, 1] = category.CategoryName;
                    worksheet.Cells[row, 2] = (double)category.Amount;
                    worksheet.Cells[row, 3] = percentage;

                    // Красим строки
                    if (row % 2 == 1)
                    {
                        worksheet.Range[$"A{row}", $"C{row}"].Interior.Color =
                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LavenderBlush);
                    }

                    row++;
                }

                // Итоговая строка
                worksheet.Cells[row + 1, 1] = "💰 ОБЩАЯ СУММА:";
                worksheet.Cells[row + 1, 2] = (double)totalAmount;

                Excel.Range totalRange = worksheet.Range[$"A{row + 1}", $"C{row + 1}"];
                totalRange.Font.Bold = true;
                totalRange.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkRed);
                totalRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Gold);

                // Настраиваем ширину колонок
                worksheet.Columns["A"].ColumnWidth = 25;
                worksheet.Columns["B"].ColumnWidth = 15;
                worksheet.Columns["C"].ColumnWidth = 12;

                // Форматируем числа
                worksheet.Range["B5", $"B{row + 1}"].NumberFormat = "#,##0.00\" руб.\"";
                worksheet.Range["C5", $"C{row}"].NumberFormat = "0.0\"%\"";

                // Показываем Excel
                excelApp.Visible = true;

                MessageBox.Show("📊 Данные успешно экспортированы в Excel!", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка при экспорте в Excel: {ex.Message}", "Ошибка");
            }
        }
    }
}