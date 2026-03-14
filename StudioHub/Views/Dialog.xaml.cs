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

    private static int _defaultIndex;
    private static int _cancelIndex;
    private static int _buttonIndex;

    /// <summary>
    /// Mostra una finestra di dialogo modale.
    /// </summary>
    /// <param name="message">Messaggio da visualizzare.</param>
    /// <param name="title">Titolo della finestra (opzionale).</param>
    /// <param name="buttons">
    /// Array di pulsanti (opzionale).<br/> Anteporre al testo del pulsante il carattere:<br/> - asterisco (<b>*</b>)
    /// per impostare il pulsante come predefinito e annullamento, visualizzato come pulsante principale, ha priorità
    /// sugli altri <br/> - punto esclamativo (<b>!</b>) per impostare il pulsante come predefinito, visualizzato come
    /// pulsante principale<br/> - tilde (<b>~</b>) per impostare il pulsante come annullamento, visualizzato come
    /// pulsante secondario
    /// </param>
    /// <param name="type">Tipo di dialog (opzionale).</param>
    /// <returns>Indice del pulsante selezionato.</returns>
    public static int Show(string message,
                           string? title = null,
                           string[]? buttons = null,
                           DialogType? type = DialogType.None) {

        if (buttons is null || buttons.Length == 0) {
            buttons = ["*Chiudi"];
        }

        if (!type.HasValue) {
            type = DialogType.None;
        }

        Window? owner = null;
        for (int n = 0; n < Application.Current.Windows.Count; n++) {
            Window window = Application.Current.Windows[n];
            if (window.IsActive) {
                owner = window;
                break;
            }
        }

        Dialog dialog = new(message, title, buttons, type) {
            Owner = owner
        };

        _buttonIndex = -1;
        dialog.ShowDialog();
        return _buttonIndex;
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
                   DialogType? type) {

        InitializeComponent();

        DialogMessage.Text = message;

        DialogTitle.Text = title ??
                (Application.Current.MainWindow?.Title) ??
                (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name) ??
                string.Empty;

        switch (type) {
            case DialogType.Error:
                DialogIcon.Symbol = SymbolRegular.ErrorCircle24;
                DialogIcon.Foreground = Brushes.Crimson;
                break;
            case DialogType.Warning:
                DialogIcon.Symbol = SymbolRegular.Warning24;
                DialogIcon.Foreground = Brushes.Orange;
                break;
            case DialogType.Question:
                DialogIcon.Symbol = SymbolRegular.QuestionCircle24;
                DialogIcon.Foreground = Brushes.MediumSlateBlue;
                break;
            case DialogType.Info:
                DialogIcon.Symbol = SymbolRegular.Info24;
                DialogIcon.Foreground = Brushes.DeepSkyBlue;
                break;
            default:
                DialogIcon.Symbol = SymbolRegular.Empty;
                DialogIcon.Foreground = Brushes.Transparent;
                break;
        }

        string[] listButtons = [.. buttons.Where(s => !string.IsNullOrWhiteSpace(s) && (s.Length > 1 || !"*~!".Contains(s[0])))];
        setCancelDefault(listButtons);
        for (int i = 0; i < listButtons.Length; i++) {
            DialogButtons.Children.Add(createButton(listButtons[i], i));
        }

        Loaded += (s, e) => SizeToContent = SizeToContent.WidthAndHeight;
        MouseLeftButtonDown += (s, e) => {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        };
    }

    /// <summary>
    /// Restituisce l'indice del pulsante che inizia con '*' (predefinito e annullamento).
    /// </summary>
    /// <param name="buttons">Array di pulsanti.</param>
    /// <returns>Indice del pulsante predefinito, oppure <see langword="-1"/> se non trovato.</returns>
    private static void setCancelDefault(string[] buttons) {
        ArgumentNullException.ThrowIfNull(buttons);

        _defaultIndex = _cancelIndex = -1;

        for (int i = 0; i < buttons.Length; i++) {
            char prefix = buttons[i][0];
            if (prefix == '*') {
                _defaultIndex = i;
                _cancelIndex = i;
                break;
            }
            if (prefix == '!' && _defaultIndex == -1) {
                _defaultIndex = i;
            }
            if (prefix == '~' && _cancelIndex == -1) {
                _cancelIndex = i;
            }
        }
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
                _buttonIndex = i;
                DialogResult = true;
            }
        };

        switch (text[0]) {
            case '*':
                if (_defaultIndex == index && _cancelIndex == index) {
                    newButton.IsDefault = newButton.IsCancel = true;
                    newButton.Appearance = ControlAppearance.Primary;
                }
                else {
                    newButton.Appearance = ControlAppearance.Transparent;
                }
                newButton.Content = text.Substring(1);
                break;
            case '!':
                if (_defaultIndex == index) {
                    newButton.IsDefault = true;
                    newButton.Appearance = ControlAppearance.Primary;
                }
                else {
                    newButton.Appearance = ControlAppearance.Transparent;
                }
                newButton.Content = text.Substring(1);
                break;
            case '~':
                if (_cancelIndex == index) {
                    newButton.IsCancel = true;
                    newButton.Appearance = ControlAppearance.Secondary;
                }
                else {
                    newButton.Appearance = ControlAppearance.Transparent;
                }
                newButton.Content = text.Substring(1);
                break;
            default:
                newButton.Appearance = ControlAppearance.Transparent;
                newButton.Content = text;
                break;
        }
        return newButton;
    }
}
