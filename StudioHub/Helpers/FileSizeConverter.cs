using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace StudioHub.Helpers;

/// <summary>
/// Converte una dimensione di file (in KB) in una stringa formattata per la visualizzazione.
/// </summary>
public class FileSizeConverter : MarkupExtension, IValueConverter {

    /// <summary>
    /// Converte una dimensione di file (long) in una stringa formattata con l'unità "KB".
    /// </summary>
    /// <param name="value">La dimensione del file in KB.</param>
    /// <param name="targetType">Il tipo di destinazione della conversione.</param>
    /// <param name="parameter">Parametro opzionale per la conversione (non utilizzato).</param>
    /// <param name="culture">Informazioni sulla cultura per la formattazione.</param>
    /// <returns>
    /// Una stringa formattata che rappresenta la dimensione del file in KB, oppure il valore originale se non è un
    /// long.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value == null) { return string.Empty; }

        double size;
        try { size = System.Convert.ToDouble(value, culture); }
        catch { return value; }

        string[] sizes = ["Byte", "KB", "MB", "GB", "TB", "PB"];
        int index = 0;
        while (size >= 1024 && index < sizes.Length - 1) {
            index++;
            size /= 1024;
        }
        string format = index == 0 ? "{0:0} {1}" : "{0:0.##} {1}";
        return string.Format(culture, format, size, sizes[index]);
    }

    /// <summary>
    /// Restituisce l'istanza corrente come valore da utilizzare nel markup XAML.
    /// </summary>
    /// <param name="serviceProvider">Provider di servizi per il markup.</param>
    /// <returns>L'istanza corrente di <see cref="BoolToCollapse"/>.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider) {
        return this;
    }

    /// <summary>
    /// Non implementato. Solleva sempre un'eccezione <see cref="NotImplementedException"/> .
    /// </summary>
    /// <param name="value">Valore da convertire.</param>
    /// <param name="targetType">Tipo di destinazione della conversione.</param>
    /// <param name="parameter">Parametro opzionale per la conversione.</param>
    /// <param name="culture">Informazioni sulla cultura per la formattazione.</param>
    /// <returns>Non restituisce mai un valore.</returns>
    /// <exception cref="NotImplementedException">Sempre sollevata.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}