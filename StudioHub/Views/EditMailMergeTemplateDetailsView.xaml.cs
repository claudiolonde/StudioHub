using System.Windows;
using StudioHub.ViewModels;

namespace StudioHub.Views;

/// <summary>
/// Logica di interazione per EditMailMergeTemplateDetailsView.xaml
/// </summary>
public partial class EditMailMergeTemplateDetailsView : Window {

    /// <inheritdoc/>
    public EditMailMergeTemplateDetailsView() {
        InitializeComponent();
    }

    /// <inheritdoc/>
    public static (string? Name, string? Description) Open(IEnumerable<string> existingNames, string currentName = "", string currentDescription = "") {

        EditMailMergeTemplateDetailsViewModel vm = new(existingNames, currentName, currentDescription);
        EditMailMergeTemplateDetailsView v = new() {
            Owner = GetActiveWindow(),
            DataContext = vm,
        };

        // Chiude la finestra quando l'evento Saved viene sollevato dal ViewModel.
        vm.Saved += (s, e) => v.Close();

        v.ShowDialog();
        if (vm.IsSaved) {
            return (vm.Name, vm.Description);
        }
        return (null, null);
    }
}