using System.Data.Common;
using System.Windows;
using StudioHub;
using StudioHub.Views;
using StudioHub.Models;

namespace _test;

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
        ManageWordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
        return;
    }

}
