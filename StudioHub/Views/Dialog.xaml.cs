using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per Dialog.xaml
/// </summary>
public partial class Dialog : FluentWindow {

    public int SelectedIndex { get; private set; } = -1;

    private Dialog(string title, string message, string[] buttons, MessageType type) {
        InitializeComponent();

        // Risoluzione Titolo
        string finalTitle = title ??
                           (Application.Current.MainWindow?.Title) ??
                           (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name) ??
                           "Messaggio";

        var btnList = buttons.Select((s, i) => new DialogButton(s, i)).ToList();
        var (symbol, brush) = GetResources(type);

        DataContext = new {
            WindowTitle = finalTitle,
            Message = message,
            Buttons = btnList,
            Symbol = symbol,
            SymbolBrush = brush
        };
        this.Loaded += (s, e) => ForceLayoutUpdate();
    }
    private void ForceLayoutUpdate()
    {
        // Forza il calcolo delle dimensioni del contenuto
        InvalidateMeasure();
        UpdateLayout();

    }
    public static int Show(string message, string title = null, string[] buttons = null, MessageType type = MessageType.Info) {
        buttons ??= ["OK"];
        Dialog dialog = new (title, message, buttons, type) {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)
        };

        dialog.ShowDialog();
        return dialog.SelectedIndex;
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        if (sender is FrameworkElement fe && fe.DataContext is DialogButton db) {
            SelectedIndex = db.Index;
            DialogResult = true;
        }
    }

    private static (SymbolRegular, Brush) GetResources(MessageType type) => type switch {
        MessageType.Error => (SymbolRegular.ErrorCircle24, Brushes.Crimson),
        MessageType.Warning => (SymbolRegular.Warning24, Brushes.Orange),
        MessageType.Question => (SymbolRegular.QuestionCircle24, Brushes.DeepSkyBlue),
        MessageType.None => (SymbolRegular.BorderNone16, Brushes.Transparent),
        _ => (SymbolRegular.Info24, Brushes.DeepSkyBlue)
    };
}
public enum MessageType { Error, Warning, Question, Info, None }

public class DialogButton
{
    public string Text { get; }
    public int Index { get; }
    public ControlAppearance Appearance { get; }
    public bool IsCancel { get; }
    public bool IsDefault { get; }

    public DialogButton(string raw, int index)
    {
        Index = index;
        if (raw.StartsWith(@"\d")) // Default / Primary
        {
            Text = raw.Substring(2);
            Appearance = ControlAppearance.Primary;
            IsDefault = true;
        }
        else if (raw.StartsWith(@"\c")) // Cancel / Danger
        {
            Text = raw.Substring(2);
            Appearance = ControlAppearance.Danger;
            IsCancel = true;
        }
        else
        {
            Text = raw;
            Appearance = ControlAppearance.Secondary;
        }
    }
}
