using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StudioHub.Helpers;

/// <summary>
/// Converte una stringa vuota in <see cref="Visibility"/> per l'uso in binding WPF.
/// </summary>
public class StringToCollapsed : MarkupExtension, IValueConverter {

    /// <summary>
    /// Se impostato a <c> true</c>, inverte il valore booleano prima della conversione.
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// Restituisce questa istanza per l'utilizzo come estensione di markup.
    /// </summary>
    /// <param name="serviceProvider">Fornitore di servizi per la risoluzione dei servizi.</param>
    /// <returns>Restituisce l'istanza corrente di <see cref="StringToCollapsed"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) {
        return this;
    }

    /// <summary>
    /// Converte una stringa in un valore <see cref="Visibility"/> . Restituisce <see cref="Visibility.Visible"/> se la
    /// stringa non è vuota o nulla, altrimenti <see cref="Visibility.Collapsed"/> . Se la proprietà
    /// <see cref="Invert"/> è impostata a <c> true</c>, il risultato viene invertito.
    /// </summary>
    /// <param name="value">Il valore da convertire, atteso come <see cref="string"/>.</param>
    /// <param name="targetType">Il tipo di destinazione della conversione (ignorato).</param>
    /// <param name="parameter">Parametro aggiuntivo per la conversione (ignorato).</param>
    /// <param name="culture">Informazioni sulla cultura da utilizzare nella conversione (ignorato).</param>
    /// <returns>
    /// Un valore <see cref="Visibility"/> basato sul contenuto della stringa e sulla proprietà <see cref="Invert"/> .
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        bool tempValue = string.IsNullOrEmpty(value as string);
        if (Invert) {
            tempValue = !tempValue;
        }
        return tempValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}