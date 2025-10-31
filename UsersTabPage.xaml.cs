using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace _522_Miheeva.Pages
{
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT ID, Login, Password, Role, FIO FROM Users";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var users = new System.Collections.ObjectModel.ObservableCollection<User>();
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                ID = (int)reader["ID"],
                                Login = reader["Login"].ToString(),
                                Password = reader["Password"].ToString(),
                                Role = reader["Role"].ToString(),
                                FIO = reader["FIO"].ToString()
                            });
                        }
                        UsersDataGrid.ItemsSource = users;
                    }
                }

                StatusText.Text = $"🌸 Загружено пользователей: {UsersDataGrid.Items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
                StatusText.Text = "❌ Ошибка загрузки данных";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditUserPage());
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is User selectedUser)
            {
                NavigationService?.Navigate(new AddEditUserPage(selectedUser));
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show($"❌ Удалить пользователя {selectedUser.FIO}?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string sql = "DELETE FROM Users WHERE ID = @ID";

                            using (var command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ID", selectedUser.ID);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("🌸 Пользователь успешно удален!", "Успех");
                                    LoadUsers(); // Перезагружаем таблицу
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
                MessageBox.Show("💖 Выберите пользователя для удаления!", "Внимание");
            }
        }
    }
}