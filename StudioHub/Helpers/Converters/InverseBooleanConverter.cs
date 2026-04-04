using System.Globalization;
using System.Windows.Data;

#pragma warning disable IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle
namespace StudioHub.Helpers;
#pragma warning restore IDE0130 // La parola chiave namespace non corrisponde alla struttura di cartelle

/// <summary>
/// Converter WPF che inverte un valore booleano.
/// </summary>
/// <remarks>
/// Questo <see cref="IValueConverter"/> restituisce <see langword="true"/> se il valore di input è
/// <see langword="false"/>; restituisce <see langword="false"/> se il valore di input è <see langword="true"/>. Se
/// <paramref name="value"/> è <see langword="null"/> o non è di tipo <see cref="System.Boolean"/>, il converter
/// restituisce <see langword="false"/>.
/// </remarks>
public class InverseBooleanConverter : IValueConverter {

    /// <summary>
    /// Converte un valore booleano invertendone il valore per il binding WPF.
    /// </summary>
    /// <param name="value">
    /// Il valore in ingresso; ci si aspetta un <see cref="System.Boolean"/>. Se non è booleano o è
    /// <see langword="null"/>, il risultato sarà <see langword="false"/>.
    /// </param>
    /// <param name="targetType">Il tipo di destinazione richiesto dallo binding; ignorato da questo converter.</param>
    /// <param name="parameter">Parametro opzionale del converter; ignorato da questo converter.</param>
    /// <param name="culture">Informazioni di cultura; ignorate da questo converter.</param>
    /// <returns>
    /// Restituisce <see langword="true"/> se <paramref name="value"/> è <see langword="false"/>; altrimenti restituisce
    /// <see langword="false"/>.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool booleanValue && !booleanValue;
    }

    /// <summary>
    /// Esegue la conversione inversa, invertendo nuovamente il valore booleano.
    /// </summary>
    /// <param name="value">
    /// Il valore in ingresso; ci si aspetta un <see cref="System.Boolean"/>. Se non è booleano o è
    /// <see langword="null"/>, il risultato sarà <see langword="false"/>.
    /// </param>
    /// <param name="targetType">Il tipo di destinazione richiesto dallo binding; ignorato da questo converter.</param>
    /// <param name="parameter">Parametro opzionale del converter; ignorato da questo converter.</param>
    /// <param name="culture">Informazioni di cultura; ignorate da questo converter.</param>
    /// <returns>
    /// Restituisce <see langword="true"/> se <paramref name="value"/> è <see langword="false"/>; altrimenti restituisce
    /// <see langword="false"/>.
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool booleanValue && !booleanValue;
    }
}
