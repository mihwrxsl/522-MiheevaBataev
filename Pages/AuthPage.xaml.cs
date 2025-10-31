using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
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
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;
        private string currentCaptcha = "";

        public AuthPage()
        {
            InitializeComponent();
        }

        // 🌟 МЕТОД ДЛЯ ХЭШИРОВАНИЯ ПАРОЛЕЙ
        public static string GetHash(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // 🌟 МЕТОД ДЛЯ ГЕНЕРАЦИИ КАПЧИ
        private void GenerateCaptcha()
        {
            // 🌟 ИСПОЛЬЗУЕМ ТОЛЬКО ЗАГЛАВНЫЕ БУКВЫ И ЦИФРЫ (чтобы избежать проблем с регистром)
            string allowchar = "A,B,C,D,E,F,G,H,J,K,L,M,N,P,Q,R,S,T,U,V,W,X,Y,Z"; // убрали сложные буквы
            allowchar += "2,3,4,5,6,7,8,9"; // убрали 0,1,I,O которые можно перепутать

            char[] separator = { ',' };
            string[] arr = allowchar.Split(separator);
            string captcha = "";

            Random random = new Random();
            for (int i = 0; i < 6; i++) // генерируем 6 символов
            {
                string temp = arr[random.Next(0, arr.Length)];
                captcha += temp;
            }

            currentCaptcha = captcha;
            CaptchaText.Text = captcha;

            // 🌟 ОЧИСТКА ПОЛЯ ВВОДА ПРИ ОБНОВЛЕНИИ КАПЧИ
            CaptchaInput.Text = "";
            CaptchaInput.Focus();
        }

        // 🌟 ПОКАЗАТЬ/СКРЫТЬ КАПЧУ
        private void ToggleCaptcha(bool show)
        {
            if (show)
            {
                MainAuthPanel.Visibility = Visibility.Collapsed;
                CaptchaPanel.Visibility = Visibility.Visible;
                GenerateCaptcha();
            }
            else
            {
                MainAuthPanel.Visibility = Visibility.Visible;
                CaptchaPanel.Visibility = Visibility.Collapsed;
                CaptchaInput.Text = "";
            }
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            // 🌟 ЕСЛИ КАПЧА ВКЛЮЧЕНА - просто напоминаем
            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                MessageBox.Show("💖 Милая, сначала пройди капчу через кнопку 'Продолжить'!", "Внимание");
                return;
            }

            // 🌟 ОСНОВНАЯ ПРОВЕРКА ВХОДА (только когда капчи нет)
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("💖 Милая, заполни все поля!", "Внимание");
                return;
            }

            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Login, Password, Role, FIO FROM Users WHERE Login = @Login";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Login", TextBoxLogin.Text.Trim());

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbLogin = reader["Login"].ToString();
                                string dbPassword = reader["Password"].ToString();
                                string dbRole = reader["Role"].ToString();
                                string dbFIO = reader["FIO"].ToString();

                                string hashedPassword = GetHash(PasswordBox.Password);

                                if (dbPassword == hashedPassword)
                                {
                                    string roleMessage = dbRole == "Admin" ? "💎 Привет, администратор!" : "🌸 Привет, красотка!";
                                    string name = dbFIO.Split(' ')[1];

                                    MessageBox.Show($"{roleMessage}\nРады видеть тебя, {name}!", "Добро пожаловать");

                                    // 🌟 СБРОС СЧЕТЧИКА ПРИ УСПЕШНОМ ВХОДЕ
                                    failedAttempts = 0;
                                    ToggleCaptcha(false);

                                    if (dbRole == "Admin")
                                    {
                                        NavigationService?.Navigate(new AdminPage());
                                    }
                                    else
                                    {
                                        NavigationService?.Navigate(new UserPage());
                                    }
                                }
                                else
                                {
                                    HandleFailedLogin();
                                }
                            }
                            else
                            {
                                HandleFailedLogin();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка подключения: {ex.Message}", "Ошибка");
            }
        }

        // 🌟 ОБРАБОТКА НЕУДАЧНОЙ ПОПЫТКИ ВХОДА
        private void HandleFailedLogin()
        {
            failedAttempts++;
            // 🌟 ОТЛАДОЧНОЕ СООБЩЕНИЕ
            MessageBox.Show($"❌ Неверный логин или пароль! Попытка: {failedAttempts}/3", "Ошибка");

            if (failedAttempts >= 3)
            {
                ToggleCaptcha(true);
            }
        }

        // 🌟 ОБРАБОТЧИКИ ДЛЯ КАПЧИ
        private void RefreshCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void CaptchaInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        // 🌟 ОСТАЛЬНЫЕ МЕТОДЫ
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePasswordPage());
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(TextBoxLogin.Text)
                ? Visibility.Visible : Visibility.Hidden;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible : Visibility.Hidden;
        }

        // 🌟 ОБРАБОТЧИК ДЛЯ ПОДТВЕРЖДЕНИЯ КАПЧИ
        private void SubmitCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            // 🌟 ОЧИСТКА ОТ ЛИШНИХ ПРОБЕЛОВ И ПРИВЕДЕНИЕ К ВЕРХНЕМУ РЕГИСТРУ
            string userInput = CaptchaInput.Text.Trim().ToUpper();
            string correctCaptcha = currentCaptcha.ToUpper();


            if (userInput != correctCaptcha)
            {
                MessageBox.Show("❌ Неверно введена капча! Попробуй еще раз.", "Ошибка");
                GenerateCaptcha();
                CaptchaInput.Text = "";
                CaptchaInput.Focus();
                return;
            }

            // 🌟 КАПЧА ПРОЙДЕНА УСПЕШНО
            MessageBox.Show("✅ Капча пройдена успешно! Можешь войти в систему.", "Успех");
            ToggleCaptcha(false);
            failedAttempts = 0; // Сбрасываем счетчик

            // 🌟 ОЧИСТКА ПОЛЕЙ И ВОЗВРАТ К ФОРМЕ
            TextBoxLogin.Clear();
            PasswordBox.Clear();
            TextBoxLogin.Focus();
        }
    }
}