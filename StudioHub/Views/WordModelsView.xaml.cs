using System.Windows;
using StudioHub.ViewModels;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per xaml
/// </summary>
public partial class WordTemplatesView : Window {

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

        string path = string.Empty;
        if (string.IsNullOrWhiteSpace(Hub.DataPath)) {
            Dialog.Show("La cartella dati dell'applicazione non è impostata correttamente.", DialogIcon.Error);
            return;
        }
        else {
            path = System.IO.Path.Combine(Hub.DataPath, "Templates", "Microsoft Word", appName);
            try {
                if (System.IO.Directory.Exists(path) == false) {
                    System.IO.Directory.CreateDirectory(path);
                }
            }
            catch {
                Dialog.Show($"Impossibile creare la cartella dati:\n{path}.", DialogIcon.Error);
                return;
            }
        }

        WordTemplatesViewModel vm = new(path, headers) { };
        WordTemplatesView v = new() {
            DataContext = vm
        };
        v.Show();

    }
}