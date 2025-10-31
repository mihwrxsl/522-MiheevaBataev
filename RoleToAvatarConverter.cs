using System;
using System.Windows.Data;

namespace _522_Miheeva.Pages
{
    public class RoleToAvatarConverter : IValueConverter
    {
        public static RoleToAvatarConverter Instance { get; } = new RoleToAvatarConverter();

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