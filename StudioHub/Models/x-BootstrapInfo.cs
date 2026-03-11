using System.Text.Json.Serialization;
using StudioHub.Helpers;

namespace StudioHub.Models;

internal class BootstrapInfo {

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
    public string PrimaryDb { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene o imposta il nome del database del gestionale CityUp.
    /// </summary>
    public string LegacyDb { get; set; } = string.Empty;

    /// <summary>
    /// Ottiene la stringa di connessione al database principale dell'applicazione.
    /// La stringa viene generata dinamicamente utilizzando i parametri correnti della configurazione.
    /// </summary>
    [JsonIgnore]
    public string PrimaryConnectionString => DB.BuildConnectionString(this, PrimaryDb);

    /// <summary>
    /// Ottiene la stringa di connessione al database legacy CityUp.
    /// La stringa viene generata dinamicamente utilizzando i parametri correnti della configurazione.
    /// </summary>
    [JsonIgnore]
    public string LegacyConnectionString => DB.BuildConnectionString(this, LegacyDb);

    /// <summary>
    /// Ottiene o imposta il percorso della cartella dati condivisa dell'applicazione
    /// </summary>
    public string DataPath { get; set; } = string.Empty;

}