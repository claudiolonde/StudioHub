using System.Windows;
using StudioHub;
using StudioHub.Views;

namespace _test;

public partial class MainWindow {

    public MainWindow() {
        InitializeComponent();
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
        ManageWordTemplatesView.Open("Meeting", ["nome", "cognome", "indirizzo"]);
    }
}
