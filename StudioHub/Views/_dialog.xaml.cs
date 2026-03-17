using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace StudioHub.Views;

/// <summary>
/// Definisce il tipo visivo del dialogo, determinando l'icona e il colore del badge.
/// </summary>
public enum DialogType {
    /// <summary>Nessun tipo specificato; il badge viene nascosto.</summary>
    None,
    /// <summary>Dialogo di domanda, con icona a punto interrogativo.</summary>
    Question,
    /// <summary>Dialogo informativo, con icona a cerchio informativo.</summary>
    Info,
    /// <summary>Dialogo di avviso, con icona a triangolo di attenzione.</summary>
    Warning,
    /// <summary>Dialogo di errore, con icona a cerchio di errore.</summary>
    Error,
}

/// <summary>
/// Finestra di dialogo modale personalizzata con supporto per titolo, tipo, messaggio e pulsanti configurabili tramite
/// prefissi di controllo.
/// </summary>
public partial class Dialog {

    /// <summary>
    /// Indice del pulsante premuto dall'utente nell'ultima istanza del dialogo. Il valore <c> -1</c> indica che nessun
    /// pulsante è stato premuto.
    /// </summary>
    private static int _pressedButtonIndex;

    /// <summary>
    /// Nome dell'assembly corrente, usato come titolo di fallback del dialogo.
    /// </summary>
    private static readonly string ASSEMBLY_NAME = typeof(Dialog).Assembly.GetName().Name ?? string.Empty;

