namespace Studio.Models.TaskPlanner;

/// <summary>
/// Rappresenta un fornitore nel contesto del Task Planner.
/// </summary>
/// <remarks>
/// Record immutabile usato per trasferire le informazioni di un fornitore.
/// </remarks>
public record Supplier {

    /// <summary>
    /// Costruttore predefinito necessario per serializzazione e binding (es. WPF).
    /// </summary>
    public Supplier() {
    }

    /// <summary>
    /// Costruisce una nuova istanza di <see cref="Supplier"/> con i valori principali.
    /// </summary>
    /// <param name="id">Identificatore del fornitore.</param>
    /// <param name="name">Nome del fornitore. Non può essere <see langword="null"/> o whitespace.</param>
    /// <exception cref="ArgumentException">
    /// Viene sollevata se <paramref name="name"/> è <see langword="null"/> oppure contiene solo spazi bianchi.
    /// </exception>
    public Supplier(int id, string name) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Identificatore numerico del fornitore.
    /// </summary>
    /// <remarks>
    /// Valore predefinito: -1 (indica che l'Id non è stato assegnato).
    /// </remarks>
    public int Id { get; init; } = -1;

    /// <summary>
    /// Nome del fornitore.
    /// </summary>
    /// <remarks>
    /// Non può essere <see langword="null"/> o una stringa vuota quando viene utilizzato il costruttore parametrizzato.
    /// </remarks>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Indirizzo email del fornitore.
    /// </summary>
    /// <remarks>
    /// Campo opzionale; può rimanere stringa vuota se non disponibile.
    /// </remarks>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Numero di telefono del fornitore.
    /// </summary>
    /// <remarks>
    /// Campo opzionale; formato non validato da questo modello.
    /// </remarks>
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Note libere relative al fornitore.
    /// </summary>
    /// <remarks>
    /// Campo opzionale per informazioni aggiuntive (es. condizioni, riferimenti, commenti).
    /// </remarks>
    public string Notes { get; set; } = string.Empty;
}
