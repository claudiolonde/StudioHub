using System.Data.Common;
using System.Diagnostics;
using System.Windows;
using StudioHub;
using StudioHub.Models;
using StudioHub.Views;

namespace StudioHub.Views;
/// <summary>
/// Interaction logic for BaseView1.xaml
/// </summary>
public partial class MainView {

    public MainView() {
        InitializeComponent();
        if (!Hub.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
        Debug.Write(Width + " " + Height);
        //ManageWordTemplatesView.Open(App.Name, ["Nome", "Cognome", "Indirizzo"]);
    }
}
