using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _522_Miheeva.Pages
{
    public partial class AddEditPaymentPage : Page
    {
        private int _paymentId = 0; // 0 = новый платеж, >0 = редактирование
        private Payment _editingPayment = null;

        public AddEditPaymentPage(Payment payment = null)
        {
            InitializeComponent();

            // Загружаем данные для комбобоксов
            LoadComboBoxData();

            if (payment != null)
            {
                // Режим редактирования
                _paymentId = payment.ID;
                _editingPayment = payment;
                PageTitle.Text = "✏️ Редактирование платежа";

                // Заполняем поля
                TxtName.Text = payment.Name;
                DpDate.SelectedDate = payment.Date;
                TxtNum.Text = payment.Num.ToString();
                TxtPrice.Text = payment.Price.ToString("F2");

                StatusText.Text = "🌸 Редактирование платежа";
            }
            else
            {
                // Новый платеж - устанавливаем сегодняшнюю дату
                DpDate.SelectedDate = DateTime.Today;
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Загружаем пользователей
                    string usersSql = "SELECT ID, FIO FROM Users";
                    using (var command = new SqlCommand(usersSql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var users = new System.Collections.ObjectModel.ObservableCollection<UserComboItem>();
                        while (reader.Read())
                        {
                            users.Add(new UserComboItem
                            {
                                ID = (int)reader["ID"],
                                FIO = reader["FIO"].ToString()
                            });
                        }
                        CmbUser.ItemsSource = users;
                    }

                    // Загружаем категории
                    string categoriesSql = "SELECT ID, Name FROM Categories";
                    using (var command = new SqlCommand(categoriesSql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var categories = new System.Collections.ObjectModel.ObservableCollection<CategoryComboItem>();
                        while (reader.Read())
                        {
                            categories.Add(new CategoryComboItem
                            {
                                ID = (int)reader["ID"],
                                Name = reader["Name"].ToString()
                            });
                        }
                        CmbCategory.ItemsSource = categories;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PaymentsTabPage());
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PaymentsTabPage());
        }

        // Проверка ввода только цифр
        private void TxtNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        // Проверка ввода только чисел (с точкой)
        private void TxtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                    CmbUser.SelectedItem == null ||
                    CmbCategory.SelectedItem == null ||
                    DpDate.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(TxtNum.Text) ||
                    string.IsNullOrWhiteSpace(TxtPrice.Text))
                {
                    MessageBox.Show("💖 Заполните все поля!", "Внимание");
                    return;
                }

                // Получаем выбранные значения
                var selectedUser = (UserComboItem)CmbUser.SelectedItem;
                var selectedCategory = (CategoryComboItem)CmbCategory.SelectedItem;

                int num = int.Parse(TxtNum.Text);
                decimal price = decimal.Parse(TxtPrice.Text, CultureInfo.InvariantCulture);

                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (_paymentId == 0)
                    {
                        // Добавление нового платежа
                        string sql = @"INSERT INTO Payments (CategoryID, UserID, Date, Name, Num, Price) 
                                     VALUES (@CategoryID, @UserID, @Date, @Name, @Num, @Price)";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@CategoryID", selectedCategory.ID);
                            command.Parameters.AddWithValue("@UserID", selectedUser.ID);
                            command.Parameters.AddWithValue("@Date", DpDate.SelectedDate);
                            command.Parameters.AddWithValue("@Name", TxtName.Text.Trim());
                            command.Parameters.AddWithValue("@Num", num);
                            command.Parameters.AddWithValue("@Price", price);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("💰 Платеж успешно добавлен!", "Успех");
                            }
                        }
                    }
                    else
                    {
                        // Редактирование существующего платежа
                        string sql = @"UPDATE Payments 
                                     SET CategoryID = @CategoryID, UserID = @UserID, 
                                         Date = @Date, Name = @Name, Num = @Num, Price = @Price 
                                     WHERE ID = @ID";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ID", _paymentId);
                            command.Parameters.AddWithValue("@CategoryID", selectedCategory.ID);
                            command.Parameters.AddWithValue("@UserID", selectedUser.ID);
                            command.Parameters.AddWithValue("@Date", DpDate.SelectedDate);
                            command.Parameters.AddWithValue("@Name", TxtName.Text.Trim());
                            command.Parameters.AddWithValue("@Num", num);
                            command.Parameters.AddWithValue("@Price", price);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("💰 Платеж успешно обновлен!", "Успех");
                            }
                        }
                    }
                }

                NavigationService?.Navigate(new PaymentsTabPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }

    // Вспомогательные классы для комбобоксов
    public class UserComboItem
    {
        public int ID { get; set; }
        public string FIO { get; set; }
    }

    public class CategoryComboItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}