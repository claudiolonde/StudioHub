namespace StudioHub.Models;

/// <summary>
/// Rappresenta i dati di bootstrap necessari per la configurazione iniziale dell'applicazione.
/// </summary>
public class BootstrapConfig
{
    /// <summary>
    /// Ottiene o imposta il percorso di rete dei dati condivisi.
    /// </summary>
    public string NetworkDataPath { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta il nome o l'indirizzo della sorgente dati (server SQL).
    /// </summary>
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta l'ID utente per l'accesso al database.
    /// </summary>
    public string UserID { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta la password per l'accesso al database.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta un valore che indica se utilizzare l'autenticazione integrata di Windows.
    /// </summary>
    public bool IntegratedSecurity { get; set; } = false;

    /// <summary>
    /// Ottiene o imposta il nome del database principale.
    /// </summary>
    public string PrimaryDB { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta il nome del database legacy.
    /// </summary>
    public string LegacyDB { get; set; } = string.Empty;
}
