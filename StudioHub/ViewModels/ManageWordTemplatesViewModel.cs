using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class ManageWordTemplatesViewModel : ObservableObject {

    private readonly ManageWordTemplatesService _templateService;
    private CancellationTokenSource? _wordCancellationTokenSource;

    private string _targetApp;
    private string[] _appHeaders;

    private List<WordTemplate> _allTemplates = [];

    [ObservableProperty]
    private ObservableCollection<WordTemplate> _filteredTemplates = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _itemsCountText = "0 su 0 elementi";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditDetailsCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditContentCommand))]
    private WordTemplate? _selectedTemplate;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Modificato Action per passare anche il contenuto binario appena generato (in caso di Nuovo)
    public Action<WordTemplate>? ShowEditDetailsDialog { get; set; }
    public Action<byte[]>? ShowNewTemplateDialog { get; set; }

    // [FIX 2] Aggiunti i parametri al costruttore per ricevere il contesto dall'App chiamante
    public ManageWordTemplatesViewModel() {
        _templateService = new ManageWordTemplatesService();
        _targetApp = string.Empty;
        _appHeaders = [];
        // In design time, le liste possono essere pre-popolate per vedere come appare la UI
    }

    public void Initialize(string targetApp, string[] appHeaders) {
        ArgumentException.ThrowIfNullOrEmpty(targetApp);
        ArgumentNullException.ThrowIfNull(appHeaders);
        _targetApp = targetApp;
        _appHeaders = appHeaders;
    }

    // 3. Metodo separato per il caricamento dati (chiamato ad esempio nell'evento Loaded della View)
    [RelayCommand]
    private async Task LoadTemplatesAsync() {
        if (_targetApp == null) return; // Sicurezza: se non è stato fatto il Setup, non carica

        IsBusy = true;
        StatusMessage = "Caricamento modelli in corso...";
        try {
            _allTemplates = await _templateService.GetAllTemplatesAsync();
            ApplyFilter();
        }
        catch (Exception ex) {
            MessageBox.Show($"Errore durante il caricamento: {ex.Message}", "Errore");
        }
        finally {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) {
        ApplyFilter();
    }

    private void ApplyFilter() {
        if (string.IsNullOrWhiteSpace(SearchText)) {
            FilteredTemplates = new ObservableCollection<WordTemplate>(_allTemplates);
        }
        else {
            var lowerSearch = SearchText.ToLowerInvariant();
            var filtered = _allTemplates.Where(t =>
                t.Name.ToLowerInvariant().Contains(lowerSearch) ||
                t.Description.ToLowerInvariant().Contains(lowerSearch));

            FilteredTemplates = new ObservableCollection<WordTemplate>(filtered);
        }

        UpdateCountText();
    }

    private void UpdateCountText() {
        ItemsCountText = $"{FilteredTemplates.Count} su {_allTemplates.Count} elementi";
    }

    [RelayCommand]
    private async Task NewTemplateAsync() {
        IsBusy = true;
        StatusMessage = "Avvio di Word per il nuovo modello...";
        _wordCancellationTokenSource = new CancellationTokenSource();

        try {
            // [FIX 1] Passiamo le intestazioni dell'app e 'null' come contenuto esistente
            var newTemplateContent = await _templateService.EditTemplateContentAsync(
                _appHeaders,
                null,
                _wordCancellationTokenSource.Token);

            // Passiamo il file generato alla UI per completare il salvataggio con Nome/Descrizione
            ShowNewTemplateDialog?.Invoke(newTemplateContent);

            await LoadTemplatesAsync();
        }
        catch (OperationCanceledException) { /* Ignora annullamento utente */ }
        catch (Exception ex) {
            MessageBox.Show($"Errore in Word: {ex.Message}", "Errore");
        }
        finally {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfSelected))]
    private async Task EditContentAsync() {
        if (SelectedTemplate == null) return;

        // [FIX 3] Controllo Concorrenza
        if (SelectedTemplate.Locked) {
            MessageBox.Show("Il modello è attualmente in uso da un altro utente.", "File Bloccato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        StatusMessage = $"Modifica contenuto di '{SelectedTemplate.Name}' in Word...";
        _wordCancellationTokenSource = new CancellationTokenSource();

        try {
            // [FIX 1] Aggiornata la firma per passare Headers e Content
            var updatedContent = await _templateService.EditTemplateContentAsync(
                _appHeaders,
                SelectedTemplate.Content,
                _wordCancellationTokenSource.Token);

            SelectedTemplate = SelectedTemplate with {
                Content = updatedContent,
                Modified = DateTime.UtcNow
            };

            // TODO: Qui andrebbe chiamato un ipotetico _templateService.UpdateTemplateAsync(SelectedTemplate)

            await LoadTemplatesAsync();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            MessageBox.Show($"Errore durante la modifica: {ex.Message}", "Errore");
        }
        finally {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfSelected))]
    private void EditDetails() {
        if (SelectedTemplate == null) return;

        // [FIX 3] Controllo Concorrenza
        if (SelectedTemplate.Locked) {
            MessageBox.Show("Il modello è attualmente in uso da un altro utente.", "File Bloccato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ShowEditDetailsDialog?.Invoke(SelectedTemplate);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfSelected))]
    private async Task DuplicateAsync() {
        if (SelectedTemplate == null) return;

        IsBusy = true;
        StatusMessage = "Duplicazione in corso...";

        try {
            string newName = $"{SelectedTemplate.Name} - Copia {DateTime.Now:yyyyMMdd_HHmmss}";
            await _templateService.DuplicateTemplateAsync(SelectedTemplate.Id, newName);
            await LoadTemplatesAsync();
        }
        catch (Exception ex) {
            MessageBox.Show($"Errore durante la duplicazione: {ex.Message}", "Errore");
        }
        finally {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfSelected))]
    private async Task DeleteAsync() {
        if (SelectedTemplate == null) return;

        // [FIX 3] Controllo Concorrenza
        if (SelectedTemplate.Locked) {
            MessageBox.Show("Impossibile eliminare: il modello è attualmente in uso.", "File Bloccato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Sei sicuro di voler eliminare il modello '{SelectedTemplate.Name}'?",
                                     "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes) {
            IsBusy = true;
            StatusMessage = "Eliminazione in corso...";
            try {
                await _templateService.DeleteTemplateAsync(SelectedTemplate.Id);
                await LoadTemplatesAsync();
            }
            catch (Exception ex) {
                MessageBox.Show($"Errore durante l'eliminazione: {ex.Message}", "Errore");
            }
            finally {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void CancelWordOperation() {
        _wordCancellationTokenSource?.Cancel();
    }

    private bool CanExecuteIfSelected() => SelectedTemplate != null;
}
