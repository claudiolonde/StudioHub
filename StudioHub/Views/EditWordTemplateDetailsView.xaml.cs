using System.Windows;

namespace StudioHub.Views;

/// <summary>
/// Finestra di dialogo per la modifica dei dettagli di un modello Word.
/// </summary>
public partial class EditWordTemplateDetailsView {

    /// <summary>
    /// Ottiene il nome del modello.
    /// </summary>
    public string TemplateName { get; private set; } = string.Empty;

    /// <summary>
    /// Ottiene la descrizione del modello.
    /// </summary>
    public string TemplateDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Insieme dei nomi di modelli non disponibili (già esistenti) per il controllo della duplicazione.
    /// </summary>
    private readonly HashSet<string> _unavailableNames;

    /// <summary>
    /// Inizializza una nuova istanza della finestra di dialogo con i dettagli del modello.
    /// </summary>
    /// <param name="name">Il nome iniziale del modello.</param>
    /// <param name="unavailableNames">Elenco dei nomi di modelli già esistenti.</param>
    /// <param name="description">La descrizione iniziale del modello.</param>
    public EditWordTemplateDetailsView(string name, IEnumerable<string> unavailableNames, string description) {
        InitializeComponent();

        TemplateName = txtName.Text = name;
        TemplateDescription = txtDescription.Text = description;
        _unavailableNames = new HashSet<string>(unavailableNames, StringComparer.OrdinalIgnoreCase);

        txtName.TextChanged += txt_TextChanged;
        txtDescription.TextChanged += txt_TextChanged;
        btnSave.Click += btnSave_Click;
    }

    /// <summary>
    /// Apre la finestra di dialogo come modale e restituisce i dettagli modificati del modello.
    /// </summary>
    /// <param name="owner">La finestra proprietaria della finestra di dialogo, o <see langword="null" />.</param>
    /// <param name="name">Il nome iniziale del modello.</param>
    /// <param name="unavailableNames">Elenco dei nomi di modelli già esistenti.</param>
    /// <param name="description">La descrizione iniziale del modello.</param>
    /// <returns>
    /// Una tupla contenente il nome e la descrizione modificati, o <see langword="null" /> se la finestra è stata annullata.
    /// </returns>
    public static (string Name, string Description)? Open(Window? owner, string name, IEnumerable<string> unavailableNames, string description) {
        EditWordTemplateDetailsView dialog = new(name, unavailableNames, description) { Owner = owner };
        return dialog.ShowDialog() == true
             ? (dialog.TemplateName, dialog.TemplateDescription)
             : null;
    }

    /// <summary>
    /// Gestisce l'evento di cambio testo nei campi di input e aggiorna lo stato del pulsante Salva.
    /// </summary>
    /// <param name="sender">L'oggetto che ha generato l'evento.</param>
    /// <param name="e">I dati dell'evento di cambio testo.</param>
    private void txt_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
        btnSave.IsEnabled = canSave();
    }

    /// <summary>
    /// Determina se il pulsante Salva deve essere abilitato in base allo stato dei campi.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> se i dati sono stati modificati e il nome non è vuoto;
    /// <see langword="false" /> diversamente.
    /// </returns>
    private bool canSave() {

        string currentName = txtName.Text.Trim();
        if (currentName.Length == 0) {
            return false;
        }
        string currentDescription = txtDescription.Text.Trim();

        return !(string.Equals(TemplateName, currentName, StringComparison.Ordinal)
             && string.Equals(TemplateDescription, currentDescription, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gestisce il clic del pulsante Salva e valida i dati prima di chiudere la finestra di dialogo.
    /// </summary>
    /// <param name="sender">L'oggetto che ha generato l'evento.</param>
    /// <param name="e">I dati dell'evento di clic.</param>
    private void btnSave_Click(object sender, RoutedEventArgs e) {

        string name = txtName.Text.Trim();
        if (_unavailableNames.Contains(name)) {
            Dialog.Show(DialogType.Error, $"Esiste già un modello con il nome '{name}'.");
            return;
        }

        TemplateName = name;
        TemplateDescription = txtDescription.Text.Trim();
        DialogResult = true;
    }
}
