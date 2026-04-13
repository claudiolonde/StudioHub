using System.Data.Common;
using System.Diagnostics;
using System.Windows;
using Studio;
using Studio.Models;
using Studio.Views;
using Wpf.Ui.Appearance;

namespace Studio.Views;
/// <summary>
/// Interaction logic for BaseView1.xaml
/// </summary>
public partial class ManageWordTemplatesView {

    public ManageWordTemplatesView() {
        //SystemThemeWatcher.Watch(this);
        InitializeComponent();
    }

    /// <summary>
    /// Apre la finestra di gestione dei template Word per l'applicazione specificata.<br/> Consente la modifica e la
    /// gestione dei template disponibili per l'app target.
    /// </summary>
    /// <param name="appName">
    /// Nome dell'applicazione target. Non può essere <see langword="null" /> o vuoto.
    /// </param>
    /// <param name="headers">
    /// Array di intestazioni disponibili per i template. Non può essere <see langword="null" />.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Generato se <paramref name="appName" /> è <see langword="null" /> o vuoto.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Generato se <paramref name="headers" /> è <see langword="null" />.
    /// </exception>
    public static void Open(string appName, string[] headers) {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentNullException.ThrowIfNull(headers);

        ManageWordTemplatesViewModel vm = new();
        ManageWordTemplatesView w = new() { DataContext = vm };

        _ = vm.InitializeAsync(appName, headers);

        vm.RequestTemplateDetails = (name, unavailableNames, description) => {
            (string Name, string Description)? result = EditWordTemplateDetailsView.Open(w, name, unavailableNames, description);
            return Task.FromResult(result);
        };

        w.Show();
    }

}
