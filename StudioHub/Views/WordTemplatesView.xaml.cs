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
    /// Inizializza il ViewModel, imposta la proprietà <see cref="Window.Owner"/> , disabilita l'icona della finestra,
    /// </remarks>
    public static void Open(string appName, string[] headers) {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentNullException.ThrowIfNull(headers);

        string dataPath = Hub.DataPath;

        if (string.IsNullOrWhiteSpace(dataPath)) {
            Dialog.Show("La cartella dati dell'applicazione non è impostata correttamente.", DialogIcon.Error);
            return;
        }

        string path = System.IO.Path.Combine(dataPath, "Templates", "Microsoft Word", appName);
        try {
            _ = System.IO.Directory.CreateDirectory(path);
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or System.Security.SecurityException) {
            Dialog.Show($"Impossibile creare la cartella dati:\n{path}.", DialogIcon.Error);
            return;
        }

        WordTemplatesViewModel viewModel = new(path, headers);
        WordTemplatesView window = new() {
            DataContext = viewModel
        };
        window.Show();
    }
}
