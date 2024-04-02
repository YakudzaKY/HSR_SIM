using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_CLIENT.Converters;

/// <summary>
/// return !val
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