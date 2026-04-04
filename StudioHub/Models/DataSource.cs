namespace StudioHub.Models;

/// <summary>
/// Rappresenta le informazioni necessarie per creare stringhe di connessione verso SQL Server. Contiene server,
/// istanza/porta, credenziali e i nomi dei database usati dall'applicazione.
/// </summary>
public class DataSource {

    /// <summary>
    /// Nome o indirizzo del server SQL.
    /// </summary>
    public string Server { get; internal set; } = string.Empty;

    /// <summary>
    /// Nome dell'istanza SQL (se utilizzata). Se valorizzata e <see cref="Port"/> non presente, verrà utilizzata nel
    /// Data Source come `Server\Instance`.
    /// </summary>
    public string Instance { get; internal set; } = string.Empty;

    /// <summary>
    /// Porta TCP del server SQL. Se presente e maggiore di zero verrà usata come `Server,Port`.
    /// </summary>
    public ushort? Port { get; internal set; } = null;

    /// <summary>
    /// Indica se utilizzare l'autenticazione integrata di Windows. Se impostato a <see langword="true" />, UserID e
    /// Password non sono richiesti.
    /// </summary>
    public bool IntegratedSecurity { get; internal set; } = false;

    /// <summary>
    /// User ID per l'autenticazione SQL. Deve essere valorizzato se <see cref="IntegratedSecurity"/> è
    /// <see langword="false" />.
    /// </summary>
    public string UserID { get; internal set; } = string.Empty;

    /// <summary>
    /// Password per l'autenticazione SQL. Deve essere valorizzata se <see cref="IntegratedSecurity"/> è
    /// <see langword="false" />.
    /// </summary>
    public string Password { get; internal set; } = string.Empty;

    /// <summary>
    /// Nomi dei database usati dall'applicazione e dal gestionale esterno CityUp. <br/> - <see cref="Database"/>
    /// .StudioHub => database dell'applicazione <br/> - <see cref="Database"/> .CityUp => database del gestionale
    /// CityUp
    /// </summary>
    public (string StudioHub, string CityUp) Database { get; internal set; } = (string.Empty, string.Empty);

    /// <summary>
    /// Costruisce la parte comune della stringa di connessione (senza il valore di <c> Initial Catalog</c>).
    /// </summary>
    /// <remarks>
    /// - Aggiunge <c> Data Source</c> usando <see cref="Server"/> e, a seconda dei valori, la <see cref="Port"/> oppure
    /// la <see cref="Instance"/>. Se nessuna di queste è presente, viene lasciato il formato base. - Se
    /// <see cref="IntegratedSecurity"/> è <see langword="true" /> viene aggiunto <c> Integrated Security=true</c>. -
    /// Altrimenti verifica che <see cref="UserID"/> e <see cref="Password"/> non siano <see langword="null" /> o vuote;
    /// in caso contrario viene sollevata un'eccezione tramite
    /// <see cref="System.ArgumentNullException.ThrowIfNullOrWhiteSpace(string?, string?)"/>.
    /// </remarks>
    /// <returns>
    /// Una stringa contenente le impostazioni comuni della connessione, inclusi timeout e trust certificate.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Viene sollevata quando <see cref="IntegratedSecurity"/> è <see langword="false" /> e <see cref="UserID"/> o
    /// <see cref="Password"/> sono <see langword="null" /> o vuote.
    /// </exception>
    private string getConnectionString() {

        ArgumentNullException.ThrowIfNullOrWhiteSpace(Server);
        string cs = $"Trust Server Certificate=true; Connect Timeout=15; Data Source={Server}";

        if (Port.HasValue && Port.Value > 0) {
            cs += $",{Port.Value}; ";
        }
        else if (!string.IsNullOrWhiteSpace(Instance)) {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Instance);
            cs += @$"\{Instance}; ";
        }
        else {
            cs += "; ";
        }

        if (IntegratedSecurity) {
            cs += "Integrated Security=true; ";
        }
        else {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(UserID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Password);
            cs += $"User ID={UserID}; Password={Password}; ";
        }

        return cs;
    }

    /// <summary>
    /// Ottiene una stringa di connessione verso il database di sistema <c> master</c>.
    /// </summary>
    /// <returns>Stringa di connessione completa con <c>Initial Catalog=master</c>.</returns>
    public string ConnectionString => getConnectionString() + " Initial Catalog=master;";

    /// <summary>
    /// Ottiene la stringa di connessione verso il database dell'applicazione StudioHub.
    /// </summary>
    /// <returns>
    /// Stringa di connessione completa con <c> Initial Catalog</c> impostato su <see cref="Database"/> .StudioHub.
    /// </returns>
    public string StudioHubConnectionString {
        get {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Database.StudioHub);
            return getConnectionString() + $" Initial Catalog={Database.StudioHub};";
        }
    }

    /// <summary>
    /// Ottiene la stringa di connessione verso il database del gestionale CityUp.
    /// </summary>
    /// <returns>
    /// Stringa di connessione completa con <c> Initial Catalog</c> impostato su <see cref="Database"/> .CityUp.
    /// </returns>
    public string CityUpConnectionString {
        get {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Database.CityUp);
            return getConnectionString() + $" Initial Catalog={Database.CityUp};";
        }
    }
}
