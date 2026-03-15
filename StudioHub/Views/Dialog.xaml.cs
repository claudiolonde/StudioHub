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
public partial class Dialog : FluentWindow {

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

        string[] validButtons = [.. buttons.Where(s => !string.IsNullOrWhiteSpace(s) && (s.Length > 1 || !"*~!".Contains(s[0])))];
        injectButtons(validButtons);

        Loaded += (s, e) => {
            SizeToContent = SizeToContent.WidthAndHeight;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;
            Left = (screenWidth - windowWidth) / 2;
            Top = (screenHeight - windowHeight) / 2;
        };

        MouseLeftButtonDown += (s, e) => {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).Y < 48) {
                DragMove();
            }
        };

    }

    /// <summary>
    /// Crea i pulsanti per il dialog, impostando proprietà e comportamento in base al prefisso del testo.
    /// </summary>
    /// <param name="buttonNames">Array di nomi dei pulsanti, ciascuno può includere un prefisso speciale.</param>
    private void injectButtons(string[] buttonNames) {

        int defaultIndex = -1;
        int cancelIndex = -1;

        // Imposta gli indici dei pulsanti predefinito e di annullamento in base ai prefissi dei testi dei pulsanti
        for (int n = 0; n < buttonNames.Length; n++) {
            char prefix = buttonNames[n][0];
            if (prefix == '*') { defaultIndex = cancelIndex = n; break; }
            if (prefix == '!' && defaultIndex == -1) { defaultIndex = n; }
            if (prefix == '~' && cancelIndex == -1) { cancelIndex = n; }
        }

        // Crea e configura ciascun pulsante
        for (int n = 0; n < buttonNames.Length; n++) {

            string content = buttonNames[n];
            char controlChar = content[0];

            Button newButton = new() {
                Appearance = ControlAppearance.Transparent,
                Content = "*!~".Contains(controlChar) ? content.Substring(1) : content,
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100,
                Padding = new Thickness(20, 5, 20, 5),
                Tag = n
            };

            switch (controlChar) {
                case '*':
                    if (defaultIndex == n && cancelIndex == n) {
                        newButton.IsDefault = true;
                        newButton.IsCancel = true;
                        newButton.Appearance = ControlAppearance.Primary;
                        newButton.ToolTip = "Pulsante predefinito e di annullamento";
                    }
                    break;
                case '!':
                    if (defaultIndex == n) {
                        newButton.IsDefault = true;
                        newButton.Appearance = ControlAppearance.Primary;
                        newButton.ToolTip = "Pulsante predefinito";
                    }
                    break;
                case '~':
                    if (cancelIndex == n) {
                        newButton.IsCancel = true;
                        newButton.Appearance = ControlAppearance.Secondary;
                        newButton.ToolTip = "Pulsante di annullamento";
                    }
                    break;
                default:
                    break;
            }

            newButton.Click += (s, a) => {
                if (s is Button button && button.Tag is int i) {
                    _buttonIndex = i;
                    DialogResult = true;
                }
            };

            DialogButtons.Children.Add(newButton);
        }
    }
}
