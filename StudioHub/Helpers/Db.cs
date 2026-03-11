using Microsoft.Data.SqlClient;
using RepoDb;
using StudioHub.Models;

namespace StudioHub.Helpers;

/// <summary>
/// Classe helper per la gestione delle operazioni di database.
/// </summary>
internal static class DB {

    /// <summary>
    /// Costruisce una stringa di connessione SQL Server a partire dalle informazioni di bootstrap fornite.
    /// </summary>
    /// <param name="info">Oggetto <see cref="BootstrapInfo"/> contenente i parametri di connessione.</param>
    /// <param name="database">Nome opzionale del database da selezionare come Initial Catalog.</param>
    /// <returns>Stringa di connessione SQL Server completa.</returns>
    /// <exception cref="ArgumentNullException">Sollevata se <paramref name="info"/> è null.</exception>
    internal static string BuildConnectionString(BootstrapInfo info, string? database = null) {
        ArgumentNullException.ThrowIfNull(info);

        SqlConnectionStringBuilder cs = new() {
            DataSource = info.DataSource,
            IntegratedSecurity = info.IntegratedSecurity,
            TrustServerCertificate = true,
            ConnectTimeout = 15
        };
        if (!info.IntegratedSecurity) {
            cs.UserID = info.UserID;
            cs.Password = info.Password;
        }
        if (!string.IsNullOrWhiteSpace(database)) {
            cs.InitialCatalog = database;
        }
        return cs.ConnectionString;
    }

    /// <summary>
    /// Recupera l'elenco dei nomi dei database disponibili sul server specificato nella configurazione di
    /// connessione.
    /// </summary>
    /// <param name="ci">Oggetto <see cref="ConnectionInfo"/> contenente i dettagli della connessione.</param>
    /// <param name="token">Token di cancellazione per annullare l'operazione asincrona.</param>
    /// <returns>
    /// Una lista di stringhe contenente i nomi dei database (escludendo i database di sistema con ID <= 4).
    /// </returns>
    /// <exception cref="SqlException">Sollevata se la connessione al server SQL fallisce.</exception>
    internal static async Task<string[]> GetDatabaseNamesAsync(ConnectionInfo ci, CancellationToken token) {

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
