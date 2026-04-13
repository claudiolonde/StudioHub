using System.Windows;
namespace Meeting;

public partial class App : Application {
    public static string Name => "Meeting";
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        //Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();
    }
}
