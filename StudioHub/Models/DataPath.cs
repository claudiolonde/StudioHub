namespace StudioHub.Models;

public static class DataPath {

    static internal string root = @"\\SERVER-18\Studio Londe\StudioHub";
    static internal string trashFolderName = "$Recycle";

    public static string Template => @$"{root}\Template";
    public static string TemplateWord => @$"{Template}\Word";

    public static string Settings => @$"{root}\Settings";
    public static string SettingsGlobalSettingsJson => @$"{Settings}\GlobalSettings.json";

}
