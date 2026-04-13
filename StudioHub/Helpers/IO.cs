using Microsoft.Win32;

namespace Studio.Helpers;

internal class IO {

    /// <summary>
    /// Percorso della chiave di registro utilizzata per memorizzare le impostazioni dell'applicazione.
    /// </summary>
    private const string REGISTRY_ROOT = @"Software\Studio Londe\StudioHub";

    /// <summary>
    /// Recupera il percorso dati precedentemente salvato dal Registro di Windows.
    /// </summary>
    /// <returns>
    /// La stringa contenente il percorso dati se presente; in caso contrario, <see langword="null"/>.
    /// </returns>
    internal static string? getDataPath() {
        using RegistryKey? rk = Registry.CurrentUser.OpenSubKey(REGISTRY_ROOT);
        return rk?.GetValue("DataPath") as string;
    }

    /// <summary>
    /// Memorizza il percorso dati specificato nel Registro di Windows.
    /// </summary>
    /// <param name="value">Il percorso della directory da salvare come stringa.</param>
    internal static void setDataPath(string value) {
        using RegistryKey rk = Registry.CurrentUser.CreateSubKey(REGISTRY_ROOT);
        rk.SetValue("DataPath", value);
    }

}
