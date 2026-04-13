namespace Studio.Models;

/// <summary>
/// Rappresenta un template Word modificabile utilizzato dall'app.
/// </summary>
public record WordTemplate {
    /// <summary>Identificativo univoco.</summary>
    public int Id { get; init; } = -1;

    /// <summary>Nome del template.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Descrizione del template.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Contenuto binario del template.</summary>
    public byte[] Content { get; init; } = [];

    /// <summary>Applicazione target per questo template.</summary>
    public string TargetApp { get; init; } = string.Empty;

    /// <summary>Data di creazione.</summary>
    public DateTime Created { get; init; }

    /// <summary>Data ultima modifica.</summary>
    public DateTime Modified { get; set; }

    /// <summary>Indica se il template è bloccato per la modifica.</summary>
    public bool Locked { get; set; }

    /// <summary>
    /// Costruttore di default: inizializza le date con un unico accesso a <see cref="DateTime.UtcNow"/>.
    /// </summary>
    public WordTemplate() {
        DateTime now = DateTime.UtcNow;
        Created = now;
        Modified = now;
    }

    /// <summary>
    /// Costruttore con parametri completi. Valida i parametri stringa e array prima dell'assegnazione.
    /// </summary>
    public WordTemplate(int id, string name, string description, string targetApp, DateTime created, DateTime modified, bool locked) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetApp);

        Id = id;
        Name = name;
        Description = description;
        Content = [];
        TargetApp = targetApp;
        Created = created;
        Modified = modified;
        Locked = locked;
    }
}
