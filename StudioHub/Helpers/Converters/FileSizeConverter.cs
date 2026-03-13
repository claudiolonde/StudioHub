using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

#pragma warning disable IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle
namespace StudioHub.Helpers;
#pragma warning restore IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle

/// <summary>
/// Converte una dimensione di file espressa in byte in una stringa leggibile dall'utente,
/// utilizzando le unità più appropriate (Byte, KB, MB, GB, TB, PB).
/// </summary>
public class FileSizeConverter : MarkupExtension, IValueConverter {

    /// <summary>
    /// Array delle unità di misura supportate per la dimensione dei file.
    /// </summary>
    static readonly string[] SIZES = ["Byte", "KB", "MB", "GB", "TB", "PB"];

    /// <summary>
    /// Converte una dimensione di file (in byte) in una stringa formattata con l'unità più adatta.
    /// </summary>
    /// <param name="value">Valore della dimensione del file da convertire.</param>
    /// <param name="targetType">Tipo di destinazione della conversione (ignorato).</param>
    /// <param name="parameter">Parametro aggiuntivo per la conversione (ignorato).</param>
    /// <param name="culture">Informazioni sulla cultura da utilizzare per la conversione.</param>
    /// <returns>
    /// Una stringa rappresentante la dimensione del file in formato leggibile (es. "1.23 MB"),
    /// oppure una stringa vuota se il valore è <see langword="null" />.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        if (value is null) {
            return string.Empty;
        }

        double size;
        try {
            size = System.Convert.ToDouble(value, culture);
        }
        catch {
            return value;
        }

        int index = 0;
        while (size >= 1024 && index < SIZES.Length - 1) {
            size /= 1024;
            index++;
        }

        return index == 0
            ? $"{size:0} {SIZES[index]}"
            : $"{size:0.##} {SIZES[index]}";
    }

    /// <summary>
    /// Restituisce l'istanza corrente del convertitore per l'utilizzo in XAML.
    /// </summary>
    /// <param name="serviceProvider">Provider di servizi per la risoluzione dei servizi.</param>
    /// <returns>L'istanza corrente di <see cref="FileSizeConverter"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) {
        return this;
    }

    /// <summary>
    /// Metodo non implementato per la conversione inversa; restituisce semplicemente il valore ricevuto.
    /// </summary>
    /// <param name="value">Valore da convertire indietro.</param>
    /// <param name="targetType">Tipo di destinazione della conversione inversa.</param>
    /// <param name="parameter">Parametro aggiuntivo per la conversione inversa.</param>
    /// <param name="culture">Informazioni sulla cultura da utilizzare per la conversione inversa.</param>
    /// <returns>Il valore ricevuto senza modifiche.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value;
    }
}
