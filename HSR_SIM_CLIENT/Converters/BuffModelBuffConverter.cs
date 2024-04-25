using System.Globalization;
using System.Windows.Data;
using HSR_SIM_CLIENT.ViewModels;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_CLIENT.Converters;

/// <summary>
///     Convert Buff into Buff view model
/// </summary>
public class BuffModelBuffConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? null : new BuffViewModel((Buff)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? null : ((BuffViewModel)value).BuffRef;
    }
}