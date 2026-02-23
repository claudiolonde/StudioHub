using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudioHub.Controls;
using StudioHub.Models;
using StudioHub.Services;

namespace StudioHub.ViewModels;

/// <summary>
/// ViewModel per la modifica delle informazioni di connessione al database. Gestisce la logica di connessione,
/// disconnessione, salvataggio e caricamento delle configurazioni.
/// </summary>
public partial class EditConnectionInfoViewModel : ObservableObject {

    /// <summary>
    /// Lanciato quando la configurazione di connessione viene salvata con successo.
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Configurazione di connessione salvata correntemente.
    /// </summary>
    private readonly ConnectionInfo? savedCI;

    /// <summary>
    /// Indica se è in corso un'operazione asincrona.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditServerSettings))]
    [NotifyPropertyChangedFor(nameof(CanEditCredentials))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private bool _isBusy;

    /// <summary>
    /// Nome o indirizzo del server di database.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string? _dataSource;

    /// <summary>
    /// Indica se viene utilizzata l'autenticazione integrata di Windows.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotIntegratedSecurity))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyPropertyChangedFor(nameof(CanEditCredentials))]
    private bool _integratedSecurity;

    /// <summary>
    /// Restituisce true se non viene utilizzata l'autenticazione integrata.
    /// </summary>
    public bool NotIntegratedSecurity => !IntegratedSecurity;

    /// <summary>
    /// Restituisce true se è possibile modificare le impostazioni del server.
    /// </summary>
    public bool CanEditServerSettings => NotIsConnected && !IsBusy;

    /// <summary>
    /// Nome utente per l'autenticazione SQL.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string? _userId;

    /// <summary>
    /// Password per l'autenticazione SQL.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string? _password;

    /// <summary>
    /// Database StudioHub selezionato.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _studioHubDBSelection;

    /// <summary>
    /// Database CityUp selezionato.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _cityUpDBSelection;

    /// <summary>
    /// Indica se la connessione al database è attiva.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotIsConnected))]
    [NotifyPropertyChangedFor(nameof(CanEditCredentials))]
    [NotifyPropertyChangedFor(nameof(CanEditServerSettings))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isConnected = false;

    /// <summary>
    /// Restituisce true se non si è connessi al database.
    /// </summary>
    public bool NotIsConnected => !IsConnected;

    /// <summary>
    /// Restituisce true se è possibile modificare le credenziali di accesso.
    /// </summary>
    public bool CanEditCredentials => NotIntegratedSecurity && NotIsConnected && !IsBusy;

    /// <summary>
    /// Elenco dei nomi dei database disponibili.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _databaseNames = [];

    /// <summary>
    /// Costruttore. Carica la configurazione salvata all'avvio.
    /// </summary>
    public EditConnectionInfoViewModel() {

        savedCI = ConnectionInfoService.LoadConnectionInfo();
        if (savedCI is null) {
            return;
        }

        DataSource = savedCI.DataSource;
        IntegratedSecurity = savedCI.IntegratedSecurity;
        UserId = savedCI.UserID;
        Password = savedCI.Password;

        StudioHubDBSelection = savedCI.PrimaryDB;
        CityUpDBSelection = savedCI.LegacyDB;
        DatabaseNames = [StudioHubDBSelection!, CityUpDBSelection!];
    }

    /// <summary>
    /// Costruisce un oggetto <see cref="ConnectionInfo"/> a partire dalle proprietà correnti.
    /// </summary>
    /// <returns>Oggetto <see cref="ConnectionInfo"/> popolato.</returns>
    private ConnectionInfo getConnectionInfo() {
        return new() {
            DataSource = DataSource ?? string.Empty,
            IntegratedSecurity = IntegratedSecurity,
            UserID = UserId ?? string.Empty,
            Password = Password ?? string.Empty,
            PrimaryDB = StudioHubDBSelection ?? string.Empty,
            LegacyDB = CityUpDBSelection ?? string.Empty
        };
    }

    /// <summary>
    /// Determina se è possibile eseguire il comando di connessione.
    /// </summary>
    /// <returns>True se la connessione è consentita, altrimenti false.</returns>
    private bool CanConnect() {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(DataSource)
            && (IntegratedSecurity
            || !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(Password));
    }

    /// <summary>
    /// Esegue la connessione al server e recupera l'elenco dei database disponibili.
    /// </summary>
    /// <param name="token">Token di cancellazione per l'operazione asincrona.</param>
    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync(CancellationToken token) {

        IsConnected = false;
        IsBusy = true;

        try {
            ConnectionInfo ci = getConnectionInfo();
            string[] names = await ConnectionInfoService.GetDatabaseNamesAsync(ci, token);

            IsConnected = true;

            DatabaseNames.Clear();
            foreach (string s in names) {
                DatabaseNames.Add(s);
            }
            StudioHubDBSelection = DatabaseNames.Contains(ci.PrimaryDB) ? ci.PrimaryDB : null;
            CityUpDBSelection = DatabaseNames.Contains(ci.LegacyDB) ? ci.LegacyDB : null;

        }
        catch (OperationCanceledException) { }
        catch (Exception ex) {
            Dialog.Show($"Errore di connessione: {ex.Message}", DialogIcon.Error);
            IsConnected = false;

        }
        finally {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Disconnette dal server e resetta lo stato di connessione.
    /// </summary>
    [RelayCommand]
    private void Disconnect() {
        IsConnected = false;
    }

    /// <summary>
    /// Determina se è possibile eseguire il comando di salvataggio.
    /// </summary>
    /// <returns>True se il salvataggio è consentito, altrimenti false.</returns>
    private bool CanSave() {
        return IsConnected
            && !string.IsNullOrEmpty(StudioHubDBSelection)
            && !string.IsNullOrEmpty(CityUpDBSelection)
            && StudioHubDBSelection != CityUpDBSelection
            && savedCI != getConnectionInfo();
    }

    /// <summary>
    /// Salva la configurazione di connessione corrente.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save() {
        ConnectionInfoService.SaveConnectionInfo(getConnectionInfo());
        SaveCommand.NotifyCanExecuteChanged();
        IsSaved = true;
        Saved?.Invoke(this, EventArgs.Empty);
    }

    internal bool IsSaved { get; private set; } = false;

}