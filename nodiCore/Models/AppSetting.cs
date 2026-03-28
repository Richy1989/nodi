namespace nodiCore.Models;

/// <summary>
/// A global application setting stored as a key-value pair. Settings are managed by
/// admins via <c>/api/admin/settings</c> and seeded with defaults on first run.
/// </summary>
public class AppSetting
{
    public int Id { get; set; }

    /// <summary>Unique setting key. See <see cref="SettingKeys"/> for well-known keys.</summary>
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

/// <summary>Well-known keys for <see cref="AppSetting"/> to avoid magic strings.</summary>
public static class SettingKeys
{
    /// <summary>
    /// Controls whether new users can self-register. Set to "true" to allow, anything
    /// else to disable. Checked by <c>AuthService.RegisterAsync</c>.
    /// </summary>
    public const string AllowRegistration = "AllowRegistration";
}
