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
    /// Inizializza il ViewModel, imposta la proprietà <see cref="Window.Owner"/>.
    /// </remarks>
    public static void Open(string appName, string[] headers) {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentNullException.ThrowIfNull(headers);

        ManageWordTemplatesViewModel vm = new();
        ManageWordTemplatesView w = new() {
            DataContext = vm
        };
        w.Show();
    }

}
