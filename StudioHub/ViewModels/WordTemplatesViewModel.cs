using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class WordTemplatesViewModel : ObservableObject {

    public string? Path { get; set; }

    [ObservableProperty]
    private string? _filterText;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTemplatesCount))]
    private IEnumerable<string>? _totalTemplates = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredTemplatesCount))]
    private IEnumerable<string>? _filteredTemplates = [];

    public int TotalTemplatesCount => TotalTemplates?.Count() ?? 0;

    public int FilteredTemplatesCount => FilteredTemplates?.Count() ?? 0;


    public WordTemplatesViewModel(string appName, string[] headers) {
        Path = System.IO.Path.Combine(TEMPLATES_PATH, "Microsoft Word", appName);
        TotalTemplates = Helpers.IO.GetVisibleFileNames(Path);
        ApplyFilter();
    }

    public void ApplyFilter() {
        if (string.IsNullOrWhiteSpace(FilterText)) {
            FilteredTemplates = TotalTemplates?.ToList() ?? [];
        }
        else {
            string filter = FilterText.Trim().ToLowerInvariant();
            FilteredTemplates = TotalTemplates?
                .Where(t => t != null && t.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                .ToList() ?? [];
        }
    }

    partial void OnFilterTextChanged(string? value) {
        ApplyFilter();
    }

}