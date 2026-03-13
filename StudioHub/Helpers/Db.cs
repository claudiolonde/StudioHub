using Microsoft.Data.SqlClient;
using RepoDb;
using StudioHub.Models;

namespace StudioHub.Helpers;

/// <summary>
/// Classe helper per la gestione delle operazioni di database.
/// </summary>
internal static class DB {

    /// <summary>
    /// Recupera l'elenco dei nomi dei database disponibili sul server specificato nella configurazione di connessione.
    /// </summary>
    /// <param name="ci">Oggetto <see cref="ConnectionInfo"/> contenente i dettagli della connessione.</param>
    /// <param name="token">Token di cancellazione per annullare l'operazione asincrona.</param>
    /// <returns>
    /// Una lista di stringhe contenente i nomi dei database (escludendo i database di sistema con ID <= 4).
    /// </returns>
    /// <exception cref="SqlException">Sollevata se la connessione al server SQL fallisce.</exception>
    internal static async Task<string[]> GetDatabaseNamesAsync(StudioHub.Models.DataSource ds, CancellationToken token) {

        string query = @"
SELECT   name
FROM     sys.databases
WHERE    database_id > 4
ORDER BY name;
";
        SqlConnectionStringBuilder b = new() {
            DataSource = ds.Server,
            InitialCatalog = "master",
            IntegratedSecurity = ds.IntegratedSecurity,
            TrustServerCertificate = true
        };
        if (!ds.IntegratedSecurity) {
            b.UserID = ds.UserID;
            b.Password = ds.Password;
        }

        using SqlConnection connection = new(b.ConnectionString);
        IEnumerable<string> result = await connection.ExecuteQueryAsync<string>(query, cancellationToken: token);
        return [.. result];
    }

}
