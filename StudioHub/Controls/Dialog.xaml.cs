using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudioHub.Controls;
/// <summary>
/// Definisce il simbolo visualizzato dalla finestra di dialogo.
/// </summary>
public enum DialogIcon {
    /// <summary>
    /// Non viene visualizzato nessun simbolo.
    /// </summary>
    None,
    /// <summary>
    /// Viene visualizzato un punto esclamativo rosso.
    /// </summary>
    Error,
    /// <summary>
    /// Viene visualizzata un punto esclamativo giallo.
    /// </summary>
    Warning,
    /// <summary>
    /// Viene visualizzata la lettera i.
    /// </summary>
    Info,
    /// <summary>
    /// Viene visualizzato un punto interrogativo.
    /// </summary>
    Question

}

/// <inheritdoc/>
public partial class Dialog : Window {

    private static readonly (SolidColorBrush? Color, string Data)[] icons = [

        // None
        (null, string.Empty),
        
        // Error
        (new SolidColorBrush(Color.FromRgb(231, 76, 60)),
        "M320 576C178.6 576 64 461.4 64 320 64 178.6 178.6 64 320 64S576 178.6 576 320 461.4 576 320 576zm0-192c-17.7 0-32 14.3-32 32s14.3 32 32 32 32-14.3 32-32-14.3-32-32-32zm0-192c-18.2 0-32.7 15.5-31.4 33.7l7.4 104c.9 12.6 11.4 22.3 23.9 22.3 12.6 0 23-9.7 23.9-22.3l7.4-104c1.3-18.2-13.1-33.7-31.4-33.7z"),
        
        // Warning
        (new SolidColorBrush(Color.FromRgb(241, 196, 15)),
        "M320 64c14.7 0 28.2 8.1 35.2 21l216 400c6.7 12.4 6.4 27.4-.8 39.5-7.2 12.1-20.3 19.5-34.4 19.5H104c-14.1 0-27.2-7.4-34.4-19.5-7.2-12.1-7.5-27.1-.8-39.5l216-400c7-12.9 20.5-21 35.2-21zm0 352c-17.7 0-32 14.3-32 32s14.3 32 32 32 32-14.3 32-32-14.3-32-32-32zm0-192c-18.2 0-32.7 15.5-31.4 33.7l7.4 104c.9 12.5 11.4 22.3 23.9 22.3 12.6 0 23-9.7 23.9-22.3l7.4-104c1.3-18.2-13.1-33.7-31.4-33.7z"),
        
        // Info
        (new SolidColorBrush(Color.FromRgb(52, 152, 219)),
        "M280 288c-13.3 0-24 10.7-24 24s10.7 24 24 24h24v64H280c-13.3 0-24 10.7-24 24s10.7 24 24 24h80c13.3 0 24-10.7 24-24s-10.7-24-24-24h-8V312c0-13.3-10.7-24-24-24H280Zm8-64c0 17.7 14.3 32 32 32s32-14.3 32-32-14.3-32-32-32-32 14.3-32 32Zm32 352C178.6 576 64 461.4 64 320 64 178.6 178.6 64 320 64S576 178.6 576 320 461.4 576 320 576Z"),
        
        // Question
        (new SolidColorBrush(Color.FromRgb(155, 89, 182)),
        "M288 432c0 17.7 14.3 32 32 32s32-14.3 32-32-14.3-32-32-32-32 14.3-32 32Zm32-191.9c17.7 0 32 14.3 32 32 0 9.6-3.4 15.4-7.7 19.6-5 4.8-11.8 8.2-18.2 10.3-15.3 5-30.1 19.7-30.1 40.2v8.1c0 13.3 10.7 24 24 24s24-10.7 24-24v-3.8c20-7.3 56-27.3 56-74.5 0-44.2-35.8-80-80-80s-80 35.8-80 80c0 13.3 10.7 24 24 24s24-10.7 24-24c0-17.7 14.3-32 32-32ZM320 576C178.6 576 64 461.4 64 320 64 178.6 178.6 64 320 64S576 178.6 576 320 461.4 576 320 576Z")
    ];

