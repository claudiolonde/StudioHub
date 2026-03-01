using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudioHub.ViewModels;

public partial class WordTemplatesViewModel : ObservableObject {

    public string Path { get; set; }

    [ObservableProperty]
    private int _totalTemplates = 8;
    
    [ObservableProperty]
    private int _filteredTemplates = 25;

}