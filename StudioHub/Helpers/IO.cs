using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace StudioHub.Helpers;

public class IO {


    public static string GetSelectedFolder(string? title = null) {

        Microsoft.Win32.OpenFolderDialog dialog = new() {
            Multiselect = false
        };
        if (!string.IsNullOrWhiteSpace(title)) {
            dialog.Title = title;
        }
        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FolderName : string.Empty;

    }

    /// <summary>
    /// Restituisce i nomi dei file visibili nel percorso specificato. Se il percorso non esiste, lo crea
    /// silenziosamente.
    /// </summary>
    /// <param name="path">Il percorso della cartella da analizzare.</param>
    /// <param name="searchPattern">
    /// Opzionale: filtro per i file (es. "*.txt"). Il default è "*.*" (tutti i file).
    /// </param>
    /// <param name="searchOption">
    /// Opzionale: specifica se cercare solo nella cartella corrente o anche nelle sottocartelle.
    /// </param>
    /// <returns>Una lista di nomi di file visibili.</returns>
    public static IEnumerable<string> GetVisibleFileNames(
        string path,
        string searchPattern = "*.*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly) {
        // Se il percorso non esiste, lo crea silenziosamente
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);

            // Dato che la cartella è appena stata creata, sarà sicuramente vuota.
            // Restituiamo subito una collezione vuota per risparmiare risorse.
            return [];
        }

        DirectoryInfo directoryInfo = new(path);

        // Se la cartella esisteva, procediamo con la normale lettura e filtraggio
        return directoryInfo.EnumerateFiles(searchPattern, searchOption)
                            .Where(file => !file.Attributes.HasFlag(FileAttributes.Hidden))
                            .Select(file => file.Name);
    }

}