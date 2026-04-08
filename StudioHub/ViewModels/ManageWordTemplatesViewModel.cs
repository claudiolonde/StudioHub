using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class ManageWordTemplatesViewModel : ObservableObject {

    private readonly ManageWordTemplatesService _service;

    private List<WordTemplate> _storedTemplates = [];
    [ObservableProperty]
    private ObservableCollection<WordTemplate> _displayedTemplates = [];
    [ObservableProperty]
    private string _searchText = string.Empty;
    [ObservableProperty]
    private string _itemsCountText = "0 su 0 elementi";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(deleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(duplicateCommand))]
    [NotifyCanExecuteChangedFor(nameof(editDetailsCommand))]
    [NotifyCanExecuteChangedFor(nameof(editContentCommand))]
    private WordTemplate? _selectedTemplate;

    [ObservableProperty]
    private bool _isBusy;

    private string _targetApp;
    private string[] _appHeaders;

    /// <summary>
    /// Inizializza il ViewModel con un servizio vuoto e valori predefiniti.
    /// </summary>
    public ManageWordTemplatesViewModel() {
        _service = new ManageWordTemplatesService();
        _targetApp = string.Empty;
        _appHeaders = [];
    }

    /// <summary>
    /// Imposta il contesto dell'applicazione chiamante.
    /// </summary>
    /// <param name="targetApp">
    /// Nome dell'applicazione target.
    /// </param>
    /// <param name="appHeaders">
    /// Intestazioni di colonna per il datasource di Word.
    /// </param>
    internal async Task InitializeAsync(string targetApp, string[] appHeaders) {
        ArgumentException.ThrowIfNullOrEmpty(targetApp);
        ArgumentNullException.ThrowIfNull(appHeaders);
        _targetApp = targetApp;
        _appHeaders = appHeaders;
        await loadTemplatesAsync();
    }

    public Func<string, IEnumerable<string>, string, Task<(string Name, string Description)?>>? RequestTemplateDetails { get; set; }

    /// <summary>
    /// Carica i template dell'applicazione corrente dal database. Da invocare nell'evento Loaded della View.
    /// </summary>
    [RelayCommand]
    private async Task loadTemplatesAsync() {

        IsBusy = true;
        try {
            _storedTemplates = await ManageWordTemplatesService.GetTemplatesAsync(_targetApp);
            applyFilter();
        }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore durante il caricamento dei modelli: {ex.Message}");
        }
        finally {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) {
        applyFilter();
    }

    /// <summary>
    /// Filtra i template visualizzati in base al testo di ricerca corrente.
    /// </summary>
    private void applyFilter() {

        if (string.IsNullOrWhiteSpace(SearchText)) {
            DisplayedTemplates = new ObservableCollection<WordTemplate>(_storedTemplates);

        }
        else {
            // Cache locale per evitare accessi ripetuti alla property e catturare snapshot coerente
            string searchText = SearchText;
            List<WordTemplate> filtered = new(_storedTemplates.Count);

            for (int i = 0; i < _storedTemplates.Count; i++) {
                WordTemplate t = _storedTemplates[i];
                if (t.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)) {
                    filtered.Add(t);
                }
            }
            DisplayedTemplates = new ObservableCollection<WordTemplate>(filtered);
        }
        updateCountText();
    }

    /// <summary>
    /// Aggiorna il testo del contatore degli elementi.
    /// </summary>
    private void updateCountText() {
        ItemsCountText = $"{DisplayedTemplates.Count} su {_storedTemplates.Count} elementi";
    }

    /// <summary>
    /// Apre Word per la creazione di un nuovo template, richiede nome/descrizione e salva nel DB.
    /// </summary>
    [RelayCommand]
    private async Task newTemplateAsync() {

        IsBusy = true;

        try {
            byte[] content = await _service.EditTemplateContentAsync(-1, _appHeaders);
            if (content.Length == 0) {
                return;
            }

            IEnumerable<string> unavailableNames = _storedTemplates.Select(t => t.Name);
            (string Name, string Description)? details = await (RequestTemplateDetails?.Invoke("", unavailableNames, "")
                                            ?? Task.FromResult<(string Name, string Description)?>(null));
            if (details == null) {
                //> Chiedere conferma all'utente
                return;
            }

            WordTemplate newTemplate = new() {
                Name = details.Value.Name,
                Description = details.Value.Description,
                Content = content,
                TargetApp = _targetApp,
            };

            await ManageWordTemplatesService.SaveTemplateAsync(newTemplate);
            await loadTemplatesAsync();
        }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore in Microsoft Word: {ex.Message}");
        }
        finally {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Apre Word per modificare il contenuto del template selezionato e salva le modifiche nel DB.
    /// </summary>
    [RelayCommand(CanExecute = nameof(canExecuteIfSelected))]
    private async Task editContentAsync() {

        if (SelectedTemplate == null) { return; }
        if (SelectedTemplate.Locked) {
            Dialog.Show(DialogType.Error, "Il modello è attualmente in uso da un altro utente.");
            return;
        }

        int id = SelectedTemplate.Id;
        await ManageWordTemplatesService.SetTemplateLockAsync(id, true);

        IsBusy = true;

        try {
            byte[] content = await _service.EditTemplateContentAsync(id, _appHeaders);
            if (content.Length == 0) { return; }

            SelectedTemplate = SelectedTemplate with {
                Content = content,
                Modified = DateTime.UtcNow
            };

            await ManageWordTemplatesService.SaveTemplateAsync(SelectedTemplate);
            await loadTemplatesAsync();
        }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore in Microsoft Word: {ex.Message}");
        }
        finally {
            await ManageWordTemplatesService.SetTemplateLockAsync(id, false);
            IsBusy = false;
        }
    }

    /// <summary>
    /// Apre il dialogo per modificare nome e descrizione del template selezionato e persiste le modifiche.
    /// </summary>
    [RelayCommand(CanExecute = nameof(canExecuteIfSelected))]
    private async Task editDetailsAsync() {

        if (SelectedTemplate == null) { return; }
        if (SelectedTemplate.Locked) {
            Dialog.Show(DialogType.Error, "Il modello è attualmente in uso da un altro utente.");
            return;
        }

        IEnumerable<string> unavailableNames = _storedTemplates.Where(t => t.Id != SelectedTemplate.Id).Select(t => t.Name);
        (string Name, string Description)? details = await (RequestTemplateDetails?.Invoke(SelectedTemplate.Name, unavailableNames, SelectedTemplate.Description)
                                            ?? Task.FromResult<(string Name, string Description)?>(null));
        if (details == null) {
            //> Chiedere conferma all'utente
            return;
        }

        IsBusy = true;
        try {
            await ManageWordTemplatesService.UpdateTemplateDetailsAsync(SelectedTemplate.Id, details.Value.Name, details.Value.Description);
            await loadTemplatesAsync();
        }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore durante la modifica dei dettagli: {ex.Message}");
        }
        finally {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Duplica il template selezionato con nome univoco basato sulla data corrente.
    /// </summary>
    [RelayCommand(CanExecute = nameof(canExecuteIfSelected))]
    private async Task duplicateAsync() {

        if (SelectedTemplate == null) { return; }

        IsBusy = true;
        try {
            string copyName = $"{SelectedTemplate.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
            await ManageWordTemplatesService.DuplicateTemplateAsync(SelectedTemplate.Id, copyName);
            await loadTemplatesAsync();
        }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore durante la duplicazione: {ex.Message}");
        }
        finally {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Elimina il template selezionato previa conferma dell'utente.
    /// </summary>
    [RelayCommand(CanExecute = nameof(canExecuteIfSelected))]
    private async Task deleteAsync() {

        if (SelectedTemplate == null) { return; }
        if (SelectedTemplate.Locked) {
            Dialog.Show(DialogType.Error, "Il modello è attualmente in uso da un altro utente.");
            return;
        }

        int result = Dialog.Show(
                         DialogType.Warning,
                         $"Sei sicuro di voler eliminare il modello '{SelectedTemplate.Name}'?",
                         ["Elimina", "*Annulla"]
                     );

        if (result == 0) {
            IsBusy = true;
            try {
                await ManageWordTemplatesService.DeleteTemplateAsync(SelectedTemplate.Id);
                await loadTemplatesAsync();
            }
            catch (Exception ex) {
                Dialog.Show(DialogType.Error, $"Errore durante l'eliminazione: {ex.Message}");
            }
            finally {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// Restituisce <c>true</c> se è selezionato un template.
    /// </summary>
    private bool canExecuteIfSelected() {
        return SelectedTemplate != null;
    }
}
