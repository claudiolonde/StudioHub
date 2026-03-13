using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class WordTemplatesViewModel : ObservableObject {

    public static string Title => "Gestione modelli Word";
    public string Path {
        get; set;
    }

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTemplatesCount))]
    private IEnumerable<string> _totalTemplates = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredTemplatesCount))]
    private ObservableCollection<string> _filteredTemplates = [];

    public int TotalTemplatesCount => TotalTemplates?.Count() ?? 0;

    public int FilteredTemplatesCount => FilteredTemplates?.Count ?? 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(
        nameof(EditTemplateCommand),
        nameof(CloneTemplateCommand),
        nameof(RenameTemplateCommand),
        nameof(DeleteTemplateCommand))]
    private string? _selectedTemplate = null;

    public WordTemplatesViewModel(string path, string[] headers) {
        Path = path;
        TotalTemplates = IO.GetVisibleFileNames(Path, "*.doc;*.docx");
        applyFilter();
    }

    /// <summary>
    /// Filtra la collezione dei modelli in base al testo inserito dall'utente. Aggiorna la proprietà
    /// <see cref="FilteredTemplates"/> per il data binding con la UI.
    /// </summary>
    private void applyFilter() {
        // Gestione del caso in cui il filtro sia vuoto o composto da soli spazi bianchi
        if (string.IsNullOrWhiteSpace(FilterText)) {
            // Se TotalTemplates è null, inizializza una collezione vuota tramite collection expression
            FilteredTemplates = TotalTemplates is null
                                ? []
                                : new ObservableCollection<string>(TotalTemplates);
        }
        else {
            // Normalizzazione della stringa di ricerca per un confronto case-insensitive
            string filter = FilterText.Trim().ToLowerInvariant();

            // Filtraggio della collezione tramite LINQ basato sulla presenza della sottostringa
            FilteredTemplates = TotalTemplates is null
                                ? []
                                : new ObservableCollection<string>(
                                    TotalTemplates.Where(t => t != null && t.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                                    );
        }
    }

    partial void OnFilterTextChanged(string value) {
        applyFilter();
    }

    [RelayCommand()]
    public void NewTemplate() {
    }

    private bool canModifyTemplate() {
        return !string.IsNullOrWhiteSpace(SelectedTemplate);
    }

    [RelayCommand(CanExecute = nameof(canModifyTemplate))]
    public void EditTemplate() {
    }

    [RelayCommand(CanExecute = nameof(canModifyTemplate))]
    public void CloneTemplate() {

    }

    [RelayCommand(CanExecute = nameof(canModifyTemplate))]
    public void RenameTemplate() {
    }

    /// <summary>
    /// Comando per l'eliminazione logica del modello selezionato. Sposta il file in una cartella di backup (trash)
    /// rinominandolo con un timestamp.
    /// </summary>
    [RelayCommand(CanExecute = nameof(canModifyTemplate))]
    public void DeleteTemplate() {

        // Visualizza dialogo di conferma; se l'utente seleziona "Annulla" (indice 0), interrompe l'esecuzione
        MessageBoxResult result = MessageBox.Show(
            $"Eliminare il modello selezionato:\n{SelectedTemplate}",
            "Gestione modelli Word",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning
        );
        if (result == MessageBoxResult.OK) {
            return;
        }

        // Verifica esistenza del file sorgente nel path specificato
        string sourceFilename = System.IO.Path.Combine(Path, SelectedTemplate!);
        if (!System.IO.File.Exists(sourceFilename)) {
            return;
        }

        // Gestione della cartella cestino: creazione e impostazione attributo Hidden se non esistente
        string trashPath = System.IO.Path.Combine(Path, Hub.TrashFolderName);
        if (!System.IO.Directory.Exists(trashPath)) {
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(trashPath);
            directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;
        }

        // Generazione del nuovo nome file con prefisso temporale per evitare collisioni
        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_");
        string newFilename = $"{timeStamp}{SelectedTemplate}";

        try {
            // Esegue lo spostamento fisico del file verso la cartella trash
            System.IO.File.Move(sourceFilename, System.IO.Path.Combine(trashPath, newFilename));

            // Aggiorna la collezione dei template visibili e applica i filtri correnti alla UI
            TotalTemplates = IO.GetVisibleFileNames(Path, "*.doc;*.docx");
            applyFilter();

        }
        catch (System.IO.IOException) {
            // Gestione errore in caso di file lock (es. documento aperto in Microsoft Word)
            MessageBox.Show(
                "Impossibile eliminare il modello, assicurati che non sia aperto in Word.",
                "Gestione modelli Word",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}
