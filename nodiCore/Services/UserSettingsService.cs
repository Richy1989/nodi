using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;

namespace nodiCore.Services;

/// <summary>
/// Manages per-user preferences (currently theme). Settings are stored directly
/// on the <see cref="Models.User"/> entity rather than a separate table.
/// </summary>
public class UserSettingsService(AppDbContext db)
{
    /// <summary>Returns the settings for a user, or null if the user doesn't exist.</summary>
    public async Task<UserSettingsDto?> GetAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        return user is null ? null : new UserSettingsDto(user.Theme);
    }

    /// <summary>
    /// Updates the user's theme preference. Only "Light" and "Dark" are valid;
    /// any other value is coerced to "Dark" to ensure the UI always has a known state.
    /// </summary>
    public async Task<UserSettingsDto?> SetThemeAsync(int userId, string theme)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;
        user.Theme = theme is "Light" or "Dark" ? theme : "Dark";
        await db.SaveChangesAsync();
        return new UserSettingsDto(user.Theme);
    }
}
