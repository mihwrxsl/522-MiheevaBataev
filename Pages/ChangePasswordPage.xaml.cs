using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace _522_Miheeva.Pages
{
    public partial class ChangePasswordPage : Page
    {
        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) ||
                string.IsNullOrWhiteSpace(TxtCurrentPassword.Password) ||
                string.IsNullOrWhiteSpace(TxtNewPassword.Password) ||
                string.IsNullOrWhiteSpace(TxtConfirmPassword.Password))
            {
                MessageBox.Show("💖 Милая, заполни все поля!", "Внимание");
                return;
            }

            // Проверка совпадения новых паролей
            if (TxtNewPassword.Password != TxtConfirmPassword.Password)
            {
                MessageBox.Show("❌ Новые пароли не совпадают!", "Ошибка");
                return;
            }

            // Проверка требований к новому паролю
            if (!IsPasswordValid(TxtNewPassword.Password))
            {
                return;
            }

            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем старый пароль
                    string checkSql = "SELECT Password FROM Users WHERE Login = @Login";
                    string currentHashedPassword = AuthPage.GetHash(TxtCurrentPassword.Password);

                    using (var checkCommand = new SqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Login", TxtLogin.Text.Trim());
                        string dbPassword = checkCommand.ExecuteScalar()?.ToString();

                        if (dbPassword != currentHashedPassword)
                        {
                            MessageBox.Show("❌ Старый пароль неверен!", "Ошибка");
                            return;
                        }
                    }

                    // Обновляем пароль
                    string updateSql = "UPDATE Users SET Password = @NewPassword WHERE Login = @Login";
                    string newHashedPassword = AuthPage.GetHash(TxtNewPassword.Password);

                    using (var updateCommand = new SqlCommand(updateSql, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Login", TxtLogin.Text.Trim());
                        updateCommand.Parameters.AddWithValue("@NewPassword", newHashedPassword);

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("🌸 Пароль успешно изменен! Теперь войди с новым паролем!", "Успех");
                            NavigationService?.Navigate(new AuthPage());
                        }
                        else
                        {
                            MessageBox.Show("❌ Пользователь с таким логином не найден!", "Ошибка");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка смены пароля: {ex.Message}", "Ошибка");
            }
        }

        // Метод проверки пароля (такой же как в RegPage)
        private bool IsPasswordValid(string password)
        {
            if (password.Length < 6)
            {
                MessageBox.Show("❌ Пароль должен содержать минимум 6 символов!", "Ошибка");
                return false;
            }

            bool hasEnglish = true;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (c >= '0' && c <= '9')
                {
                    hasDigit = true;
                }
                else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                {
                    hasEnglish = false;
                }
            }

            if (!hasEnglish)
            {
                MessageBox.Show("❌ Используй только английские буквы!", "Ошибка");
                return false;
            }

            if (!hasDigit)
            {
                MessageBox.Show("❌ Добавь хотя бы одну цифру!", "Ошибка");
                return false;
            }

            return true;
        }
    }
}