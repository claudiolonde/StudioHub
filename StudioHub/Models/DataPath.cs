namespace StudioHub.Models;

public static class DataPath {

    public static string Root => @"\\SERVER\StudioHub";

    public static string RootTemplate => @$"{Root}\Template";
    public static string RootTemplateWord => @$"{RootTemplate}\Word";
    public static string RootTemplateWordMeeting => @$"{RootTemplateWord}\Meeting";

    public static string RootSettings => @$"{Root}\Settings";
    public static string RootSettingsGlobalJson => @$"{RootSettings}\GlobalSettings.json";

    public static string RootDeploy => @$"{Root}\Deploy";
    public static string RootDeployScript => @$"{RootDeploy}\Install StudioHub.cmd";

    public static string TrashFolderName => "$Recycle.Bin";

}
