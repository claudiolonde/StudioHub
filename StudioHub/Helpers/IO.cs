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
    /// <param name="filter">
    /// Opzionale: filtro per i file (es. "*.txt"). Il default è "*.*" (tutti i file).
    /// </param>
    /// <param name="option">
    /// Opzionale: specifica se cercare solo nella cartella corrente o anche nelle sottocartelle.
    /// </param>
    /// <returns>Una lista di nomi di file visibili.</returns>
    public static IEnumerable<string> GetVisibleFileNames(string path,
                                                          string filter = "*.*",
                                                          SearchOption option = SearchOption.TopDirectoryOnly) {
        if (!Directory.Exists(path)) {
            try {
                Directory.CreateDirectory(path);
            }
            catch {
                Dialog.Show($"Impossibile trovare una parte del percorso\n{path}", DialogIcon.Error);
            }
            return [];
        }

        DirectoryInfo directoryInfo = new(path);
        return directoryInfo.EnumerateFiles(filter, option)
                            .Where(file => !file.Attributes.HasFlag(FileAttributes.Hidden))
                            .Select(file => file.Name);
    }

}