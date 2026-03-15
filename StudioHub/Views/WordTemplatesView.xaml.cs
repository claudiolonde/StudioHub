using System.IO;
using System.Windows;
using StudioHub.ViewModels;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per xaml
/// </summary>
public partial class WordTemplatesView {

    public WordTemplatesView() {
        InitializeComponent();
    }

    /// <summary>
    /// Apre la vista.
    /// </summary>
    /// <remarks>
    /// Inizializza il ViewModel, imposta la proprietà <see cref="Window.Owner"/>, disabilita l'icona della finestra,
    /// </remarks>
    public static void Open(string appName, string[] headers) {

        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentNullException.ThrowIfNull(headers);

        if (string.IsNullOrWhiteSpace(DataPath.root)) {
            Dialog.Show(
                "La cartella dati dell'applicazione non è impostata correttamente.",
                "Gestione modelli Word",
                null,
                DialogType.Error
            );
            return;
        }

        string appPath = Path.Combine(DataPath.TemplateWord, appName);
        try {
            _ = Directory.CreateDirectory(appPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException) {
            Dialog.Show(
                $"Impossibile creare la cartella dati:\n{appPath}.",
                "Gestione modelli Word",
                null,
                DialogType.Error
            );
            return;
        }

        WordTemplatesViewModel vm = new(appPath, headers);
        WordTemplatesView w = new() {
            DataContext = vm
        };
        w.Show();
    }
}
