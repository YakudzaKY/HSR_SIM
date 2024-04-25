using System.Globalization;
using System.Windows.Data;

namespace HSR_SIM_CLIENT.Converters;

/// <summary>
///     return !val
/// </summary>
public class ReverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }
}