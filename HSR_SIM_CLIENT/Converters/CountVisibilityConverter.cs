using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HSR_SIM_CLIENT.Converters;

public class CountVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable ie && ie.Cast<object?>().Any())

            return Visibility.Visible;


        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new Exception("cant covert back");
    }
}