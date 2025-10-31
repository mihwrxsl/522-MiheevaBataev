using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _522_Miheeva.Pages
{
    public partial class UserPage : Page
    {
        private List<User> allUsers = new List<User>();

        public UserPage()
        {
            try
            {
                InitializeComponent();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка создания страницы: {ex.Message}", "Ошибка");
            }
        }

        private void LoadUsers()
        {
            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT Login, FIO, Role, Photo FROM Users";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        allUsers.Clear();
                        while (reader.Read())
                        {
                            allUsers.Add(new User
                            {
                                Login = reader["Login"].ToString(),
                                FIO = reader["FIO"].ToString(),
                                Role = reader["Role"].ToString(),
                                Photo = reader["Photo"] as byte[]
                            });
                        }
                    }
                }

                UpdateUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateUsers()
        {
            try
            {
                if (ListUser == null) return;

                var filteredUsers = allUsers.AsEnumerable();

                // Фильтрация по ФИО
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox?.Text))
                {
                    filteredUsers = filteredUsers.Where(u =>
                        u.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower()));
                }

                // Фильтрация по роли
                if (onlyAdminCheckBox?.IsChecked == true)
                {
                    filteredUsers = filteredUsers.Where(u => u.Role == "Admin");
                }

                // Сортировка
                if (sortComboBox?.SelectedIndex == 0) // По возрастанию
                {
                    filteredUsers = filteredUsers.OrderBy(u => u.FIO);
                }
                else // По убыванию
                {
                    filteredUsers = filteredUsers.OrderByDescending(u => u.FIO);
                }

                ListUser.ItemsSource = filteredUsers.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка обновления списка: {ex.Message}", "Ошибка");
            }
        }

        // Обработчики событий
        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
            UpdateUsers();
        }
    }

    // Вспомогательный класс для отображения
    public class User
    {
        public int ID { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public string FIO { get; set; }
        public string Role { get; set; }
        public byte[] Photo { get; set; }
    }
}