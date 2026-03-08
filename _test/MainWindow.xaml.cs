using System.Windows;
using StudioHub;
using StudioHub.Views;

#pragma warning disable IDE1006 // Stili di denominazione
namespace _test;
#pragma warning restore IDE1006 // Stili di denominazione

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView {

    public MainView() {
        InitializeComponent();
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
    }
    private void button_Click(object sender, RoutedEventArgs e) {
        WordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
        return;
        //EditConnectionInfoView.Open();
    }

}
