using System.Windows;
using StudioHub.Controls;
using StudioHub.Services;
using StudioHub.Views;

namespace _test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        if (!ConnectionInfoService.Initialize()) {
            Application.Current.Shutdown();
            return;
        }
    }

    private async void Button_Click(object sender, RoutedEventArgs e) {
        WordModelsView.Open();
        return;
        //EditConnectionInfoView.Open();
    }

}