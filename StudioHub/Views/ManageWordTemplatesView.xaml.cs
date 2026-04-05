using System.Runtime.CompilerServices;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per xaml
/// </summary>
public partial class ManageWordTemplatesView {

    public ManageWordTemplatesView() {
        InitializeComponent();
    }

    /// <summary>
    /// Apre la vista, inizializza il ViewModel e assegna i callback per i dialoghi di dettaglio.
    /// </summary>
    /// <param name="appName">Nome dell'applicazione chiamante.</param>
    /// <param name="headers">Intestazioni di colonna per il datasource di Word.</param>
    public static void Open(string appName, string[] headers) {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);
        ArgumentNullException.ThrowIfNull(headers);

        ManageWordTemplatesViewModel vm = new();
        ManageWordTemplatesView w = new() { DataContext = vm };
        Func<TaskAwaiter> _ = vm.Initialize(appName, headers).GetAwaiter;

        // Assegna il callback per il salvataggio di un nuovo template
        vm.RequestNewTemplateDetails = _ => {
            (string Name, string Description)? result = EditWordTemplateDetailsView.ShowDialog(w);
            return Task.FromResult(result);
        };

        // Assegna il callback per la modifica dei dettagli di un template esistente
        vm.RequestEditTemplateDetails = template => {
            (string Name, string Description)? result = EditWordTemplateDetailsView.ShowDialog(w, template.Name, template.Description);
            return Task.FromResult(result);
        };

        w.Show();
    }
}
