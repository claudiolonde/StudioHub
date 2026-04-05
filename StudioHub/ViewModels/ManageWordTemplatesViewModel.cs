using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class ManageWordTemplatesViewModel : ObservableObject {

    private readonly ManageWordTemplatesService _service;
    private CancellationTokenSource? _wordCTS;

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
    /// <param name="targetApp">Nome dell'applicazione target.</param>
    /// <param name="appHeaders">Intestazioni di colonna per il datasource di Word.</param>
    internal async Task Initialize(string targetApp, string[] appHeaders) {
        ArgumentException.ThrowIfNullOrEmpty(targetApp);
        ArgumentNullException.ThrowIfNull(appHeaders);
        _targetApp = targetApp;
        _appHeaders = appHeaders;
        await loadTemplatesAsync();
    }

    /// <summary>
    /// Callback per richiedere nome e descrizione di un nuovo template. Riceve il contenuto binario appena generato;
    /// restituisce i dettagli o <c> null</c> se annullato.
    /// </summary>
    public Func<byte[], Task<(string Name, string Description)?>>? RequestNewTemplateDetails { get; set; }

    /// <summary>
    /// Callback per richiedere la modifica di nome e descrizione di un template esistente. Riceve il template
    /// selezionato; restituisce i dettagli aggiornati o <see langword="null"/> se annullato.
    /// </summary>
    public Func<WordTemplate, Task<(string Name, string Description)?>>? RequestEditTemplateDetails { get; set; }

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
            string lowerText = SearchText.ToLowerInvariant();
            IEnumerable<WordTemplate> filtered = _storedTemplates.Where(t => {
                return t.Name.Contains(lowerText, StringComparison.InvariantCultureIgnoreCase) ||
                       t.Description.Contains(lowerText, StringComparison.InvariantCultureIgnoreCase);
            });
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
        _wordCTS = new CancellationTokenSource();

        try {
            byte[] content = await _service.EditTemplateContentAsync(-1, _appHeaders, _wordCTS.Token);
            // Contenuto vuoto, utente ha annullato in Word
            if (content.Length == 0) {
                return;
            }

            // La callback restituisce il risultato del dialogo, senza di essa non si salva nulla
            (string Name, string Description)? details = await (RequestNewTemplateDetails?.Invoke(content)
                                            ?? Task.FromResult<(string Name, string Description)?>(null));
            // Utente ha annullato il dialogo dei dettagli
            if (details == null) {
                return;
            }

            // Verifica se esiste già un template con lo stesso nome
            bool nameExists = _storedTemplates.Any(t =>
                t.Name.Equals(details.Value.Name, StringComparison.InvariantCultureIgnoreCase));
            if (nameExists) {
                Dialog.Show(DialogType.Error, $"Esiste già un modello con il nome '{details.Value.Name}'.");
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
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore in Microsoft Word: {ex.Message}");
        }
        finally {
            _wordCTS?.Dispose();
            _wordCTS = null;
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
        _wordCTS = new CancellationTokenSource();

        try {
            byte[] content = await _service.EditTemplateContentAsync(id, _appHeaders, _wordCTS.Token);
            if (content.Length == 0) { return; }

            SelectedTemplate = SelectedTemplate with {
                Content = content,
                Modified = DateTime.UtcNow
            };

            await ManageWordTemplatesService.SaveTemplateAsync(SelectedTemplate);
            await loadTemplatesAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            Dialog.Show(DialogType.Error, $"Errore in Microsoft Word: {ex.Message}");
        }
        finally {
            await ManageWordTemplatesService.SetTemplateLockAsync(id, false);
            _wordCTS?.Dispose();
            _wordCTS = null;
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

        // La callback restituisce il risultato, solo se confermato si aggiorna il DB
        (string Name, string Description)? details = await (RequestEditTemplateDetails?.Invoke(SelectedTemplate)
                                                  ?? Task.FromResult<(string Name, string Description)?>(null));
        if (details == null) { return; }

        // Verifica se esiste già un altro template con lo stesso nome
        bool nameExists = _storedTemplates.Any(t =>
            t.Id != SelectedTemplate.Id &&
            t.Name.Equals(details.Value.Name, StringComparison.InvariantCultureIgnoreCase));
        if (nameExists) {
            Dialog.Show(DialogType.Error, $"Esiste già un modello con il nome '{details.Value.Name}'.");
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
    /// Annulla l'operazione Word in corso.
    /// </summary>
    [RelayCommand]
    private void cancelWordOperation() {
        _wordCTS?.Cancel();
    }

    /// <summary>
    /// Restituisce <see langword="true"/> se è selezionato un template.
    /// </summary>
    private bool canExecuteIfSelected() {
        return SelectedTemplate != null;
    }
}
