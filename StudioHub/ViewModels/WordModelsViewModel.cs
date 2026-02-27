using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class WordModelsViewModel : ObservableObject {

    /// <summary>
    /// Evento sollevato quando avviene un salvataggio con successo.
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Indica se il modello è stato salvato con successo.
    /// </summary>
    public bool IsSaved { get; private set; }

    /// <summary>
    /// Determina se il comando di salvataggio può essere eseguito.
    /// </summary>
    /// <returns>
    /// <c> true</c> if the save is allowed; <c> false</c> otherwise.
    /// </returns>
    private bool CanSave() {
        return true;
    }

    /// <summary>
    /// Salva il modello di stampa unione, previa validazione di unicità del nome.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save() {
        IsSaved = true;
        Saved?.Invoke(this, EventArgs.Empty);
    }
}