using System.IO;
using System.Text.Json;
using static StudioHub.Hub;
namespace StudioHub.Services;

/// <summary>
/// Servizio interno per la gestione della configurazione di avvio e persistenza del percorso dati nel Registro di
/// Windows.
/// </summary>
internal static class BootstrapService {

    /// <summary>
    /// Garantisce che l'intera struttura delle cartelle di rete sia presente.
    /// </summary>
    /// <param name="rootPath">Il percorso radice salvato nel registro.</param>
    public static void ensureDirectoryStructure(string rootPath) {
        if (string.IsNullOrWhiteSpace(rootPath)) {
            return;
        }

        // Definiamo la lista esplicita delle sottocartelle necessarie dalla più superficiale alla più profonda
        List<string> directories = [
            SETTINGS,
            TEMPLATES,
            WORD_TEMPLATES,
            MEETING_WORD_TEMPLATES
        ];

        foreach (string dir in directories) {
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
        }
    }

    /// <summary>
    /// Carica le impostazioni globali dal file JSON. In caso di errore di I/O, effettua fino a 3 tentativi.
    /// </summary>
    public static GlobalSettings loadSettings() {
        string filePath = Path.Combine(SETTINGS, GLOBAL_SETTINGS);
        int maxRetries = 4;
        int delay = 250;

        for (int i = 0; i < maxRetries; i++) {
            try {
                if (!File.Exists(filePath)) {
                    // Se il file non esiste, restituiamo un oggetto con i default
                    return new GlobalSettings();
                }

                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new(fs);
                string json = sr.ReadToEnd();

                GlobalSettings? settings = JsonSerializer.Deserialize<GlobalSettings>(json);
                return settings ?? new GlobalSettings();
            }
            catch (IOException) {
                if (i == maxRetries - 1) {
                    throw; // Se fallisce all'ultimo tentativo, lanciamo l'eccezione
                }
                Thread.Sleep(delay);
            }
        }

        return new GlobalSettings();
    }

    /// <summary>
    /// Opzioni di configurazione per il <see cref="JsonSerializer"/> . Impostate per generare un output JSON formattato con
    /// indentazione per una migliore leggibilità.
    /// </summary>
    private readonly static JsonSerializerOptions jso = new() { WriteIndented = true };

    /// <summary>
    /// Salva le impostazioni globali nel file JSON utilizzando una scrittura atomica e logica di retry.
    /// </summary>
    /// <param name="rootPath">Il percorso radice dei dati (solitamente Hub.DataPath).</param>
    /// <param name="settings">L'oggetto GlobalSettings da serializzare.</param>
    public static void saveSettings(string rootPath, GlobalSettings settings) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(rootPath);

        string folderPath = Path.Combine(rootPath, SETTINGS);
        string filePath = Path.Combine(SETTINGS, GLOBAL_SETTINGS);
        string tempFilePath = filePath + ".tmp";

        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        string jsonContent = JsonSerializer.Serialize(settings, jso);

        int maxRetries = 4;
        int delay = 250;

        for (int i = 0; i < maxRetries; i++) {
            try {
                File.WriteAllText(tempFilePath, jsonContent);
                if (File.Exists(filePath)) {
                    File.Replace(tempFilePath, filePath, null);
                }
                else {
                    File.Move(tempFilePath, filePath);
                }
                return;
            }
            catch (IOException) {

                if (i == maxRetries - 1) {
                    throw; // Gestione errore di I/O (file occupato o rete instabile)
                }
                Thread.Sleep(delay);
            }
            finally {
                if (File.Exists(tempFilePath)) {
                    try { File.Delete(tempFilePath); }
                    catch { }
                }
            }
        }
    }
}
