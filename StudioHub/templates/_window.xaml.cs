using System.Windows;
using StudioHub.Services;
using StudioHub.ViewModels;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per xaml
/// </summary>
public partial class WordModelsView : Window {
    public WordModelsView() {
        InitializeComponent();
        //if (!ConnectionInfoService.Initialize()) {
        //    Application.Current.Shutdown();
        //    return;
        //}
    }

    /// <summary>
    /// Apre la vista.
    /// </summary>
    /// <remarks>
    /// Inizializza il ViewModel, imposta la proprietà <see cref="Window.Owner"/> , disabilita l'icona della finestra,
    /// </remarks>
    public static bool Open() {

        _viewModel vm = new();
        WordModelsView v = new() {
            Owner = GetActiveWindow(),
            DataContext = vm
        };

        // Disabilita l'icona della finestra.
        v.SourceInitialized += (s, e) => DisableWindowIcon(v);

        // Chiude la finestra quando l'evento Saved viene sollevato dal ViewModel.
        vm.Saved += (s, e) => v.Close();

        v.ShowDialog();
        return vm.IsSaved;

    }

}