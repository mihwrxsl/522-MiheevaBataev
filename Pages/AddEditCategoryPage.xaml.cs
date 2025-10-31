using System;
using System.Data.SqlClient;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AddEditCategoryPage.xaml
    /// </summary>
    public partial class AddEditCategoryPage : Page
    {
        private int _categoryId = 0; // 0 = новая категория, >0 = редактирование
        private Category _editingCategory = null;

        public AddEditCategoryPage(Category category = null)
        {
            InitializeComponent();

            if (category != null)
            {
                // Режим редактирования
                _categoryId = category.ID;
                _editingCategory = category;
                PageTitle.Text = "✏️ Редактирование категории";
                TxtCategoryName.Text = category.Name;
                StatusText.Text = "🌸 Редактирование категории";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка заполнения поля
                if (string.IsNullOrWhiteSpace(TxtCategoryName.Text))
                {
                    MessageBox.Show("💖 Введите название категории!", "Внимание");
                    TxtCategoryName.Focus();
                    return;
                }

                string categoryName = TxtCategoryName.Text.Trim();
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (_categoryId == 0)
                    {
                        // Добавление новой категории
                        string sql = @"INSERT INTO Categories (Name) VALUES (@Name)";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Name", categoryName);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("🌸 Категория успешно добавлена!", "Успех");
                            }
                        }
                    }
                    else
                    {
                        // Редактирование существующей категории
                        string sql = @"UPDATE Categories SET Name = @Name WHERE ID = @ID";

                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ID", _categoryId);
                            command.Parameters.AddWithValue("@Name", categoryName);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("🌸 Категория успешно обновлена!", "Успех");
                            }
                        }
                    }
                }

                NavigationService?.Navigate(new CategoryTabPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}