using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace StudioHub.Views;

public enum DialogType {
    None,
    Question,
    Info,
    Warning,
    Error,
}

public partial class Dialog {

    private static int _pressedButtonIndex;
    private static readonly string ASSEMBLY_NAME = typeof(Dialog).Assembly.GetName().Name ?? string.Empty;

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
        Loaded += dialog_Loaded;
    }

    private void dialog_Loaded(object sender, RoutedEventArgs e) {
        //Caption.Text = $"width={Width} - height={Height}";
    }

    public static int Show(string message) {
        return Show(null, DialogType.None, message, null);
    }

    public static int Show(DialogType type, string message) {
        return Show(null, type, message, null);
    }

    public static int Show(string? title, DialogType type, string message, string[]? buttons) {

        ArgumentNullException.ThrowIfNullOrWhiteSpace(message);

        WindowCollection windows = Application.Current.Windows;
        Window? owner = null;
        for (int i = 0; i < windows.Count; i++) {
            if (windows[i].IsActive) {
                owner = windows[i];
                break;
            }
        }

        string caption = title ?? owner?.Title ?? ASSEMBLY_NAME;

        if (buttons is null || buttons.Length == 0) {
            buttons = ["*Chiudi"];
        }

        Dialog dialog = new(caption, type, message, buttons) {
            Owner = owner
        };

        _pressedButtonIndex = -1;
        dialog.ShowDialog();
        return _pressedButtonIndex;

    }

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

    private void calculateAndSetLayout(DialogType type) {

        const double MIN_WIDTH = 270, MAX_WIDTH = 540;
        const double MIN_HEIGHT = 180, MAX_HEIGHT = 360;
        const double H_MARGINS = 50;   // 25 + 25
        const double V_MARGINS = 35;   // 10 top + 25 bottom (Grid margin)

        // Altezze fisse delle righe del Grid
        const double ROW_CAPTION = 30;  // RowDefinition Height="30"
        const double ROW_BUTTONS = 32;  // RowDefinition Height="32"
        const double MSG_MARGIN = 50;  // Margin="0 25" → 25 top + 25 bottom

        // -------------------------------------------------------------------------
        // 1. MEASURE BUTTONS
        // -------------------------------------------------------------------------
        Buttons.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double absoluteMinimumWidth = Buttons.DesiredSize.Width + H_MARGINS;

        // -------------------------------------------------------------------------
        // 2. MEASURE BADGE (RowDefinition Height="Auto")
        // -------------------------------------------------------------------------
        Badge.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double badgeHeight = type == DialogType.None ? 0 : Badge.DesiredSize.Height;

        // -------------------------------------------------------------------------
        // 3. MEASURE MESSAGE (unconstrained — testo su riga singola)
        // -------------------------------------------------------------------------
        Message.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double textNaturalWidth = Message.DesiredSize.Width;
        double textSingleLineHeight = Message.DesiredSize.Height;

        // -------------------------------------------------------------------------
        // 4. IDEAL WIDTH via aspect-ratio trick
        // -------------------------------------------------------------------------
        double textArea = textNaturalWidth * textSingleLineHeight;
        double idealWidth = Math.Sqrt(textArea * 1.50) + H_MARGINS;
        double targetWidth = Math.Clamp(
            Math.Max(absoluteMinimumWidth, idealWidth),
            MIN_WIDTH, MAX_WIDTH);

        Width = targetWidth;

        // -------------------------------------------------------------------------
        // 5. MEASURE MESSAGE (constrained) — ora WPF calcola i veri a-capo
        // -------------------------------------------------------------------------
        double availableTextWidth = targetWidth - H_MARGINS;
        Message.Measure(new Size(availableTextWidth, double.PositiveInfinity));
        double textWrappedHeight = Message.DesiredSize.Height;

        // -------------------------------------------------------------------------
        // 6. TARGET HEIGHT — somma di tutte le righe reali del layout
        //    V_MARGINS + ROW_CAPTION + badgeHeight + MSG_MARGIN + textWrapped + ROW_BUTTONS
        // -------------------------------------------------------------------------
        double targetHeight = Math.Clamp(
            V_MARGINS + ROW_CAPTION + badgeHeight + MSG_MARGIN + textWrappedHeight + ROW_BUTTONS,
            MIN_HEIGHT, MAX_HEIGHT);

        Height = targetHeight;
    }
}
