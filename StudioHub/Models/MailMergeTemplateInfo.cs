namespace StudioHub.Models;

/// <summary>
/// Rappresenta i metadati di un modello di Word per la stampa unione.
/// </summary>
public record MailMergeTemplateInfo {

    /// <summary>
    /// Identificativo univoco del modello.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Nome dell'applicazione associata al modello.
    /// </summary>
    public string App { get; init; } = string.Empty;

    /// <summary>
    /// Nome del modello.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrizione del modello.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Dimensione in byte del modello.
    /// </summary>
    public int Size { get; set; } = 0;

    /// <summary>
    /// Data dell'ultima modifica del modello.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.Now;

}