using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace StudioHub.Views;

/// <summary>
/// Tipi di dialog disponibili.
/// </summary>
public enum DialogType {
    /// <summary>
    /// Dialog di errore.
    /// </summary>
    Error,
    /// <summary>
    /// Dialog di avviso.
    /// </summary>
    Warning,
    /// <summary>
    /// Dialog di domanda.
    /// </summary>
    Question,
    /// <summary>
    /// Dialog informativo.
    /// </summary>
    Info,
    /// <summary>
    /// Nessun tipo specificato.
    /// </summary>
    None
}

/// <summary>
/// Finestra di dialogo personalizzata per messaggi e interazione.
/// </summary>
public partial class Dialog {

    private bool _haveDefault = false;
    private bool _haveCancel = false;
    private int _selectedIndex = -1;

    /// <summary>
    /// Mostra una finestra di dialogo modale.
    /// </summary>
    /// <param name="message">Messaggio da visualizzare.</param>
    /// <param name="title">Titolo della finestra (opzionale).</param>
    /// <param name="buttons">Array di pulsanti (opzionale).</param>
    /// <param name="type">Tipo di dialog (opzionale).</param>
    /// <returns>Indice del pulsante selezionato.</returns>
    public static int Show(string message,
                           string? title = null,
                           string[]? buttons = null,
                           DialogType type = DialogType.None) {

        buttons ??= ["*Chiudi"];
        Dialog dialog = new(message, title, buttons, type) {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)
        };

        dialog.ShowDialog();
        return dialog._selectedIndex;
    }

    /// <summary>
    /// Messaggio visualizzato nel dialog.
    /// </summary>
    public string Message {
        get;
    }

    /// <summary>
    /// Simbolo associato al tipo di dialog.
    /// </summary>
    public SymbolRegular Symbol {
        get;
    }

    /// <summary>
    /// Pennello per il simbolo del dialog.
    /// </summary>
    public Brush SymbolBrush {
        get;
    }

    /// <summary>
    /// Pulsanti visualizzati nel dialog.
    /// </summary>
    public Button[] Buttons {
        get;
    }

    /// <summary>
    /// Costruttore privato. Inizializza il dialog con i parametri specificati.
    /// </summary>
    /// <param name="message">Messaggio da visualizzare.</param>
    /// <param name="title">Titolo della finestra.</param>
    /// <param name="buttons">Array di pulsanti.</param>
    /// <param name="type">Tipo di dialog.</param>
    private Dialog(string message,
                   string? title,
                   string[] buttons,
                   DialogType type) {

        InitializeComponent();
        DataContext = this;

        Message = message;
        Title = title ??
                (Application.Current.MainWindow?.Title) ??
                (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name) ??
                string.Empty;
        Buttons = [.. buttons.Where(s => !string.IsNullOrWhiteSpace(s)).Select(createButton)];
        switch (type) {
            case DialogType.Error:
                Symbol = SymbolRegular.ErrorCircle24;
                SymbolBrush = Brushes.Crimson;
                break;
            case DialogType.Warning:
                Symbol = SymbolRegular.Warning24;
                SymbolBrush = Brushes.Orange;
                break;
            case DialogType.Question:
                Symbol = SymbolRegular.QuestionCircle24;
                SymbolBrush = Brushes.MediumSlateBlue;
                break;
            case DialogType.Info:
                Symbol = SymbolRegular.Info24;
                SymbolBrush = Brushes.DeepSkyBlue;
                break;
            default:
                Symbol = SymbolRegular.Empty;
                SymbolBrush = Brushes.Transparent;
                break;
        }

        Loaded += (s, e) => SizeToContent = SizeToContent.WidthAndHeight;
        MouseLeftButtonDown += (s, e) => {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        };
    }

    /// <summary>
    /// Crea un pulsante per il dialog in base al testo e all'indice.
    /// </summary>
    /// <param name="text">Testo del pulsante (può includere prefisso per default/cancel).</param>
    /// <param name="index">Indice del pulsante.</param>
    /// <returns>Istanza di <see cref="Button"/> configurata.</returns>
    private Button createButton(string text, int index) {

        Button newButton = new() {
            Margin = new Thickness(10, 0, 10, 0),
            Padding = new Thickness(20, 5, 20, 5),
            MinWidth = 100,
            Tag = index
        };

        newButton.Click += (s, e) => {
            if (s is Button b && b.Tag is int i) {
                _selectedIndex = i;
                DialogResult = true;
            }
        };

        char prefix = text[0];
        text = "*!~".Contains(prefix)
             ? text.Substring(1)
             : text;

        if (prefix == '*' && !_haveDefault && !_haveCancel) {
            _haveDefault = _haveCancel = true;
            newButton.IsDefault = newButton.IsCancel = true;
            newButton.Appearance = ControlAppearance.Primary;
        }
        else if (prefix == '!' && !_haveDefault) {
            _haveDefault = true;
            newButton.IsDefault = true;
            newButton.Appearance = ControlAppearance.Primary;
        }
        else if (prefix == '~' && !_haveCancel) {
            _haveCancel = true;
            newButton.IsCancel = true;
            newButton.Appearance = ControlAppearance.Secondary;
        }
        else {
            newButton.Appearance = ControlAppearance.Transparent;
        }

        newButton.Content = text;
        return newButton;
    }
}
