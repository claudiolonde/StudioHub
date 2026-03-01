using System.Windows;
using System.Windows.Controls;
using StudioHub.Services;
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

        WordTemplatesViewModel vm = new(appName, headers) { };

        WordTemplatesView v = new() {
            Owner = GetActiveWindow(),
            DataContext = vm
        };

        // Disabilita l'icona della finestra.
        //v.SourceInitialized += (s, e) => DisableWindowIcon(v);


        v.ShowDialog();

    }
}