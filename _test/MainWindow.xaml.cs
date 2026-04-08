using System.Data.Common;
using System.Windows;
using StudioHub;
using StudioHub.Views;
using StudioHub.Models;
using System.Diagnostics;

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
        StudioHub.Views.MainView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
        //ManageWordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
    }
}