    /// <summary>
    /// Inizializza una nuova istanza di <see cref="Dialog"/> con titolo, tipo, messaggio e pulsanti specificati.
    /// Configura il badge visivo in base al tipo e calcola automaticamente il layout.
    /// </summary>
    /// <param name="title">Titolo visualizzato nella barra del dialogo.</param>
    /// <param name="type">Tipo del dialogo; determina l'icona e il colore del badge.</param>
    /// <param name="message">Testo del messaggio principale da visualizzare.</param>
    /// <param name="buttons">
    /// Array di etichette per i pulsanti. Supporta i prefissi di controllo: <c> '*'</c> per default + cancel, <c>
    /// '!'</c> per default, <c> '~'</c> per cancel.
    /// </param>
    public Dialog(string title, DialogType type, string message, string[] buttons) {

        InitializeComponent();
        MouseLeftButtonDown += (s, e) => {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).Y < 40) {
                DragMove();
            }
        };

        Caption.Text = title;
        switch (type) {
            case DialogType.Error:
                Badge.Symbol = SymbolRegular.ErrorCircle24;
                Badge.Foreground = Brushes.Crimson;
                break;
            case DialogType.Warning:
                Badge.Symbol = SymbolRegular.Warning24;
                Badge.Foreground = Brushes.Orange;
                break;
            case DialogType.Question:
                Badge.Symbol = SymbolRegular.QuestionCircle24;
                Badge.Foreground = Brushes.MediumSlateBlue;
                break;
            case DialogType.Info:
                Badge.Symbol = SymbolRegular.Info24;
                Badge.Foreground = Brushes.DeepSkyBlue;
                break;
            default:
                Badge.Visibility = Visibility.Collapsed;
                break;
        }
        Message.Text = message;
        injectButtons(buttons);
        calculateAndSetLayout(type);

    }

    /// <summary>
    /// Mostra un dialogo senza tipo e con un solo pulsante "Chiudi".
    /// </summary>
    /// <param name="message">Testo del messaggio da visualizzare.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    public static int Show(string message) {
        return show(null, DialogType.None, message, ["*Chiudi"]);
    }

    /// <summary>
    /// Mostra un dialogo del tipo specificato con un solo pulsante "Chiudi".
    /// </summary>
    /// <param name="type">Tipo del dialogo.</param>
    /// <param name="message">Testo del messaggio da visualizzare.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    public static int Show(DialogType type, string message) {
        return show(null, type, message, ["*Chiudi"]);
    }

    /// <summary>
    /// Mostra un dialogo senza tipo con pulsanti personalizzati.
    /// </summary>
    /// <param name="message">Testo del messaggio da visualizzare.</param>
    /// <param name="buttons">Etichette dei pulsanti con prefissi di controllo opzionali.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    public static int Show(string message, string[] buttons) {
        return show(null, DialogType.None, message, buttons);
    }

    /// <summary>
    /// Mostra un dialogo del tipo specificato con pulsanti personalizzati.
    /// </summary>
    /// <param name="type">Tipo del dialogo.</param>
    /// <param name="message">Testo del messaggio da visualizzare.</param>
    /// <param name="buttons">Etichette dei pulsanti con prefissi di controllo opzionali.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    public static int Show(DialogType type, string message, string[] buttons) {
        return show(null, type, message, buttons);
    }

    /// <summary>
    /// Mostra un dialogo del tipo specificato con titolo personalizzato e un solo pulsante "Chiudi".
    /// </summary>
    /// <param name="type">Tipo del dialogo.</param>
    /// <param name="title">Titolo della finestra di dialogo.</param>
    /// <param name="message">Testo del messaggio da visualizzare.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    public static int Show(DialogType type, string title, string message) {
        return show(title, type, message, ["*Chiudi"]);
    }

    /// <summary>
    /// Mostra un dialogo del tipo specificato con titolo e pulsanti personalizzati.
    /// </summary>
    /// <param name="type">Tipo del dialogo.</param>
    /// <param name="title">Titolo della finestra di dialogo.</param>
    /// <param name="message">Testo del messaggio da visualizzare.</param>
    /// <param name="buttons">Etichette dei pulsanti con prefissi di controllo opzionali.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    public static int Show(DialogType type, string title, string message, string[] buttons) {
        return show(title, type, message, buttons);
    }

    /// <summary>
    /// Metodo interno che crea e mostra il dialogo in modo modale, rilevando automaticamente la finestra proprietaria
    /// attiva. Il titolo viene ricavato dal parametro, dalla finestra attiva o dal nome dell'assembly, in ordine di
    /// priorità.
    /// </summary>
    /// <param name="title">Titolo esplicito, oppure <see langword="null"/> per il rilevamento automatico.</param>
    /// <param name="type">Tipo del dialogo.</param>
    /// <param name="message">Testo del messaggio; non può essere <see langword="null"/> o spazio vuoto.</param>
    /// <param name="buttons">Etichette dei pulsanti con prefissi di controllo opzionali.</param>
    /// <returns>L'indice del pulsante premuto, oppure <c>-1</c> se il dialogo è stato chiuso senza selezione.</returns>
    /// <exception cref="ArgumentNullException">
    /// Generata se <paramref name="message"/> è <see langword="null"/> o composto solo da spazi bianchi.
    /// </exception>
    private static int show(string? title, DialogType type, string message, string[] buttons) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(message);

        WindowCollection windows = Application.Current.Windows;
        Window? owner = null;
        for (int i = 0; i < windows.Count; i++) {
            if (windows[i].IsActive) {
                owner = windows[i];
                break;
            }
        }

        Dialog dialog = new(title ?? owner?.Title ?? ASSEMBLY_NAME, type, message, buttons) {
            Owner = owner
        };

        _pressedButtonIndex = -1;
        dialog.ShowDialog();
        return _pressedButtonIndex;
    }

    /// <summary>
    /// Crea e aggiunge dinamicamente i pulsanti al pannello <c> Buttons</c>, interpretando i prefissi di controllo per
    /// assegnare i ruoli di default e cancel. <list type="bullet"> <item> <description> <c> '*'</c>: pulsante default
    /// <em> e</em> cancel (Invio/Esc).</description></item> <item> <description> <c> '!'</c>: pulsante default
    /// (Invio).</description></item> <item> <description> <c> '~'</c>: pulsante cancel (Esc).</description></item>
    /// </list> Le stringhe vuote o composte solo dal carattere di controllo vengono ignorate.
    /// </summary>
    /// <param name="items">Array di etichette raw con prefissi di controllo opzionali.</param>
    private void injectButtons(string[] items) {

        int defaultIndex = -1;
        int cancelIndex = -1;

        // Filtra le stringhe vuote e quelle composte solo da un carattere di controllo
        List<string> filtered = [.. items.Where(s => !string.IsNullOrEmpty(s) && !(s.Length == 1 && "~!*".Contains(s[0])))];

        // Cerca il prefisso '*'
        int starIdx = filtered.FindIndex(s => s.StartsWith('*'));
        if (starIdx != -1) {
            defaultIndex = cancelIndex = starIdx;
        }
        else {
            // Cerca il prefisso '!'
            int bangIdx = filtered.FindIndex(s => s.StartsWith('!'));
            if (bangIdx != -1) {
                defaultIndex = bangIdx;
            }
            // Cerca il prefisso '~'
            int tildeIdx = filtered.FindIndex(s => s.StartsWith('~'));
            if (tildeIdx != -1) {
                cancelIndex = tildeIdx;
            }
        }

        // Crea un Button per ogni stringa filtrata
        for (int i = 0; i < filtered.Count; i++) {

            string original = filtered[i];
            string content = original[0] is '~' or '!' or '*'
                   ? original[1..]
                   : original;

            Button newButton = new() {
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100,
                Padding = new Thickness(20, 5, 20, 6),
                Tag = i,
                Content = content,
            };

            if (i == defaultIndex && i == cancelIndex) {
                newButton.IsDefault = newButton.IsCancel = true;
                newButton.Appearance = ControlAppearance.Primary;
                newButton.ToolTip = "Invio/Esc";
            }
            else if (i == defaultIndex) {
                newButton.IsDefault = true;
                newButton.Appearance = ControlAppearance.Primary;
                newButton.ToolTip = "Invio";
            }
            else if (i == cancelIndex) {
                newButton.IsCancel = true;
                newButton.Appearance = ControlAppearance.Secondary;
                newButton.ToolTip = "Esc";
            }
            else {
                newButton.Appearance = ControlAppearance.Transparent;
            }

            newButton.Click += (s, a) => {
                if (s is Button button && button.Tag is int i) {
                    _pressedButtonIndex = i;
                    DialogResult = true;
                }
            };

            Buttons.Children.Add(newButton);
        }
    }

    /// <summary>
    /// Calcola e imposta le dimensioni ottimali della finestra in base al contenuto. La larghezza viene determinata dal
    /// massimo tra lo spazio richiesto dai pulsanti e la larghezza derivata da una proporzione 3:2 del testo; entrambe
    /// le dimensioni vengono poi vincolate ai valori minimi e massimi consentiti.
    /// </summary>
    /// <param name="type">
    /// Tipo del dialogo; se <see cref="DialogType.None"/>, l'altezza del badge viene esclusa dal calcolo.
    /// </param>
    private void calculateAndSetLayout(DialogType type) {

        // Calcola la larghezza minima richiesta dai pulsanti
        Buttons.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double minimumWidth = Buttons.DesiredSize.Width + 50; //margini orizzontali

        // Calcola la dimensione del testo del messaggio senza limiti di spazio
        Message.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double unlimitedTextWidth = Message.DesiredSize.Width;
        double unlimitedTextHeight = Message.DesiredSize.Height;

        // Calcola la larghezza del testo costretto a una proporzione di 3:2
        double ratioTextWidth = Math.Sqrt(unlimitedTextWidth * unlimitedTextHeight * 1.50) + 50; //proporzione, margini orizzontali
        double finalWidth = Math.Clamp(Math.Max(minimumWidth, ratioTextWidth), 270, 540); //MinWidth, MaxWidth

        // Ricalcola l'altezza del testo limitato alla larghezza finale
        Message.Measure(new Size(finalWidth - 50, double.PositiveInfinity)); //margini orizzontali
        double textHeight = Message.DesiredSize.Height;

        // Stabilisce l'altezza dell'immagine
        double badgeHeight = type == DialogType.None ? 0 : 70;
        double finalHeight = Math.Clamp(147 + badgeHeight + textHeight, 180, 360); //margini verticali + altezze fisse, MinHeight, MaxHeight

        Width = finalWidth;
        Height = finalHeight;
    }
}
