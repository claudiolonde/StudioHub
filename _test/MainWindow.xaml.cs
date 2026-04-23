using System.Windows;
using Studio;
using Studio.Views;

namespace Meeting;

public partial class MainWindow {

    public MainWindow() {
        InitializeComponent();
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
        UserLoginView w = new();
        w.ShowDialog();
        return;
        ManageWordTemplatesView.Open("Meeting", ["nome", "cognome", "indirizzo"]);
    }
}
