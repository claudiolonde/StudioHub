using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per Dialog.xaml
/// </summary>
public partial class Dialog : FluentWindow {

    private int _selectedIndex = -1;
    
    public static int Show(string message, string? title = null, string[]? buttons = null, MessageType type = MessageType.None) {

        buttons ??= ["\\dChiudi"];
        Dialog dialog = new(message, title, buttons, type) {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)
        };

        dialog.ShowDialog();
        return dialog._selectedIndex;
    }

    private Dialog(string message, string? title, string[] buttons, MessageType type) {

        InitializeComponent();

        (SymbolRegular symbol, Brush? brush) = getResources(type);

        DataContext = new {
            Title = title ??
                    (Application.Current.MainWindow?.Title) ??
                    (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name) ??
                    "Avviso",
            Message = message,
            Buttons = buttons.Select(createButton).ToArray(),
            Symbol = symbol,
            SymbolBrush = brush
        };

        Loaded += (s, e) => SizeToContent = SizeToContent.WidthAndHeight;
        MouseLeftButtonDown += (s, e) => {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        };
    }

    private static (SymbolRegular, Brush) getResources(MessageType type) {
        return type switch {
            MessageType.Error => (SymbolRegular.ErrorCircle24, Brushes.Crimson),
            MessageType.Warning => (SymbolRegular.Warning24, Brushes.Orange),
            MessageType.Question => (SymbolRegular.QuestionCircle24, Brushes.DeepSkyBlue),
            MessageType.Info => (SymbolRegular.Info24, Brushes.DeepSkyBlue),
            _ => (SymbolRegular.Empty, Brushes.Transparent)
        };
    }

    private Button createButton(string text, int index) {
        Button b = new() {
            Margin = new Thickness(10, 0, 10, 0),
            Padding = new Thickness(20, 5, 20, 5),
            MinWidth = 100,
            Tag = index
        };

        if (text.StartsWith("\\d")) // Default
           {
            b.Content = text.Substring(2);
            b.Appearance = ControlAppearance.Primary;
            b.IsDefault = true;
        }
        else if (text.StartsWith("\\c")) // Cancel
        {
            b.Content = text.Substring(2);
            b.Appearance = ControlAppearance.Secondary;
            b.IsCancel = true;
        }
        else {
            b.Content = text;
            b.Appearance = ControlAppearance.Transparent;
        }

        b.Click += (s, e) => {
            if (s is Button b && b.Tag is int i) {
                _selectedIndex = i;
                DialogResult = true;
            }
        };
        return b;
    }
}

public enum MessageType {
    Error, Warning, Question, Info, None
}
