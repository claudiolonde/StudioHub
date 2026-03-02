using RepoDb;
using StudioHub.Models;

namespace StudioHub;

public static class Hub {

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
    /// Il punto di ingresso unico. Accende l'infrastruttura, configura le connessioni e inizializza RepoDb.
    /// </summary>
    public static bool Initialize() {

        if (_isInitialized) {
            return true;
        }

        BootstrapInfo? info = LoadConfig();
        PrimaryConnection = info.PrimaryConnectionString;
        LegacyConnection = info.LegacyConnectionString;
        PrimaryDbName = info.PrimaryDb;
        LegacyDbName = info.LegacyDb;
        DataPath = info.DataPath;

        GlobalConfiguration.Setup().UseSqlServer();

        _isInitialized = true;
        return true;
    }

    private static void MapEntities() {
        /*
        FluentMapper.Entity<ModelName>()
            .Table("Schema.TableName")
            .Primary(x => x.Id)
            .Identity(x => x.Id);
        */

    }

    private static BootstrapInfo LoadConfig() {
        return new BootstrapInfo {
            DataSource = "192.168.123.18",
            UserID = "sa",
            Password = "gestionale_2008",
            IntegratedSecurity = false,
            PrimaryDb = "StudioHub",
            LegacyDb = "ARCHIVIO",
            DataPath = @"D:\FILES\Documenti\StudioHub"
        };
    }
}