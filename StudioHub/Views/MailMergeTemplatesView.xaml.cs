using System.Windows;
using System.Windows.Input;
using StudioHub.ViewModels;
using StudioHub.Models;

namespace StudioHub.Views;
/// <summary>
/// Logica di interazione per MailMergeTemplatesView.xaml
/// </summary>
public partial class MailMergeTemplatesView : Window {

    /// <inheritdoc/>
    public MailMergeTemplatesView() {
        InitializeComponent();
        //Loaded += MailMergeTemplatesView_Loaded;
    }

    /// <inheritdoc/>
    public static void Open(string appName, string[] headers) {

        MailMergeTemplatesViewModel vm = new() {
            AppName = appName,
            Headers = headers
        };
        MailMergeTemplatesView v = new() {
            Owner = GetActiveWindow(),
            DataContext = vm,
            Title = $"Gestione modelli - {appName}"
        };

        // Disabilita l'icona della finestra all'inizializzazione della sorgente.
        v.SourceInitialized += (s, e) => DisableWindowIcon(v);

        // Permette la chiusura della finestra tramite il tasto ESC.
        //v.PreviewKeyDown += (s, e) => {
        //    if (e.Key == Key.Escape) {
        //        v.Close();
        //        e.Handled = true;
        //    }
        //};

        // Annulla l'operazione di connessione se la finestra viene chiusa mentre il comando è in esecuzione.
        //v.Closing += (s, e) => {
        //    if (vm.ConnectCommand.IsRunning) {
        //        vm.ConnectCommand.Cancel();
        //    }
        //};

        // Chiude la finestra quando l'evento Saved viene sollevato dal ViewModel.
        //vm.Saved += (s, e) => v.Close();

        _ = vm.LoadTemplatesCommand.ExecuteAsync(null);
        v.ShowDialog();
    }


    private void MailMergeTemplatesView_Loaded(object sender, RoutedEventArgs e) {
        for (int n = 0; n < 53; n++) {
            listTemplates.Items.Add(new MailMergeTemplateInfo() {
                Name = $"Convocazione assemblea straordinaria n. {DateTime.Now.Millisecond}",
                LastModified = new DateTime(2026, 9, 15),
                Size = 2934790,
                Description = "Allineamento Colonne: Ho impostato la colonna del Nome a 200 e della Data a 100."
            });

        }



    }
}
