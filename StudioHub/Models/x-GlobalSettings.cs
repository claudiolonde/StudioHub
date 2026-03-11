namespace StudioHub.Models;

internal class GlobalSettings {

    /// <summary>
    /// Credenziali di accesso all'istanza SQL Server comune.
    /// </summary>
    public DataSourceCredentials Credentials { get; set; } = new DataSourceCredentials();

    /// <summary>
    /// Nome del database primario (StudioHub).
    /// </summary>
    public string PrimaryDbName { get; set; } = string.Empty;

    /// <summary>
    /// Nome del database legacy (CityUp).
    /// </summary>
    public string LegacyDbName { get; set; } = string.Empty;
}