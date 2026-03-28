using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

/// <summary>
/// Manages global <see cref="AppSetting"/> key-value pairs. These settings are
/// admin-only and affect application-wide behaviour (e.g. allowing registration).
/// </summary>
public class SettingsService(AppDbContext db)
{
    /// <summary>Returns all application settings as DTOs.</summary>
    public async Task<List<AppSettingDto>> GetAllAsync()
    {
        return await db.AppSettings
            .Select(s => new AppSettingDto(s.Key, s.Value))
            .ToListAsync();
    }

    /// <summary>Returns a single setting by key, or null if the key does not exist.</summary>
    public async Task<AppSettingDto?> GetAsync(string key)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting is null ? null : new AppSettingDto(setting.Key, setting.Value);
    }

    /// <summary>
    /// Creates the setting if it doesn't exist, or updates its value if it does.
    /// This upsert pattern means callers don't need to know whether a key already exists.
    /// </summary>
    public async Task UpsertAsync(string key, string value)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
        await db.SaveChangesAsync();
    }
}
