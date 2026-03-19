namespace StudioHub.Models;

/*
FluentMapper.Entity<User>()
    .Table("[Shared].[Users]")
    .Primary(e => e.Id)
    .Identity(e => e.Id);
 */

/// <summary>
/// Tipologia di avatar che un utente può selezionare nell'applicazione.
/// </summary>
public enum UserAvatar {
    /// <summary>
    /// Nessun avatar selezionato.
    /// </summary>
    None = 0,
    /// <summary>
    /// Avatar: Orso.
    /// </summary>
    Orso = 1,
    /// <summary>
    /// Avatar: Ape.
    /// </summary>
    Ape = 2,
    /// <summary>
    /// Avatar: Gatto.
    /// </summary>
    Gatto = 3,
    /// <summary>
    /// Avatar: Delfino.
    /// </summary>
    Delfino = 4,
    /// <summary>
    /// Avatar: Aquila.
    /// </summary>
    Aquila = 5,
    /// <summary>
    /// Avatar: Volpe.
    /// </summary>
    Volpe = 6,
    /// <summary>
    /// Avatar: Colibrì.
    /// </summary>
    Colibri = 7,
    /// <summary>
    /// Avatar: Leone.
    /// </summary>
    Leone = 8,
    /// <summary>
    /// Avatar: Polipo.
    /// </summary>
    Polipo = 9,
    /// <summary>
    /// Avatar: Gufo.
    /// </summary>
    Gufo = 10,
    /// <summary>
    /// Avatar: Procione.
    /// </summary>
    Procione = 11,
    /// <summary>
    /// Avatar: Salmone.
    /// </summary>
    Salmone = 12
}

/// <summary>
/// Rappresenta un utente dell'applicazione StudioHub.
/// </summary>
/// <remarks>
/// Questa classe contiene i campi principali usati per l'autenticazione, l'autorizzazione e le informazioni di
/// contatto. I campi `Emails` e `Phones` sono conservati come stringhe (formato libero) e possono contenere più valori
/// separati secondo le convenzioni dell'applicazione.
/// </remarks>
public record User {
    /// <summary>
    /// Identificativo univoco dell'utente (chiave primaria).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome utente usato per l'accesso (login).
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Nome proprio dell'utente.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Cognome dell'utente.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Hash della password dell'utente.
    /// </summary>
    /// <remarks>
    /// Non memorizzare mai la password in chiaro. Qui deve essere conservato il valore hash prodotto da un algoritmo
    /// sicuro (es. PBKDF2, BCrypt, Argon2) secondo le policy dell'app.
    /// </remarks>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Indica se l'utente ha privilegi di amministratore.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Tipo di avatar selezionato dall'utente.
    /// </summary>
    public UserAvatar AvatarType { get; set; }

    /// <summary>
    /// Indirizzi email associati all'utente (stringa, formato libero).
    /// </summary>
    /// <remarks>
    /// Potrebbe contenere più email separate da virgola o altro separatore applicativo.
    /// </remarks>
    public string Emails { get; set; }

    /// <summary>
    /// Numeri di telefono associati all'utente (stringa, formato libero).
    /// </summary>
    /// <remarks>
    /// Potrebbe contenere più numeri separati da virgola o altro separatore applicativo.
    /// </remarks>
    public string Phones { get; set; }

    /// <summary>
    /// Codice temporaneo per il reset della password, se presente.
    /// </summary>
    /// <remarks>
    /// Valore nullable: <see langword="null" /> quando non è stato richiesto un reset.
    /// </remarks>
    public string? ResetCode { get; set; }

    /// <summary>
    /// Data/ora di scadenza del codice di reset della password.
    /// </summary>
    /// <remarks>
    /// Valore nullable: <see langword="null" /> quando non è stato impostato alcun reset. Si consiglia di memorizzare
    /// date in formato UTC e validarle al momento dell'uso.
    /// </remarks>
    public DateTime? ResetExpiry { get; set; }

    /// <summary>
    /// Inizializza una nuova istanza della classe <see cref="User"/>.
    /// </summary>
    /// <remarks>
    /// Il costruttore imposta le stringhe su valori vuoti e seleziona l'avatar predefinito `<see cref="UserAvatar.None"
    /// />` per evitare valori <see langword="null" /> nelle stringhe.
    /// </remarks>
    public User() {
        Id = -1;
        Username = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        PasswordHash = string.Empty;
        Emails = string.Empty;
        Phones = string.Empty;
        AvatarType = UserAvatar.None;
    }
}