    /// <summary>
    /// Contiene l'indice del pulsante premuto.
    /// </summary>
    private int ResultIndex = 0;
    private readonly string messageString;

    /// <summary>
    /// Mostra un dialogo personalizzato in modo modale.
    /// </summary>
    /// <param name="message">Testo del messaggio.</param>
    /// <param name="icon">Icona da visualizzare.</param>
    /// <param name="title">Titolo della finestra.</param>
    /// <param name="buttons">
    /// Array di stringhe contenente i testi dei pulsanti, se nullo o vuoto verrà visualizzato un pulsante
    /// "Chiudi".<br/> Inserire un carattere nullo (<c>\0</c>) all'inizio o alla fine della stringa per definire
    /// rispettivamente il pulsante predefinito (Invio) o il pulsante annulla (Esc)<br/> Solo le prime occorrenze
    /// saranno considerate, tutte le successive verranno ignorate.
    /// </param>
    /// <returns>L'indice <see cref="int"/> in base zero del pulsante premuto dall'utente.</returns>
    public static int Show(string message, DialogIcon icon = DialogIcon.None, string title = "", params string[] buttons) {

        Window? activeWindow = GetActiveWindow();

        if (string.IsNullOrWhiteSpace(title)) {
            title = activeWindow is null || string.IsNullOrWhiteSpace(activeWindow.Title)
                ? Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty
                : activeWindow.Title;
        }

        if (buttons == null || buttons.Length == 0) {
            buttons = ["\0Chiudi\0"];
        }

        Dialog dialog = new(title, message, icon, buttons) {
            Owner = activeWindow
        };

        // Disabilita l'icona della finestra all'inizializzazione della sorgente.
        dialog.SourceInitialized += (s, e) => DisableWindowIcon(dialog);
        dialog.Owner?.Opacity = .5;

        dialog.ShowDialog();

        dialog.Owner?.Opacity = 1;

        return dialog.ResultIndex;
    }

    /// <inheritdoc cref="Show(string, DialogIcon, string, string[])"/>
    private Dialog(string title, string message, DialogIcon icon, string[] buttons) {

        InitializeComponent();
        Loaded += dialog_Loaded;

        Title = title;
        SizeToContent = SizeToContent.Width;
        setIcon((int)icon);
        setButtons(buttons);
        messageString = message;
    }

    private void dialog_Loaded(object sender, RoutedEventArgs e) {
        double horizontalMargin = textblockMessage.Margin.Left + textblockMessage.Margin.Right;
        textblockMessage.MaxWidth = ActualWidth - horizontalMargin;
        SizeToContent = SizeToContent.Height;
        textblockMessage.Text = messageString;
    }

    private void button_Click(object sender, RoutedEventArgs e) {
        ResultIndex = (int)((Button)sender).Tag;
        DialogResult = true;
    }

    private void setIcon(int iconIndex) {
        pathIcon.Fill = icons[iconIndex].Color;
        pathIcon.Data = Geometry.Parse(icons[iconIndex].Data);
        pathIcon.Visibility = iconIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void setButtons(string[] buttonsText) {

        bool isDefault = false;
        bool isCancel = false;
        int count = buttonsText.Length;
        int lastIndex = count - 1;

        for (int i = 0; i < count; i++) {

            string text = buttonsText[i];
            if (string.IsNullOrWhiteSpace(text)) {
                continue;
            }

            Button button = new() {
                Margin = new Thickness(0, 0, i < lastIndex ? 16 : 0, 0),
                Padding = new Thickness(24, 5, 24, 6),
                Tag = i
            };

            if (!isCancel && text.Length > 0 && text[^1] == '\0') {
                isCancel = true;
                button.IsCancel = true;
            }

            if (!isDefault && text.Length > 0 && text[0] == '\0') {
                isDefault = true;
                button.IsDefault = true;
            }

            button.Content = text.Trim('\0');
            button.Click += button_Click;

            panelButtons.Children.Add(button);
        }
    }
}