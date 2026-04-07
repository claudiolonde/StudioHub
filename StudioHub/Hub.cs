using RepoDb;
using StudioHub.Models;

namespace StudioHub;

public static class Hub {

    public const string SOLUTION_NAME = "StudioHub";

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

    public static DataSourceInfo DataSource = new();

    /// <summary>
    /// Esegue il bootstrap dell'applicazione caricando le configurazioni e impostando le stringhe di connessione.
    /// </summary>
    /// <returns>
    /// True se l'inizializzazione è completata con successo o se è già stata eseguita.
    /// </returns>
    public static bool Initialize() {

        if (_isInitialized) { return true; }

        DataSource = new() {
            Server = "192.168.123.18",
            UserID = "sa",
            Password = "gestionale_2008",
            Database = ("StudioHub", "ARCHIVIO")
        };

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
