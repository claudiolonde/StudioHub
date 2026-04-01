using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows; // Per MessageBox semplice, sostituibile con dialog custom
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Office.Interop.Word;
using StudioHub.Models;
using StudioHub.Services;

namespace StudioHub.ViewModels;

public partial class ManageWordTemplatesViewModel : ObservableObject {
    private readonly ManageWordTemplatesService _templateService;
    private CancellationTokenSource? _wordCancellationTokenSource;

    // Lista master nascosta per mantenere tutti i dati in memoria
    private List<WordTemplate> _allTemplates = [];

    // Lista bindata alla ListBox
    [ObservableProperty]
    private ObservableCollection<WordTemplate> _filteredTemplates = [];

    // Bindato alla TextBox di ricerca. Quando cambia, innesca il filtro.
    [ObservableProperty]
    private string _searchText = string.Empty;

    // Bindato al TextBlock per mostrare "X su Y elementi"
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

    public Action<WordTemplate>? ShowEditDetailsDialog { get; set; }
    public Action? ShowNewTemplateDialog { get; set; }

    public ManageWordTemplatesViewModel() {
        _templateService = new ManageWordTemplatesService();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadTemplatesAsync() {
        IsBusy = true;
        StatusMessage = "Caricamento modelli in corso...";
        try {
            _allTemplates = await _templateService.GetAllTemplatesAsync();
            ApplyFilter(); // Popola la UI la prima volta
        }
        catch (Exception ex) {
            MessageBox.Show($"Errore durante il caricamento: {ex.Message}", "Errore");
        }
        finally {
            IsBusy = false;
        }
    }
    // Hook generato da CommunityToolkit per reagire al cambio di SearchText
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
    private async System.Threading.Tasks.Task NewTemplateAsync() {
        // Il flusso "Nuovo" è complesso (Word -> poi Dettagli). 
        // In base al manifesto, apriamo prima il flusso Word, poi se va a buon fine
        // chiederemo all'utente nome e descrizione.
        IsBusy = true;
        StatusMessage = "Avvio di Word per il nuovo modello...";
        _wordCancellationTokenSource = new CancellationTokenSource();

        try {
            // Questa chiamata bloccherà (asincronamente) finché Word non viene chiuso
            var newTemplateData = await _templateService.CreateNewTemplateContentAsync(_wordCancellationTokenSource.Token);

            // Se arriviamo qui, Word è stato chiuso e salvato correttamente
            // Mostriamo il dialog per i dettagli finali (Nome/Descrizione)
            ShowNewTemplateDialog?.Invoke();

            // Dopo il dialog, ricarichiamo la lista
            await LoadTemplatesAsync();
        }
        catch (OperationCanceledException) {
            // Utente ha annullato il flusso Word
        }
        catch (Exception ex) {
            MessageBox.Show($"Errore in Word: {ex.Message}", "Errore");
        }
        finally {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfSelected))]
    private async System.Threading.Tasks.Task EditContentAsync() {
        if (SelectedTemplate == null) return;

        IsBusy = true;
        StatusMessage = $"Modifica contenuto di '{SelectedTemplate.Name}' in Word...";
        _wordCancellationTokenSource = new CancellationTokenSource();

        try {
            var updatedContent = await _templateService.EditTemplateContentAsync(
                SelectedTemplate.Id,
                SelectedTemplate.FileContent,
                _wordCancellationTokenSource.Token);

            // Aggiorniamo il record in memoria
            SelectedTemplate = SelectedTemplate with { FileContent = updatedContent };

            // Ricarichiamo o notifichiamo l'aggiornamento
            await LoadTemplatesAsync();
        }
        catch (OperationCanceledException) { /* Ignora annullamento */ }
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

        // Apre il modale per rinominare/cambiare descrizione
        ShowEditDetailsDialog?.Invoke(SelectedTemplate);

        // Dopo il salvataggio nel modale, potremmo chiamare LoadTemplatesAsync()
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfSelected))]
    private async System.Threading.Tasks.Task DuplicateAsync() {
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
    private async System.Threading.Tasks.Task DeleteAsync() {
        if (SelectedTemplate == null) return;

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
        // Se l'utente preme il bottone "Annulla" sull'overlay
        _wordCancellationTokenSource?.Cancel();
    }

    private bool CanExecuteIfSelected() => SelectedTemplate != null;
}
