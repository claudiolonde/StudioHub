using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StudioHub.Controls;

/// <summary>   
/// Rappresenta un controllo di etichetta personalizzato per WPF che estende <see cref="TextBlock"/>.
/// Supporta la visualizzazione di testo e un indicatore visivo per i campi obbligatori.
/// </summary>
public class Label : TextBlock {
    /// <summary>
    /// Pennello di default per il testo della label.
    /// </summary>
    private static readonly Brush DefaultForeground = Brushes.Gray;

    /// <summary>
    /// Pennello di default per l'indicatore di campo obbligatorio.
    /// </summary>
    private static readonly Brush DefaultRequiredBrush = Brushes.Pink;

    /// <summary>
    /// Inizializza una nuova istanza della classe <see cref="Label"/> con le proprietà di stile predefinite.
    /// </summary>
    public Label() {
        FontSize = 12;
        Foreground = DefaultForeground;
        Margin = new Thickness(0, 0, 8, 0);
        Focusable = false;
        VerticalAlignment = VerticalAlignment.Center;
    }

    /// <summary>
    /// Gestisce la modifica delle proprietà <see cref="Text"/> e <see cref="IsRequired"/>, aggiornando la visualizzazione.
    /// </summary>
    /// <param name="d">L'oggetto di dipendenza.</param>
    /// <param name="e">I dati dell'evento di modifica.</param>
    private static void OnRequiredPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((Label)d).UpdateInlines();
    }

    /// <summary>
    /// Identifica la proprietà di dipendenza <see cref="Text"/>.
    /// </summary>
    public static new readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(Label),
            new PropertyMetadata(string.Empty, OnRequiredPropertyChanged)
        );

    /// <summary>
    /// Ottiene o imposta il testo visualizzato dalla label.
    /// </summary>
    public new string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Identifica la proprietà di dipendenza <see cref="IsRequired"/>.
    /// </summary>
    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(Label),
            new PropertyMetadata(false, OnRequiredPropertyChanged)
        );

    /// <summary>
    /// Ottiene o imposta un valore che indica se la label rappresenta un campo obbligatorio.
    /// </summary>
    public bool IsRequired {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// Aggiorna la collezione per riflettere il testo e l'indicatore di obbligatorietà.
    /// </summary>
    private void UpdateInlines() {

        Brush secondaryBrush = TryFindResource("TextFillColorTertiaryBrush") as Brush ?? DefaultForeground;
        if (!ReferenceEquals(Foreground, secondaryBrush)) { Foreground = secondaryBrush; }

        Inlines.Clear();

        string text = Text;
        if (!string.IsNullOrEmpty(text)) { Inlines.Add(new Run(text)); }

        if (IsEnabled && IsRequired) {
            Brush requiredBrush = TryFindResource("SystemFillColorCriticalBrush") as Brush ?? DefaultRequiredBrush;
            Inlines.Add(new Run(" *") {
                FontWeight = FontWeights.Bold,
                Foreground = requiredBrush
            }
            );
        }
    }
}