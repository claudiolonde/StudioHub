using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudioHub.Controls;

namespace StudioHub.ViewModels;

/// <summary>
/// ViewModel per la gestione dei dettagli di un modello di stampa unione (Mail Merge).
/// Permette la creazione o modifica di un modello, gestendo nome, descrizione e validazione.
/// </summary>
public partial class EditMailMergeTemplateDetailsViewModel : ObservableObject
{
    /// <summary>
    /// Evento sollevato quando il modello è stato salvato con successo.
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Inizializza una nuova istanza della classe <see cref="EditMailMergeTemplateDetailsViewModel"/>.
    /// </summary>
    /// <param name="existingNames">Elenco dei nomi già esistenti per la validazione di unicità.</param>
    /// <param name="currentName">Nome corrente del modello (vuoto per nuovo modello).</param>
    /// <param name="currentDescription">Descrizione corrente del modello.</param>
    public EditMailMergeTemplateDetailsViewModel(IEnumerable<string>? existingNames, string currentName = "", string currentDescription = "")
    {
        _currentName = currentName;
        Name = currentName;
        _currentDescription = currentDescription;
        Description = currentDescription;
        _existingNames = existingNames ?? [];
    }

    private readonly IEnumerable<string> _existingNames = [];

    /// <summary>
    /// Nome del modello di stampa unione.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;
    private readonly string _currentName = string.Empty;

    /// <summary>
    /// Descrizione del modello di stampa unione.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _description = string.Empty;
    private readonly string _currentDescription = string.Empty;

    /// <summary>
    /// Indica se il modello è stato salvato con successo.
    /// </summary>
    public bool IsSaved { get; private set; }

    /// <summary>
    /// Determina se il comando di salvataggio può essere eseguito.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the save is allowed; <c>false</c> otherwise.
    /// </returns>
    private bool CanSave()
    {
        bool hasName = !string.IsNullOrWhiteSpace(Name);

        // nuovo modello, basta che ci sia il nome
        if (string.IsNullOrEmpty(_currentName))
        {
            return hasName;
        }

        // modifica modello, deve esserci il nome e almeno un cambiamento
        bool isChanged = Name != _currentName || Description != _currentDescription;
        return hasName && isChanged;
    }

    /// <summary>
    /// Salva il modello di stampa unione, previa validazione di unicità del nome.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (_existingNames.Contains(Name, StringComparer.InvariantCultureIgnoreCase))
        {
            Dialog.Show("Esiste già un modello con questo nome.", DialogIcon.Error);
            SaveCommand.NotifyCanExecuteChanged();
            return;
        }

        IsSaved = true;
        Saved?.Invoke(this, EventArgs.Empty);
    }
}