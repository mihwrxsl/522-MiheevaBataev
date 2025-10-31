using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace _522_Miheeva.Pages
{
    public partial class AddEditUserPage : Page
    {
        private int _userId = 0; // 0 = новый пользователь, >0 = редактирование
        private User _editingUser = null;

        public AddEditUserPage(User user = null)
        {
            InitializeComponent();

            if (user != null)
            {
                // Режим редактирования
                _userId = user.ID;
                _editingUser = user;
                PageTitle.Text = "✏️ Редактирование пользователя";
                TxtLogin.Text = user.Login;
                TxtFIO.Text = user.FIO;

                // Выбираем роль в комбобоксе
                foreach (ComboBoxItem item in CmbRole.Items)
                {
                    if (item.Content.ToString() == user.Role)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }

                StatusText.Text = "🌸 Редактирование пользователя";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UsersTabPage());
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UsersTabPage());
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения полей
                if (string.IsNullOrWhiteSpace(TxtLogin.Text) ||
                    string.IsNullOrWhiteSpace(TxtPassword.Password) ||
                    string.IsNullOrWhiteSpace(TxtFIO.Text))
                {
                    MessageBox.Show("💖 Заполните все поля!", "Внимание");
                    return;
                }

                string role = ((ComboBoxItem)CmbRole.SelectedItem).Content.ToString();
                string password = TxtPassword.Password;

                // Хэшируем пароль (используем метод из AuthPage)
                string hashedPassword = AuthPage.GetHash(password);

                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (_userId == 0)
                    {
                        // Добавление нового пользователя
                        string sql = @"INSERT INTO Users (Login, Password, Role, FIO) 
                                     VALUES (@Login, @Password, @Role, @FIO)";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Login", TxtLogin.Text.Trim());
                            command.Parameters.AddWithValue("@Password", hashedPassword);
                            command.Parameters.AddWithValue("@Role", role);
                            command.Parameters.AddWithValue("@FIO", TxtFIO.Text.Trim());

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("🌸 Пользователь успешно добавлен!", "Успех");
                            }
                        }
                    }
                    else
                    {
                        // Редактирование существующего пользователя
                        string sql = @"UPDATE Users 
                                     SET Login = @Login, Password = @Password, Role = @Role, FIO = @FIO 
                                     WHERE ID = @ID";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ID", _userId);
                            command.Parameters.AddWithValue("@Login", TxtLogin.Text.Trim());
                            command.Parameters.AddWithValue("@Password", hashedPassword);
                            command.Parameters.AddWithValue("@Role", role);
                            command.Parameters.AddWithValue("@FIO", TxtFIO.Text.Trim());

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("🌸 Пользователь успешно обновлен!", "Успех");
                            }
                        }
                    }
                }

                NavigationService?.Navigate(new UsersTabPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}