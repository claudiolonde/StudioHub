using Microsoft.Data.SqlClient;
using StudioHub.Models;

namespace StudioHub.Helpers;

/// <summary>
/// Classe helper per la gestione delle operazioni di database.
/// </summary>
internal static class Db {

    /// <summary>
    /// Costruisce una stringa di connessione SQL Server a partire dalle informazioni di bootstrap fornite.
    /// </summary>
    /// <param name="info">Oggetto <see cref="BootstrapInfo"/> contenente i parametri di connessione.</param>
    /// <param name="database">Nome opzionale del database da selezionare come Initial Catalog.</param>
    /// <returns>Stringa di connessione SQL Server completa.</returns>
    /// <exception cref="ArgumentNullException">Sollevata se <paramref name="info"/> è null.</exception>
    public static string BuildConnectionString(BootstrapInfo info, string? database = null) {
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

}