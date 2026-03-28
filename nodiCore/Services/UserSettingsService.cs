using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;

namespace nodiCore.Services;

public class UserSettingsService(AppDbContext db)
{
    public async Task<UserSettingsDto?> GetAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        return user is null ? null : new UserSettingsDto(user.Theme);
    }

    public async Task<UserSettingsDto?> SetThemeAsync(int userId, string theme)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;
        user.Theme = theme is "Light" or "Dark" ? theme : "Dark";
        await db.SaveChangesAsync();
        return new UserSettingsDto(user.Theme);
    }
}
