using System.IO;
using Microsoft.Win32;

namespace StudioHub.Helpers;

public class IO {

    /// <summary>
    /// Mostra una finestra di dialogo per la selezione di una cartella e restituisce il percorso selezionato.
    /// </summary>
    /// <param name="title">Titolo opzionale della finestra di dialogo.</param>
    /// <returns>Il percorso della cartella selezionata, oppure stringa vuota se annullato.</returns>
    public static string GetSelectedFolder(string? title = null) {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog {
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