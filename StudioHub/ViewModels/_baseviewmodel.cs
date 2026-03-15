using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class BaseViewModel : ObservableObject {

    /// <summary>
    /// Evento sollevato quando avviene un salvataggio con successo.
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Indica se il salvataggio è avvenuto con successo.
    /// </summary>
    public bool IsSaved { get; private set; }

    /// <summary>
    /// Determina se il comando di salvataggio può essere eseguito.
    /// </summary>
    /// <returns>
    /// <c> true</c> if the save is allowed; <c> false</c> otherwise.
    /// </returns>
    private bool canSave() {
        return !IsSaved;
    }

    /// <summary>
    /// Esegue le operazioni di salvataggio.
    /// </summary>
    [RelayCommand(CanExecute = nameof(canSave))]
    public void Save() {
        IsSaved = true;
        Saved?.Invoke(this, EventArgs.Empty);
    }
}
