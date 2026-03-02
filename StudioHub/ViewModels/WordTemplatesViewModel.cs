using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudioHub.Models;

namespace StudioHub.ViewModels;

public partial class WordTemplatesViewModel : ObservableObject {

    public string Path { get; set; }

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTemplatesCount))]
    private IEnumerable<string> _totalTemplates = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredTemplatesCount))]
    private ObservableCollection<string> _filteredTemplates = [];

    public int TotalTemplatesCount => TotalTemplates?.Count() ?? 0;

    public int FilteredTemplatesCount => FilteredTemplates?.Count() ?? 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteTemplateCommand))]
    private string? _selectedTemplate = null;

    public WordTemplatesViewModel(string appName, string[] headers) {

        Path = System.IO.Path.Combine(Hub.DataPath, "Templates", "Microsoft Word", appName);
        if (System.IO.Directory.Exists(Path) == false) {
            System.IO.Directory.CreateDirectory(Path);
        }
        TotalTemplates = IO.GetVisibleFileNames(Path, "*.doc;*.docx");
        ApplyFilter();
    }

    public void ApplyFilter() {
        if (string.IsNullOrWhiteSpace(FilterText)) {
            FilteredTemplates = TotalTemplates is null
                              ? []
                              : new ObservableCollection<string>(TotalTemplates);
        }
        else {
            string filter = FilterText.Trim().ToLowerInvariant();
            FilteredTemplates = TotalTemplates is null
                              ? []
                              : new ObservableCollection<string>(
                                  TotalTemplates.Where(t => t != null && t.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
                                  );
        }
    }

    partial void OnFilterTextChanged(string value) {
        ApplyFilter();
    }


    private const string TRASH_FOLDER_NAME = "$Trash";

    private bool CanDeleteTemplate() {
        return !string.IsNullOrWhiteSpace(SelectedTemplate);
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTemplate))]
    public void DeleteTemplate(string templateName) {
        string originalFilePath = System.IO.Path.Combine(Path, templateName);

        if (!System.IO.File.Exists(originalFilePath)) {
            return;
        }

        string trashFolderPath = System.IO.Path.Combine(Path, TRASH_FOLDER_NAME);

        // Creazione lazy della directory del cestino impostata come nascosta
        if (!System.IO.Directory.Exists(trashFolderPath)) {
            System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(trashFolderPath);
            directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;
        }

        string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(templateName);
        string fileExtension = System.IO.Path.GetExtension(templateName);
        string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Anti-collisione: appending del timestamp al nome del file originale
        string newFileName = $"{fileNameWithoutExtension}_{timeStamp}{fileExtension}";
        string destinationFilePath = System.IO.Path.Combine(trashFolderPath, newFileName);

        try {
            System.IO.File.Move(originalFilePath, destinationFilePath);

            TotalTemplates = IO.GetVisibleFileNames(Path, "*.doc;*.docx");
            ApplyFilter();
        }
        catch (System.IO.IOException) {
            Dialog.Show("Impossibile spostare il file nel cestino. Assicurati che non sia aperto in Word.", DialogIcon.Error);
        }
    }
}