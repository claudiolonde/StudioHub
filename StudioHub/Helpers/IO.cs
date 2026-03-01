namespace StudioHub.Helpers;

public class IO {


    public static string GetSelectedFolder(string? title = null) {

        Microsoft.Win32.OpenFolderDialog dialog = new() {
            Multiselect = false
        };
        if (!string.IsNullOrWhiteSpace(title)) {
            dialog.Title = title;
        }
        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FolderName : string.Empty;

    }

}