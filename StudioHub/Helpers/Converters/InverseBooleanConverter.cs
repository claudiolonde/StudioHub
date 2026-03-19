using System.Globalization;
using System.Windows.Data;

namespace StudioHub.Helpers.Converters;

/// <summary>
/// Converte un valore booleano nel suo opposto. Utile per legare la proprietà IsEnabled di un controllo allo stato
/// Unchecked di una CheckBox.
/// </summary>
public class InverseBooleanConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool booleanValue ? !booleanValue : false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool booleanValue ? !booleanValue : false;
    }
}
