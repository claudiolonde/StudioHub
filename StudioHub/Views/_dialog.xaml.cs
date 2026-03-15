using System.Diagnostics;
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
                           DialogType? type = DialogType.None,
                           string[]? buttons = null,
                           string? title = null) {

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

        Dialog dialog = new(message, type, buttons, title) {
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
                   DialogType? type,
                   string[] buttons,
                   string? title
                   ) {

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
                DialogIcon.Visibility = Visibility.Collapsed;
                break;
        }

        string[] validButtons = [.. buttons.Where(s => !string.IsNullOrWhiteSpace(s) && (s.Length > 1 || !"*~!".Contains(s[0])))];
        injectButtons(validButtons);
        //calculateAndApplyOptimalSize();

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
                        newButton.ToolTip = "Invio/Esc";
                    }
                    break;
                case '!':
                    if (defaultIndex == n) {
                        newButton.IsDefault = true;
                        newButton.Appearance = ControlAppearance.Primary;
                        newButton.ToolTip = "Invio";
                    }
                    break;
                case '~':
                    if (cancelIndex == n) {
                        newButton.IsCancel = true;
                        newButton.Appearance = ControlAppearance.Secondary;
                        newButton.ToolTip = "Esc";
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

    private void calculateAndApplyOptimalSize() {
        // 1. Definizioni delle costanti lette dal XAML
        const double MAX_W = 600;
        const double MAX_H = 600;
        const double MIN_W = 400;
        const double HORIZONTAL_MARGINS_BUTTONS = 50; // 25 sx + 25 dx
        const double HORIZONTAL_MARGINS_TEXT = 100;   // 50 grid + 50 scrollviewer
        const double VERTICAL_MARGINS_TOTAL = 115;    // 15+25(Grid) + 25+25(Content) + 25(Buttons)

        // Target Ratio: 3/2 (1.5) o 4/3 (1.33)
        const double TARGET_RATIO = 1.5;

        // 2. Misuriamo i controlli base (Titolo, Icona, Pulsanti) con spazio infinito
        DialogTitle.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        DialogButtons.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        double iconHeight = 0;
        if (DialogIcon.Visibility == Visibility.Visible) {
            DialogIcon.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            iconHeight = DialogIcon.DesiredSize.Height + 25; // 25 è il Margin.Bottom
        }

        double fixedElementsHeight = DialogTitle.DesiredSize.Height + iconHeight + DialogButtons.DesiredSize.Height;
        double buttonsTotalWidth = DialogButtons.DesiredSize.Width;

        // 3. Misuriamo il testo su una singola riga (senza wrapping)
        DialogMessage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double rawTextWidth = DialogMessage.DesiredSize.Width;

        double finalWidth = 0;
        double finalHeight = 0;

        // 4. Logica: Testo corto vs Testo lungo
        if (rawTextWidth <= buttonsTotalWidth) {
            // Il testo è più corto dei pulsanti, mantieni la larghezza dei pulsanti (rispettando MinWidth)
            finalWidth = Math.Max(MIN_W, buttonsTotalWidth + HORIZONTAL_MARGINS_BUTTONS);

            // Calcoliamo l'altezza con questa larghezza
            DialogMessage.Measure(new Size(finalWidth - HORIZONTAL_MARGINS_TEXT, double.PositiveInfinity));
            finalHeight = VERTICAL_MARGINS_TOTAL + fixedElementsHeight + DialogMessage.DesiredSize.Height;
        }
        else {
            // Testo lungo: Ricerca della larghezza ottimale per mantenere la proporzione
            double minSearchW = Math.Max(MIN_W, buttonsTotalWidth + HORIZONTAL_MARGINS_BUTTONS);
            double maxSearchW = MAX_W;

            double bestWidth = minSearchW;
            double bestRatioDiff = double.MaxValue;

            // Iteriamo a step di 10 pixel per trovare la larghezza ideale
            for (double w = minSearchW; w <= maxSearchW; w += 10) {
                // Misuriamo l'altezza del testo se costretto in questa larghezza 'w'
                double textWrapWidth = Math.Max(0, w - HORIZONTAL_MARGINS_TEXT);
                DialogMessage.Measure(new Size(textWrapWidth, double.PositiveInfinity));

                double estimatedHeight = VERTICAL_MARGINS_TOTAL + fixedElementsHeight + DialogMessage.DesiredSize.Height;

                // Calcoliamo il ratio attuale
                double currentRatio = w / estimatedHeight;
                double ratioDiff = Math.Abs(currentRatio - TARGET_RATIO);

                // Se ci stiamo avvicinando al rapporto 3:2, salviamo questo valore come migliore
                if (ratioDiff < bestRatioDiff) {
                    bestRatioDiff = ratioDiff;
                    bestWidth = w;
                }
            }

            finalWidth = bestWidth;

            // Calcoliamo l'altezza reale finale usando la larghezza trovata
            DialogMessage.Measure(new Size(finalWidth - HORIZONTAL_MARGINS_TEXT, double.PositiveInfinity));
            finalHeight = VERTICAL_MARGINS_TOTAL + fixedElementsHeight + DialogMessage.DesiredSize.Height;
        }

        // 5. Applichiamo i limiti di sicurezza massimi (MaxHeight / MaxWidth del XAML)
        Width = Math.Min(MAX_W, finalWidth);
        Height = Math.Min(MAX_H, finalHeight);
    }
}
