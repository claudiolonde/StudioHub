using RepoDb;
using StudioHub.Models;

namespace StudioHub;

public static class Hub {

    #region    Paths  ----------------------------------------------------------------------------------------------------

    //internal static string root = @"\\SERVER-18\Studio Londe\StudioHub";
    private static string root = @"D:\FILES\StudioHub";
    public static string Settings => @$"{root}\settings";

    /// <summary>
    /// Nome completo del file .json che contiene le informazioni di connessione alla sorgente dati e i nomi dei
    /// database
    /// </summary>
    public static string GlobalSettingsJson => @$"{Settings}\GlobalSettings.json";

    #endregion Paths  ----------------------------------------------------------------------------------------------------


    /// <summary>
    /// Flag che indica se il servizio è stato inizializzato.
    /// </summary>
    private static bool _isInitialized = false;

    /// <summary>
    /// Percorso della chiave di registro utilizzata dall'applicazione per memorizzare le impostazioni di connessione.
    /// </summary>
    public static string RegistryPath => @"Software\Studio Londe\StudioHub";

    /// <summary>
    /// Stringa di connessione principale utilizzata per accedere al database primario.
    /// </summary>
    public static string PrimaryConnection { get; private set; } = string.Empty;

    /// <summary>
    /// Stringa di connessione utilizzata per accedere al database legacy.
    /// </summary>
    public static string LegacyConnection { get; private set; } = string.Empty;

    /// <summary>
    /// Nome del database primario a cui si connette l'applicazione.
    /// </summary>
    public static string PrimaryDbName { get; private set; } = string.Empty;

    /// <summary>
    /// Nome del database legacy del gestionale CityUp.
    /// </summary>
    public static string LegacyDbName { get; private set; } = string.Empty;

    /// <summary>
    /// Percorso della cartella dati condivisa utilizzata dall'applicazione.
    /// </summary>
    public static string DataPath { get; private set; } = string.Empty;

    /// <summary>
    /// Nome costante della cartella utilizzata per il cestino.
    /// </summary>
    public static string TrashFolderName => "$Recycle.Bin";


    /// <summary>
    /// Esegue il bootstrap dell'applicazione caricando le configurazioni e impostando le stringhe di connessione.
    /// </summary>
    /// <returns>True se l'inizializzazione è completata con successo o se è già stata eseguita.</returns>
    public static bool Initialize() {

        if (_isInitialized) {
            return true;
        }

        GlobalConfiguration.Setup().UseSqlServer();
        mapEntities();

        _isInitialized = true;
        return true;
    }

    /*
       FluentMapper.Entity<ModelName>()
           .Table("Schema.TableName")
           .Primary(x => x.Id)
           .Identity(x => x.Id);
   */
    private static void mapEntities() {
        FluentMapper.Entity<WordTemplate>()
            .Table("Hub.WordTemplates")
            .Primary(x => x.Id)
            .Identity(x => x.Id);
    }

}
