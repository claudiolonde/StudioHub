using System.Windows;
using StudioHub.Services;

namespace _test;

public partial class App : Application {
    ///<inheritdoc/>
    protected override void OnStartup(StartupEventArgs e) {
        ConnectionInfoService.Initialize();
    }
}