using System.Text.Json;
using System.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RepoDb;
using StudioHub.Models;
using StudioHub.Views;

namespace StudioHub.Services;

/// <summary>
/// Fornisce servizi per la gestione della connessione SQL, inclusi caricamento, salvataggio e test della
/// configurazione.
/// </summary>
public static class ConnectionInfoService {

    /// <summary>
    /// Flag che indica se il servizio è stato inizializzato.
    /// </summary>
    private static bool _isInitialized = false;

    public static string PrimaryDB { get; private set; } = string.Empty;
    public static string LegacyDB { get; private set; } = string.Empty;

    public static string ConnectionString { get; private set; } = string.Empty;

    public static string PrimaryConnectionString => $"{ConnectionString};Initial Catalog=" + PrimaryDB;
    public static string LegacyConnectionString => $"{ConnectionString};Initial Catalog=" + LegacyDB;


    /// <summary>
    /// Inizializza la configurazione globale per l'accesso a SQL Server. Esegue il setup di RepoDb e la mappatura delle
    /// entità tramite FluentMapper.
    /// </summary>
    /// <remarks>
    /// Questo metodo deve essere chiamato una sola volta durante il ciclo di vita dell'applicazione.
    /// </remarks>
    public static bool Initialize() {

        if (_isInitialized) {
            return true;
        }

        GlobalConfiguration.Setup().UseSqlServer();

        while (true) {
            ConnectionInfo? ci = LoadConnectionInfo();
            if (testConnection(ci)) {
                PrimaryDB = ci!.PrimaryDB;
                LegacyDB = ci!.LegacyDB;
                break;
            }
            if (!EditConnectionInfoView.Open()) {
                return false;
            }
        }

        mappingEntities();
        _isInitialized = true;
        return true;
    }

    private static bool testConnection(ConnectionInfo? ci) {

        if (ci is null) {
            return false;
        }

        try {
            ConnectionString = buildConnectionString(ci);
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            return true;
        }
        catch {
            return false;
        }
    }

    private static string buildConnectionString(ConnectionInfo ci) {
        SqlConnectionStringBuilder builder = new() {
            DataSource = ci.DataSource,
            IntegratedSecurity = ci.IntegratedSecurity,
            TrustServerCertificate = true,
            ConnectTimeout = 15
        };
        if (!ci.IntegratedSecurity) {
            builder.UserID = ci.UserID;
            builder.Password = ci.Password;
        }
        return builder.ConnectionString;
    }

    private static void mappingEntities() {
        /*
        FluentMapper.Entity<ModelName>()
            .Table("Schema.TableName")
            .Primary(x => x.Id)
            .Identity(x => x.Id);
        */
        FluentMapper.Entity<MailMergeTemplateData>()
            .Table("Shared.MailMergeTemplateData")
            .Primary(x => x.Id);
        FluentMapper.Entity<MailMergeTemplateInfo>()
            .Table("Shared.MailMergeTemplateInfo")
            .Primary(x => x.Id)
            .Identity(x => x.Id);
    }

    /// <summary>
    /// Salva la configurazione della connessione dati nel registro di sistema di Windows.
    /// </summary>
    /// <param name="ci">L'oggetto <see cref="ConnectionInfo"/> da salvare.</param>
    /// <remarks>
    /// Questo metodo è supportato solo su piattaforme Windows.
    /// </remarks>
    public static void SaveConnectionInfo(ConnectionInfo ci) {
        string json = JsonSerializer.Serialize(ci);
        using RegistryKey rk = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH);
        rk.SetValue("ConnectionInfo", json);
    }

    /// <summary>
    /// Carica la configurazione della connessione dati dal registro di sistema di Windows.
    /// </summary>
    /// <remarks>
    /// Questo metodo è supportato solo su piattaforme Windows.
    /// </remarks>
    /// <returns>
    /// Un oggetto <see cref="ConnectionInfo"/> con i dati caricati dal registro, oppure un nuovo oggetto
    /// <see cref="ConnectionInfo"/> se la chiave o il valore non esistono o la deserializzazione fallisce.
    /// </returns>
    public static ConnectionInfo? LoadConnectionInfo() {
        using RegistryKey? rk = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH);
        if (rk == null) {
            return null;// new ConnectionInfo();
        }

        string? json = rk.GetValue("ConnectionInfo") as string;
        if (string.IsNullOrWhiteSpace(json)) {
            return null;//return new ConnectionInfo();
        }

        try {
            return JsonSerializer.Deserialize<ConnectionInfo>(json) ?? new ConnectionInfo();
        }
        catch (JsonException) {
            return null;//return new ConnectionInfo();
        }
    }

    /// <summary>
    /// Recupera l'elenco dei nomi dei database disponibili sul server specificato nella configurazione di connessione.
    /// </summary>
    /// <param name="ci">Oggetto <see cref="ConnectionInfo"/> contenente i dettagli della connessione.</param>
    /// <param name="token">Token di cancellazione per annullare l'operazione asincrona.</param>
    /// <returns>
    /// Una lista di stringhe contenente i nomi dei database (escludendo i database di sistema con ID <= 4).
    /// </returns>
    /// <exception cref="SqlException">Sollevata se la connessione al server SQL fallisce.</exception>
    public static async Task<string[]> GetDatabaseNamesAsync(ConnectionInfo ci, CancellationToken token) {

        string query = @"
SELECT   name
FROM     sys.databases
WHERE    database_id > 4
ORDER BY name;
";
        SqlConnectionStringBuilder b = new() {
            DataSource = ci.DataSource,
            InitialCatalog = "master",
            IntegratedSecurity = ci.IntegratedSecurity,
            TrustServerCertificate = true
        };
        if (!ci.IntegratedSecurity) {
            b.UserID = ci.UserID;
            b.Password = ci.Password;
        }

        using SqlConnection connection = new(b.ConnectionString);
        IEnumerable<string> result = await connection.ExecuteQueryAsync<string>(query, cancellationToken: token);
        return [.. result];
    }
}