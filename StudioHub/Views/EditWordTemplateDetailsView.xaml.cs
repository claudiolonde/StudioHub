using System.Windows;

namespace StudioHub.Views;

/// <summary>
/// Dialogo modale per l'inserimento o la modifica di nome e descrizione di un modello Word.
/// </summary>
public partial class EditWordTemplateDetailsView {

    /// <summary>Nome confermato dall'utente.</summary>
    public string TemplateName { get; private set; } = string.Empty;

    /// <summary>Descrizione confermata dall'utente.</summary>
    public string TemplateDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Inizializza il dialogo con valori predefiniti opzionali.
    /// </summary>
    /// <param name="name">Nome iniziale del modello.</param>
    /// <param name="description">Descrizione iniziale del modello.</param>
    public EditWordTemplateDetailsView(string name = "", string description = "") {
        InitializeComponent();
        NameBox.Text = name;
        DescriptionBox.Text = description;
    }

    /// <summary>
    /// Mostra il dialogo modale e restituisce nome e descrizione confermati, o <see langword="null"/> se annullato.
    /// </summary>
    /// <param name="owner">Finestra proprietaria del dialogo.</param>
    /// <param name="name">Nome iniziale (vuoto per un nuovo template).</param>
    /// <param name="description">Descrizione iniziale (vuota per un nuovo template).</param>
    /// <returns>Tupla con nome e descrizione, oppure <see langword="null"/> se annullato.</returns>
    public static (string Name, string Description)? ShowDialog(Window? owner, string name = "", string description = "") {
        EditWordTemplateDetailsView dialog = new(name, description) { Owner = owner };
        return dialog.ShowDialog() == true
            ? (dialog.TemplateName, dialog.TemplateDescription)
            : null;
    }

    /// <summary>
    /// Valida i campi e conferma il dialogo.
    /// </summary>
    private void saveButton_Click(object sender, RoutedEventArgs e) {
        string name = NameBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name)) {
            NameBox.Focus();
            return;
        }

        TemplateName = name;
        TemplateDescription = DescriptionBox.Text?.Trim() ?? string.Empty;
        DialogResult = true;
    }
        
    /// <summary>
    /// Annulla il dialogo senza salvare.
    /// </summary>
    private void cancelButton_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
