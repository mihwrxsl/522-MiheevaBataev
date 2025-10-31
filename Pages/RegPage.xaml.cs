using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace _522_Miheeva.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }
        // 🌟 МЕТОД ДЛЯ ХЭШИРОВАНИЯ ПАРОЛЕЙ (такой же как в AuthPage)
        public static string GetHash(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем заполнение всех полей
            if (string.IsNullOrEmpty(TextBoxLogin.Text) ||
                string.IsNullOrEmpty(TextBoxFIO.Text) ||
                string.IsNullOrEmpty(PasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("💖 Милая, заполни все поля!", "Внимание");
                return;
            }

            // Проверяем совпадение паролей
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("❌ Пароли не совпадают!", "Ошибка");
                return;
            }

            // Проверяем требования к паролю
            if (!IsPasswordValid(PasswordBox.Password))
            {
                return;
            }

            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Проверяем, не занят ли логин
                    string checkSql = "SELECT COUNT(*) FROM Users WHERE Login = @Login";
                    using (var checkCommand = new SqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Login", TextBoxLogin.Text.Trim());
                        int userCount = (int)checkCommand.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("❌ Этот логин уже занят! Выбери другой.", "Ошибка");
                            return;
                        }
                    }

                    // Получаем выбранную роль
                    string role = "User";
                    if (RoleComboBox.SelectedItem is ComboBoxItem selectedItem)
                    {
                        role = selectedItem.Tag.ToString();
                    }

                    // Добавляем нового пользователя
                    string insertSql = @"INSERT INTO Users (Login, Password, Role, FIO) 
                                     VALUES (@Login, @Password, @Role, @FIO)";
                    using (var insertCommand = new SqlCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Login", TextBoxLogin.Text.Trim());
                        insertCommand.Parameters.AddWithValue("@Password", GetHash(PasswordBox.Password));
                        insertCommand.Parameters.AddWithValue("@Role", role);
                        insertCommand.Parameters.AddWithValue("@FIO", TextBoxFIO.Text.Trim());

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"🌸 Поздравляю! Ты успешно зарегистрирована!\nТвоя роль: {role}", "Успех");
                            NavigationService?.Navigate(new AuthPage());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка регистрации: {ex.Message}", "Ошибка");
            }
        }

        // 🌟 ПРОВЕРКА ПАРОЛЯ ПО ТРЕБОВАНИЯМ
        private bool IsPasswordValid(string password)
        {
            // 1. Проверка длины
            if (password.Length < 6)
            {
                MessageBox.Show("❌ Пароль должен содержать минимум 6 символов!", "Ошибка");
                return false;
            }

            bool hasEnglish = true;
            bool hasDigit = false;

            // 2. Проверка символов
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }

        // 🌟 ОБРАБОТЧИКИ ДЛЯ PLACEHOLDER'ОВ
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(TextBoxLogin.Text)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void TextBoxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            FIOPlaceholder.Visibility = string.IsNullOrEmpty(TextBoxFIO.Text)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordPlaceholder.Visibility = string.IsNullOrEmpty(ConfirmPasswordBox.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
