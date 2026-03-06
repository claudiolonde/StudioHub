using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudioHub.Controls;

public class PasswordTextBox : TextBox {
    private bool _isUpdatingText;
    private const char PasswordChar = '●';

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(PasswordTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordOrRevealChanged));

    public string Password {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly DependencyProperty IsRevealedProperty =
        DependencyProperty.Register(
            nameof(IsRevealed),
            typeof(bool),
            typeof(PasswordTextBox),
            new PropertyMetadata(false, OnPasswordOrRevealChanged));

    public bool IsRevealed {
        get => (bool)GetValue(IsRevealedProperty);
        set => SetValue(IsRevealedProperty, value);
    }

    public PasswordTextBox() {
        // Protezioni di sicurezza e sincronizzazione
        IsUndoEnabled = false;
        AllowDrop = false;

        // Intercettiamo i comandi di sistema (Ctrl+C, Ctrl+X, Ctrl+V o Menu Tasto Destro)
        CommandManager.AddPreviewExecutedHandler(this, OnPreviewCommandExecuted);
    }

    private static void OnPasswordOrRevealChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is PasswordTextBox pb) {
            pb.UpdateDisplayText();
        }
    }

    private void UpdateDisplayText() {
        if (_isUpdatingText) {
            return;
        }

        _isUpdatingText = true;

        int savedCaret = CaretIndex;
        string currentPassword = Password ?? string.Empty;

        Text = IsRevealed ? currentPassword : new string(PasswordChar, currentPassword.Length);

        CaretIndex = Math.Min(savedCaret, Text.Length);

        _isUpdatingText = false;
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
        if (!string.IsNullOrEmpty(e.Text) && e.Text != "\r") {
            ReplaceSelectionWithText(e.Text);
            e.Handled = true;
        }
        base.OnPreviewTextInput(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e) {
        if (e.Key == Key.Space) {
            ReplaceSelectionWithText(" ");
            e.Handled = true;
        }
        else if (e.Key == Key.Back) {
            HandleBackspace();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete) {
            HandleDelete();
            e.Handled = true;
        }
        base.OnPreviewKeyDown(e);
    }

    private void ReplaceSelectionWithText(string insertText) {
        _isUpdatingText = true;
        int start = SelectionStart;
        int len = SelectionLength;
        string currentPass = Password ?? string.Empty;

        Password = currentPass.Remove(start, len).Insert(start, insertText);

        Text = IsRevealed ? Password : new string(PasswordChar, Password.Length);

        CaretIndex = start + insertText.Length;
        SelectionLength = 0;

        _isUpdatingText = false;
    }

    private void HandleBackspace() {
        if (SelectionLength > 0) {
            ReplaceSelectionWithText(string.Empty);
        }
        else if (CaretIndex > 0) {
            int newCaret = CaretIndex - 1;
            string currentPass = Password ?? string.Empty;
            Password = currentPass.Remove(newCaret, 1);

            _isUpdatingText = true;
            Text = IsRevealed ? Password : new string(PasswordChar, Password.Length);
            CaretIndex = newCaret;
            _isUpdatingText = false;
        }
    }

    private void HandleDelete() {
        if (SelectionLength > 0) {
            ReplaceSelectionWithText(string.Empty);
        }
        else if (CaretIndex < Text.Length) {
            int currentCaret = CaretIndex;
            string currentPass = Password ?? string.Empty;
            Password = currentPass.Remove(currentCaret, 1);

            _isUpdatingText = true;
            Text = IsRevealed ? Password : new string(PasswordChar, Password.Length);
            CaretIndex = currentCaret;
            _isUpdatingText = false;
        }
    }

    // Il nuovo gestore dei comandi
    private void OnPreviewCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
        if (e.Command == ApplicationCommands.Copy) {
            if (!IsRevealed) {
                e.Handled = true; // Niente copia dei pallini
            }
        }
        else if (e.Command == ApplicationCommands.Cut) {
            if (!IsRevealed) {
                e.Handled = true; // Niente taglio dei pallini
            }
            else {
                // Gestiamo noi il Taglio per aggiornare la variabile Password
                if (SelectionLength > 0) {
                    Clipboard.SetText(SelectedText);
                    ReplaceSelectionWithText(string.Empty);
                }
                e.Handled = true;
            }
        }
        else if (e.Command == ApplicationCommands.Paste) {
            // Gestiamo noi l'Incolla 
            if (Clipboard.ContainsText()) {
                ReplaceSelectionWithText(Clipboard.GetText());
            }
            e.Handled = true;
        }
    }
}