using System.Windows;
using StudioHub;
using StudioHub.Views;

namespace _test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView : Window {

    public MainView() {
        InitializeComponent();
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
    }
    private async void Button_Click(object sender, RoutedEventArgs e) {
        WordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
        return;
        //EditConnectionInfoView.Open();
    }

}