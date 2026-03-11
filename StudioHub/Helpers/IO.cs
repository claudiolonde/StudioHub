using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using StudioHub.Models;
namespace StudioHub.Helpers;

public class IO {




    /// <summary>
    /// Percorso della chiave di registro utilizzata per memorizzare le impostazioni dell'applicazione.
    /// </summary>
    private const string REGISTRY_ROOT = @"Software\Studio Londe\StudioHub";

    /// <summary>
    /// Recupera il percorso dati precedentemente salvato dal Registro di Windows.
    /// </summary>
    /// <returns>
    /// La stringa contenente il percorso dati se presente; in caso contrario, <see langword="null"/> .
    /// </returns>
    internal static string? getDataPath() {

    DataPath.
        using RegistryKey? rk = Registry.CurrentUser.OpenSubKey(REGISTRY_ROOT);
        return rk?.GetValue("DataPath") as string;
    }

    /// <summary>
    /// Memorizza il percorso dati specificato nel Registro di Windows.
    /// </summary>
    /// <param name="value">Il percorso della directory da salvare come stringa.</param>
    internal static void setDataPath(string value) {
        using RegistryKey rk = Registry.CurrentUser.CreateSubKey(REGISTRY_ROOT);
        rk.SetValue("DataPath", value);
    }

    /// <summary>
    /// Crea l'alberatura delle cartelle di rete e salva il file di configurazione globale.
    /// </summary>
    /// <param name="config">Il modello contenente i parametri di connessione e il percorso radice.</param>
    /// <returns>True se l'operazione ha successo; altrimenti solleva un'eccezione logica.</returns>
    internal static void InitializeEcosystem(BootstrapConfig config) {
        if (string.IsNullOrWhiteSpace(config.NetworkDataPath)) {
            throw new ArgumentException("Il percorso di rete non può essere vuoto.");
        }

        // 1. Creazione Alberatura Cartelle
        string settingsPath = Path.Combine(config.NetworkDataPath, "settings");
        string templatesPath = Path.Combine(config.NetworkDataPath, "templates");
        string deployPath = Path.Combine(config.NetworkDataPath, "deploy");

        Directory.CreateDirectory(settingsPath);
        Directory.CreateDirectory(templatesPath);
        Directory.CreateDirectory(deployPath);

        // 2. Costruzione della Stringa di Connessione base (per il file JSON)
        // Nota: Costruiamo la stringa omettendo il catalogo iniziale (Initial Catalog), 
        // in modo che l'app possa connettersi al Server e poi smistare le query sui due db.
        // Oppure potresti voler salvare la stringa completa del PrimaryDB, a seconda della tua architettura RepoDb.
        // Qui la salvo puntando al PrimaryDB come default.

        string connectionString = config.IntegratedSecurity
            ? $"Data Source={config.DataSource};Initial Catalog={config.PrimaryDB};Integrated Security=True;Encrypt=False;"
            : $"Data Source={config.DataSource};Initial Catalog={config.PrimaryDB};User ID={config.UserID};Password={config.Password};Encrypt=False;";

        // 3. Creazione dell'oggetto per il JSON globale
        var globalSettings = new {
            ConnectionString = connectionString,
            PrimaryDatabase = config.PrimaryDB,
            LegacyDatabase = config.LegacyDB
        };

        // 4. Scrittura del file globalsettings.json
        string jsonFilePath = Path.Combine(settingsPath, "globalsettings.json");
        string jsonContent = JsonSerializer.Serialize(globalSettings, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(jsonFilePath, jsonContent);

        // 5. Generazione dello script di Setup per gli Utenti
        generateSetupScript(config.NetworkDataPath, deployPath);
    }

    /// <summary>
    /// Genera lo script batch (.cmd) per l'auto-configurazione delle postazioni client.
    /// </summary>
    /// <param name="rootPath">Il percorso di rete UNC dell'ecosistema StudioHub.</param>
    /// <param name="deployPath">La cartella in cui salvare lo script (es. \deploy).</param>
    private static void generateSetupScript(string rootPath, string deployPath) {
        string scriptPath = Path.Combine(deployPath, "Install_StudioHub.cmd");
        string clickOnceAppPath = Path.Combine(deployPath, "StudioHub.application");

        // Utilizziamo uno StringBuilder o una stringa multiline per comporre il batch
        string batchScript = $@"@echo off
echo ===================================================
echo Inizializzazione di StudioHub in corso...
echo ===================================================
echo.
echo Scrittura dei parametri di rete nel Registro di Sistema...
reg add ""HKCU\{REGISTRY_ROOT}"" /v ""DataPath"" /t REG_SZ /d ""{rootPath}"" /f >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo [ERRORE] Impossibile scrivere nel Registro di Sistema.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Avvio del setup di ClickOnce...
start """" ""{clickOnceAppPath}""

echo.
echo Configurazione completata. Questa finestra si chiudera' automaticamente.
timeout /t 3 >nul
";

        File.WriteAllText(scriptPath, batchScript);
    }


    /// <summary>
    /// Mostra una finestra di dialogo per la selezione di una cartella e restituisce il percorso selezionato.
    /// </summary>
    /// <param name="title">Titolo opzionale della finestra di dialogo.</param>
    /// <returns>Il percorso della cartella selezionata, oppure stringa vuota se annullato.</returns>
    internal static string getSelectedFolder(string? title = null) {
        OpenFolderDialog openFolderDialog = new() {
            Multiselect = false
        };
        if (!string.IsNullOrWhiteSpace(title)) {
            openFolderDialog.Title = title;
        }
        bool? dialogResult = openFolderDialog.ShowDialog();
        return dialogResult == true && !string.IsNullOrWhiteSpace(openFolderDialog.FolderName)
             ? openFolderDialog.FolderName.TrimEnd(Path.DirectorySeparatorChar)
             : string.Empty;
    }

    /// <summary>
    /// Restituisce i nomi dei file visibili nel percorso specificato in ordine alfabetico. Se il percorso non esiste,
    /// lo crea silenziosamente. Permette di specificare più estensioni nel filtro separate da punto e virgola (es.
    /// "*.txt;*.md").
    /// </summary>
    /// <param name="path">Il percorso della cartella da analizzare.</param>
    /// <param name="filter">
    /// Opzionale: filtro per i file (es. "*.txt;*.md"). Il default è "*.*" (tutti i file).
    /// </param>
    /// <param name="option">
    /// Opzionale: specifica se cercare solo nella cartella corrente o anche nelle sottocartelle.
    /// </param>
    /// <returns>Una lista di nomi di file visibili in ordine alfabetico.</returns>
    public static IEnumerable<string> GetVisibleFileNames(string path,
                                                          string filter = "*.*",
                                                          SearchOption option = SearchOption.TopDirectoryOnly) {
        DirectoryInfo directoryInfo = new(path);
        List<string> fileNames = [];

        string[] filters = filter.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (filters.Length == 0) {
            filters = ["*.*"];
        }

        foreach (string singleFilter in filters) {
            IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles(singleFilter, option)
                .Where(file => !file.Attributes.HasFlag(FileAttributes.Hidden));
            foreach (FileInfo file in files) {
                if (!fileNames.Contains(file.Name)) {
                    fileNames.Add(file.Name);
                }
            }
        }

        fileNames.Sort(StringComparer.OrdinalIgnoreCase);
        return fileNames;
    }

}
