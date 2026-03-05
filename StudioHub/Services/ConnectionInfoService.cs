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
    /// Flag di stato privato per tracciare l'avvenuta inizializzazione del modulo.
    /// </summary>
    private static bool _isInitialized = false;

    /// <summary>
    /// Nome del database primario estratto dalla configurazione.
    /// </summary>
    public static string PrimaryDB { get; private set; } = string.Empty;

    /// <summary>
    /// Nome del database legacy estratto dalla configurazione.
    /// </summary>
    public static string LegacyDB { get; private set; } = string.Empty;

    /// <summary>
    /// Stringa di connessione base (senza Initial Catalog) validata dal sistema.
    /// </summary>
    public static string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Ottiene la Connection String completa puntando al database primario.
    /// </summary>
    public static string PrimaryConnectionString => $"{ConnectionString};Initial Catalog=" + PrimaryDB;

    /// <summary>
    /// Ottiene la Connection String completa puntando al database legacy.
    /// </summary>
    public static string LegacyConnectionString => $"{ConnectionString};Initial Catalog=" + LegacyDB;

    /// <summary>
    /// Inizializza la configurazione globale, valida la connessione al database e mappa le entità.
    /// </summary>
    /// <returns>
    /// True se l'inizializzazione ha successo; False se l'utente annulla l'operazione di configurazione.
    /// </returns>
    public static bool Initialize() {

        // Verifica se il modulo è già stato inizializzato per evitare riesecuzioni
        if (_isInitialized) {
            return true;
        }

        // Configurazione globale del provider SQL Server
        GlobalConfiguration.Setup().UseSqlServer();

        // Loop di validazione delle credenziali di connessione
        while (true) {
            ConnectionInfo? ci = LoadConnectionInfo();
            if (testConnection(ci)) {
                // Assegnazione dei nomi database post-validazione
                PrimaryDB = ci!.PrimaryDd;
                LegacyDB = ci!.LegacyDb;
                break;
            }
            // Se la connessione fallisce, apre la UI di editing; se l'utente chiude, interrompe il boot
            if (!EditConnectionInfoView.Open()) {
                return false;
            }
        }

        // Esegue il mapping delle entità tramite Fluent API
        mappingEntities();
        _isInitialized = true;
        return true;
    }

    /// <summary>
    /// Tenta di stabilire una connessione di prova verso il server specificato.
    /// </summary>
    /// <param name="ci">Oggetto contenente i parametri di connessione.</param>
    /// <returns>True se la connessione viene aperta con successo, altrimenti False.</returns>
    private static bool testConnection(ConnectionInfo? ci) {

        if (ci is null) {
            return false;
        }

        try {
            // Genera la stringa e tenta l'apertura tramite SqlConnection
            ConnectionString = buildConnectionString(ci);
            using SqlConnection connection = new(ConnectionString);
            connection.Open();
            return true;
        }
        catch {
            // Ritorna false in caso di eccezione durante l'handshake o il login
            return false;
        }
    }

    /// <summary>
    /// Costruisce la stringa di connessione utilizzando <see cref="SqlConnectionStringBuilder"/> .
    /// </summary>
    /// <param name="ci">Dati di input per la sorgente dati e autenticazione.</param>
    /// <returns>Stringa di connessione formattata.</returns>
    private static string buildConnectionString(ConnectionInfo ci) {
        SqlConnectionStringBuilder builder = new() {
            DataSource = ci.DataSource,
            IntegratedSecurity = ci.IntegratedSecurity,
            TrustServerCertificate = true, // Necessario per connessioni locali/auto-firmate
            ConnectTimeout = 15
        };
        // Gestione autenticazione SQL se IntegratedSecurity è disabilitato
        if (!ci.IntegratedSecurity) {
            builder.UserID = ci.UserID;
            builder.Password = ci.Password;
        }
        return builder.ConnectionString;
    }

    /// <summary>
    /// Registra le configurazioni di mapping tra classi POCO e tabelle del database.
    /// </summary>
    private static void mappingEntities() {
        /*
        FluentMapper.Entity<ModelName>()
            .Table("Schema.TableName")
            .Primary(x => x.Id)
            .Identity(x => x.Id);
        */

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
        using RegistryKey? rk = Registry.CurrentUser.OpenSubKey("");
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