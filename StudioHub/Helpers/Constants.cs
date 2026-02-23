using System.IO;

namespace StudioHub.Helpers;

public static class Constants {

    /// <summary>
    /// Percorso della cartella dati dell'applicazione.
    /// </summary>
    public static readonly string APP_DATA_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Studio Londe", "StudioHub");

    /// <summary>
    /// Percorso della chiave di registro utilizzata dall'applicazione per memorizzare le impostazioni.
    /// </summary>
    public static readonly string REGISTRY_PATH = @"Software\Studio Londe\StudioHub";

    /// <summary>
    /// Percorso della cartella temporanea utilizzata dall'applicazione.
    /// </summary>
    public static readonly string TEMP_PATH = Path.Combine(Path.GetTempPath(), "StudioHub");

}