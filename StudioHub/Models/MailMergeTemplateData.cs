namespace StudioHub.Models;

public record MailMergeTemplateData {

    /// <summary>
    /// Identificativo univoco del modello.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Contenuto del file .docx del modello in formato binario.
    /// </summary>
    public byte[] FileContent { get; set; } = [];

    /// <summary>
    /// Intestazioni separate da TAB utilizzate nel modello.
    /// </summary>
    public string Headers { get; init; } = string.Empty;

}