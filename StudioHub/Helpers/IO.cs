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
