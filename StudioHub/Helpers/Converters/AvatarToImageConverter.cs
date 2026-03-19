using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

#pragma warning disable IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle
namespace StudioHub.Helpers;
#pragma warning restore IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle

/// <summary>
/// Converte un valore booleano nel suo opposto. Utilizzabile direttamente in XAML senza dichiarazione nelle risorse.
/// </summary>
public class InverseBooleanConverter : MarkupExtension, IValueConverter {
    public override object ProvideValue(IServiceProvider serviceProvider) {
        return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool booleanValue && !booleanValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool booleanValue && !booleanValue;
    }
}
