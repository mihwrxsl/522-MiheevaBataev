using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace _522_Miheeva.Pages
{
    public partial class PaymentsTabPage : Page
    {
        public PaymentsTabPage()
        {
            InitializeComponent();
            LoadPayments();
        }

        private void LoadPayments()
        {
            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"SELECT p.ID, p.Date, p.Name, p.Num, p.Price, 
                                  u.FIO as UserFIO, c.Name as CategoryName,
                                  (p.Num * p.Price) as Total
                           FROM Payments p
                           INNER JOIN Users u ON p.UserID = u.ID
                           INNER JOIN Categories c ON p.CategoryID = c.ID
                           ORDER BY p.Date DESC";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var payments = new System.Collections.ObjectModel.ObservableCollection<Payment>();
                        while (reader.Read())
                        {
                            payments.Add(new Payment
                            {
                                ID = (int)reader["ID"],
                                Date = (DateTime)reader["Date"],
                                Name = reader["Name"].ToString(),
                                Num = (int)reader["Num"],
                                Price = (decimal)reader["Price"],
                                UserFIO = reader["UserFIO"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Total = (decimal)reader["Total"]
                            });
                        }
                        DataGridPayment.ItemsSource = payments; // 👈 ИСПРАВЛЕНО ИМЯ
                    }
                }

                StatusText.Text = $"💰 Загружено платежей: {DataGridPayment.Items.Count}"; // 👈 ИСПРАВЛЕНО
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка загрузки платежей: {ex.Message}", "Ошибка");
                StatusText.Text = "❌ Ошибка загрузки данных";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }

        private void BtnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditPaymentPage());
        }

        private void EditPayment_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Payment selectedPayment)
            {
                NavigationService?.Navigate(new AddEditPaymentPage(selectedPayment));
            }
        }

        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPayment.SelectedItem is Payment selectedPayment) // 👈 ИСПРАВЛЕНО
            {
                if (MessageBox.Show($"❌ Удалить платеж '{selectedPayment.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string sql = "DELETE FROM Payments WHERE ID = @ID"; // 👈 ИСПРАВЛЕНО

                            using (var command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ID", selectedPayment.ID);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("💰 Платеж успешно удален!", "Успех");
                                    LoadPayments(); // Перезагружаем таблицу
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"💔 Ошибка удаления: {ex.Message}", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("💖 Выберите платеж для удаления!", "Внимание");
            }
        }
    }

    public class Payment
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int Num { get; set; }
        public decimal Price { get; set; }
        public string UserFIO { get; set; }
        public string CategoryName { get; set; }
        public decimal Total { get; set; }
    }
}