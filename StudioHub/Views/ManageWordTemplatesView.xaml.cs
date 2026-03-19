using System.IO;
using System.Windows;
using StudioHub.ViewModels;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per xaml
/// </summary>
public partial class ManageWordTemplatesView {

    public ManageWordTemplatesView() {
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
            Dialog.Show(DialogType.Error,
                "La cartella dati dell'applicazione non è impostata correttamente.");
            return;
        }

        string appPath = Path.Combine(DataPath.TemplateWord, appName);
        try {
            _ = Directory.CreateDirectory(appPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException) {
            Dialog.Show(DialogType.Error,
                $"Impossibile creare la cartella dati:\n{appPath}.");
            return;
        }

        ManageWordTemplatesViewModel vm = new(appPath, headers);
        ManageWordTemplatesView w = new() {
            DataContext = vm
        };
        w.Show();
    }
}
