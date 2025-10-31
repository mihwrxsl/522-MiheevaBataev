using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace _522_Miheeva.Pages
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT ID, Name FROM Categories";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        var categories = new System.Collections.ObjectModel.ObservableCollection<Category>();
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                ID = (int)reader["ID"],
                                Name = reader["Name"].ToString()
                            });
                        }
                        CategoriesDataGrid.ItemsSource = categories;
                    }
                }

                StatusText.Text = $"🌸 Загружено категорий: {CategoriesDataGrid.Items.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка загрузки категорий: {ex.Message}", "Ошибка");
                StatusText.Text = "❌ Ошибка загрузки данных";
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddEditCategoryPage());
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Category selectedCategory)
            {
                NavigationService?.Navigate(new AddEditCategoryPage(selectedCategory));
            }
        }

        private void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                if (MessageBox.Show($"❌ Удалить категорию '{selectedCategory.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        string connectionString = "Data Source=БРБРБРРР\\SQLEXPRESS;Initial Catalog=Miheeva_DB_Payment;Integrated Security=True";

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            string sql = "DELETE FROM Categories WHERE ID = @ID";

                            using (var command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ID", selectedCategory.ID);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("🌸 Категория успешно удалена!", "Успех");
                                    LoadCategories(); // Перезагружаем таблицу
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
                MessageBox.Show("💖 Выберите категорию для удаления!", "Внимание");
            }
        }
    }

    // Класс для отображения категорий
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}