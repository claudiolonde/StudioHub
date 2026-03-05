namespace StudioHub.Models;

/// <summary>
/// Rappresenta le credenziali e i parametri di connessione per un data source.
/// </summary>
internal record DataSourceCredentials {

    /// <summary>
    /// Ottiene o imposta l'indirizzo o il nome del server.
    /// </summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta l'identificativo utente per l'autenticazione.
    /// </summary>
    public string UserID { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta la password associata all'utente per l'autenticazione.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta un valore che indica se utilizzare l'autenticazione integrata di Windows (SSPI). Se impostato
    /// su <see langword="true"/> , <see cref="UserID"/> e <see cref="Password"/> vengono ignorati.
    /// </summary>
    public bool IntegratedSecurity { get; set; } = false;

    /// <summary>
    /// Restituisce true se mancano i dati fondamentali per tentare una connessione.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(DataSource) ||
                          !IntegratedSecurity && (string.IsNullOrWhiteSpace(UserID) || string.IsNullOrWhiteSpace(Password));
}