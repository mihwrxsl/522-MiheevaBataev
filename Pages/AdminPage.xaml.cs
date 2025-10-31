using System;
using System.Windows;
using System.Windows.Controls;

namespace _522_Miheeva.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService?.Navigate(new UsersTabPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        private void BtnPayments_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PaymentsTabPage());
        }

        private void BtnDiagrams_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChartsPage());
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }

        private void BtnTab2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }
    }
}