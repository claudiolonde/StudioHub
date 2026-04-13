using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

#pragma warning disable IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle
namespace Studio.Helpers;
#pragma warning restore IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle

/// <summary>
/// Converte un valore booleano in <see cref="Visibility"/> per l'uso in binding WPF.
/// </summary>
public class BoolToHidden : MarkupExtension, IValueConverter {

    /// <summary>
    /// Se impostato a <c>true</c>, inverte il valore booleano prima della conversione.
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// Restituisce l'istanza corrente come valore da utilizzare nel markup XAML.
    /// </summary>
    /// <param name="serviceProvider">Provider di servizi per il markup.</param>
    /// <returns>L'istanza corrente di <see cref="BoolToCollapsed"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) {
        return this;
    }

    /// <summary>
    /// Converte un valore booleano in <see cref="Visibility.Visible"/> o <see cref="Visibility.Hidden"/>.
    /// </summary>
    /// <param name="value">Valore da convertire.</param>
    /// <param name="targetType">Tipo di destinazione.</param>
    /// <param name="parameter">Parametro opzionale.</param>
    /// <param name="culture">Informazioni sulla cultura.</param>
    /// <returns><see cref="Visibility.Visible"/> se <c>true</c>, <see cref="Visibility.Collapsed"/> altrimenti.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        bool tempValue = value is true;
        if (Invert) {
            tempValue = !tempValue;
        }
        return tempValue ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// Converte un valore <see cref="Visibility"/> in booleano.
    /// </summary>
    /// <param name="value">Valore da convertire.</param>
    /// <param name="targetType">Tipo di destinazione.</param>
    /// <param name="parameter">Parametro opzionale.</param>
    /// <param name="culture">Informazioni sulla cultura.</param>
    /// <returns><c>true</c> se <see cref="Visibility.Visible"/>, <c>false</c> altrimenti.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is Visibility v && v == Visibility.Visible;
    }
}
