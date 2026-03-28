namespace nodiCore.Services;

/// <summary>
/// Strongly-typed wrapper around the application configuration. Inject this
/// service instead of <see cref="IConfiguration"/> to keep dependencies explicit
/// and limit each service to only the config values it actually needs.
/// </summary>
public class ConfigurationService(IConfiguration config)
{
    /// <summary>
    /// Root directory for all persistent files (SQLite database, future uploads, etc.).
    /// Resolved from the <c>DataFolder</c> config key or the <c>DataFolder</c> environment
    /// variable. Defaults to <c>data</c> relative to the working directory.
    /// </summary>
    /// <summary>
    /// Resolves relative paths against the working directory so all callers
    /// always receive an absolute path regardless of how the value was set.
    /// </summary>
    public string DataFolder { get; } = Path.GetFullPath(config.GetValue<string>("DataFolder") ?? "programData");
}
