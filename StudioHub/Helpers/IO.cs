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