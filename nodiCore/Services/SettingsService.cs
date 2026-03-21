using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

public class SettingsService(AppDbContext db)
{
    public async Task<List<AppSettingDto>> GetAllAsync()
    {
        return await db.AppSettings
            .Select(s => new AppSettingDto(s.Key, s.Value))
            .ToListAsync();
    }

    public async Task<AppSettingDto?> GetAsync(string key)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting is null ? null : new AppSettingDto(setting.Key, setting.Value);
    }

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
