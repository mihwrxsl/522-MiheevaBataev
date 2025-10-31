using System;
using System.Windows.Data;

namespace _522_Miheeva.Pages
{
    public class RoleToIconConverter : IValueConverter
    {
        public static RoleToIconConverter Instance { get; } = new RoleToIconConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string role)
            {
                return role == "Admin" ? "💎" : "🌸";
            }
            return "👤";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}