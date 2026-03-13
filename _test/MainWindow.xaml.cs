using System.Windows;
using StudioHub;
using StudioHub.Views;

namespace _test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView {

    public MainView() {
        InitializeComponent();
        Dialog.Show("Ecco il mio primo messaggio per te!", null, null, MessageType.Error);
        return        ;
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
    }
    private void button_Click(object sender, RoutedEventArgs e) {
        WordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
        return;
    }

}
