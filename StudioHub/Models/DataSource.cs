namespace StudioHub.Models;

/// <summary>
/// Rappresenta una sorgente dati SQL Server, inclusi parametri di connessione e autenticazione.
/// </summary>
internal class DataSource {

    /// <summary>
    /// Nome o indirizzo del server SQL.
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Nome dell'istanza SQL Server (utilizzato se la porta non è specificata).
    /// </summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// Porta TCP del server SQL. Se <c>null</c> o 0, viene usata l'istanza.
    /// </summary>
    public ushort? Port { get; set; } = null;

    /// <summary>
    /// Indica se utilizzare l'autenticazione integrata di Windows.
    /// </summary>
    public bool IntegratedSecurity { get; set; } = false;

    /// <summary>
    /// Nome utente per l'autenticazione SQL Server.
    /// </summary>
    public string UserID { get; set; } = string.Empty;

    /// <summary>
    /// Password per l'autenticazione SQL Server.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Nome del database principale da utilizzare nella connessione.
    /// </summary>
    public string StudioHubDB { get; set; } = string.Empty;

    /// <summary>
    /// Nome del database legacy da utilizzare nella connessione.
    /// </summary>
    public string CityUpDB { get; set; } = string.Empty;

    /// <summary>
    /// Genera la stringa di connessione SQL Server in base ai parametri specificati.
    /// </summary>
    /// <param name="ds">Oggetto <see cref="DataSource"/> con i parametri di connessione.</param>
    /// <param name="legacy">
    /// Se <see langword="true" /> , utilizza <see cref="CityUpDB"/> come database iniziale; altrimenti utilizza
    /// <see cref="StudioHubDB"/> .
    /// </param>
    /// <returns>Stringa di connessione SQL Server formattata.</returns>
    /// <exception cref="ArgumentNullException">
    /// Se uno dei parametri obbligatori (Server, Instance, UserID, Password) non è valorizzato.
    /// </exception>
    public static string GetConnectionString(DataSource ds, bool legacy = false) {

        ArgumentNullException.ThrowIfNullOrWhiteSpace(ds.Server);

        string cs = $"Trust Server Certificate=true; Connect Timeout=15; Data Source={ds.Server}";
        if (ds.Port.HasValue && ds.Port.Value > 0) {
            cs += $",{ds.Port.Value}; ";
        }
        else if (!string.IsNullOrWhiteSpace(ds.Instance)) {
            cs += @$"\{ds.Instance}; ";
        }
        else {
            cs += "; ";
        }

        if (ds.IntegratedSecurity) {
            cs += "Integrated Security=true; ";
        }
        else {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(ds.UserID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(ds.Password);
            cs += $"User ID={ds.UserID}; Password={ds.Password}; ";
        }

        if (legacy) {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(ds.CityUpDB);
            cs += $"Initial Catalog={ds.CityUpDB};";
        }
        else {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(ds.StudioHubDB);
            cs += $"Initial Catalog={ds.StudioHubDB};";
        }

        return cs;
    }
}
