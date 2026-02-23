using System.Windows;
using System.Windows.Input;
using StudioHub.ViewModels;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per EditConnectionInfoView.xaml
/// </summary>
public partial class EditConnectionInfoView : Window {

    ///<inheritdoc/>
    public EditConnectionInfoView() {
        InitializeComponent();
    }

    /// <summary>
    /// Apre la finestra di dialogo per modificare le informazioni di connessione.
    /// </summary>
    /// <remarks>
    /// Inizializza il ViewModel, imposta la proprietà <see cref="Window.Owner"/>, 
    /// disabilita l'icona della finestra, abilita la chiusura tramite ESC,
    /// gestisce la cancellazione del comando di connessione in chiusura,
    /// e chiude la finestra al salvataggio.
    /// </remarks>
    public static bool Open() {

        EditConnectionInfoViewModel vm = new();
        EditConnectionInfoView v = new() {
            Owner = GetActiveWindow(),
            DataContext = vm
        };

        // Disabilita l'icona della finestra all'inizializzazione della sorgente.
        v.SourceInitialized += (s, e) => DisableWindowIcon(v);

        // Permette la chiusura della finestra tramite il tasto ESC.
        v.PreviewKeyDown += (s, e) => {
            if (e.Key == Key.Escape) {
                v.Close();
                e.Handled = true;
            }
        };

        // Annulla l'operazione di connessione se la finestra viene chiusa mentre il comando è in esecuzione.
        v.Closing += (s, e) => {
            if (vm.ConnectCommand.IsRunning) {
                vm.ConnectCommand.Cancel();
            }
        };

        // Chiude la finestra quando l'evento Saved viene sollevato dal ViewModel.
        vm.Saved += (s, e) => v.Close();
        v.ShowDialog();
        return vm.IsSaved;
    }

}