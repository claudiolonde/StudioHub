using System.Windows;

namespace StudioHub.Views;

/// <summary>
/// Finestra di dialogo per la modifica dei dettagli di un template Word.
/// </summary>
public partial class EditWordTemplateDetailsView {

    /// <summary>
    /// Ottiene il nome del template.
    /// </summary>
    public string TemplateName { get; private set; } = string.Empty;

    /// <summary>
    /// Ottiene la descrizione del template.
    /// </summary>
    public string TemplateDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Inizializza una nuova istanza della finestra di dialogo con nome e descrizione opzionali.
    /// </summary>
    /// <param name="name">Nome iniziale del template.</param>
    /// <param name="description">Descrizione iniziale del template.</param>
    public EditWordTemplateDetailsView(string name = "", string description = "") {
        InitializeComponent();

        TemplateName = txtName.Text = name;
        TemplateDescription = txtDescription.Text = description;

        txtName.TextChanged += txt_TextChanged;
        txtDescription.TextChanged += txt_TextChanged;
        btnSave.Click += btnSave_Click;
    }

    /// <summary>
    /// Gestisce la modifica del testo nei campi nome e descrizione, abilitando o disabilitando il pulsante Salva.
    /// </summary>
    /// <param name="sender">Origine dell'evento.</param>
    /// <param name="e">Argomenti dell'evento di modifica testo.</param>
    private void txt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {

        btnSave.IsEnabled = canSave();
    }

    /// <summary>
    /// Determina se è possibile salvare i dati in base allo stato corrente dei campi.
    /// </summary>
    /// <returns><see langword="true"/> if saving is allowed; otherwise, <see langword="false"/>.</returns>
    private bool canSave() {

        string currentName = txtName.Text.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentName)) {
            return false;
        }
        string currentDescription = txtDescription.Text.Trim() ?? string.Empty;

        return !(string.Equals(TemplateName, currentName, StringComparison.Ordinal)
              && string.Equals(TemplateDescription, currentDescription, StringComparison.Ordinal));
    }

    /// <summary>
    /// Mostra la finestra di dialogo per l'editing del template e restituisce i valori inseriti se confermati.
    /// </summary>
    /// <param name="owner">Finestra proprietaria.</param>
    /// <param name="name">Nome iniziale del template.</param>
    /// <param name="description">Descrizione iniziale del template.</param>
    /// <returns>Tuple con nome e descrizione se confermato; altrimenti <see langword="null"/>.</returns>
    public static (string Name, string Description)? Open(Window? owner, string name = "", string description = "") {
        EditWordTemplateDetailsView dialog = new(name, description) { Owner = owner };
        return dialog.ShowDialog() == true
            ? (dialog.TemplateName, dialog.TemplateDescription)
            : null;
    }

    /// <summary>
    /// Gestisce il click sul pulsante Salva, aggiorna le proprietà e chiude la finestra con esito positivo.
    /// </summary>
    /// <param name="sender">Origine dell'evento.</param>
    /// <param name="e">Argomenti dell'evento click.</param>
    private void btnSave_Click(object sender, RoutedEventArgs e) {
        TemplateName = txtName.Text.Trim();
        TemplateDescription = txtDescription.Text?.Trim() ?? string.Empty;
        DialogResult = true;
    }
}
