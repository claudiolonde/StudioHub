namespace StudioHub.Models;

public static class DataPath {

    //static internal string root = @"\\SERVER-18\Studio Londe\StudioHub";
    static internal string root = @"D:\FILES\StudioHub";
    public static string Settings => @$"{root}\settings";
    public static string GlobalSettingsJson => @$"{Settings}\GlobalSettings.json";

}
