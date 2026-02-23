namespace StudioHub.Models;

/// <summary>   
/// Rappresenta le informazioni di connessione a una sorgente dati.
/// </summary>
public record ConnectionInfo {

    /// <summary>
    /// Ottiene o imposta il nome server, nome istanza o l'indirizzo IP della sorgente dati.
    /// </summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta l'identificativo dell'utente per l'autenticazione di SQL Server.
    /// </summary>
    public string UserID { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta la password dell'utente per l'autenticazione di SQL Server.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta un valore che indica se utilizzare la sicurezza integrata di Windows.
    /// </summary>
    public bool IntegratedSecurity { get; set; } = false;

    /// <summary>
    /// Ottiene o imposta il nome del database dell'applicazione.
    /// </summary>
    public string PrimaryDB { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta il nome del database del gestionale CityUp.
    /// </summary>
    public string LegacyDB { get; set; } = string.Empty;

}