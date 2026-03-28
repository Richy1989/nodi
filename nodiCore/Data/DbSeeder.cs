using nodiCore.Models;

namespace nodiCore.Data;

/// <summary>
/// Populates the database with required initial data on first run. Called during
/// application startup after <c>EnsureCreated</c> so the schema already exists.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seeds default <see cref="AppSetting"/> rows and creates the first admin user if
    /// no admin exists. Admin credentials can be overridden via the <c>Admin</c> section
    /// in <c>appsettings.json</c>; the hard-coded fallback is admin / Admin1234!.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        await db.Database.EnsureCreatedAsync();

        // Seed default app settings only on a fresh database.
        if (!db.AppSettings.Any())
        {
            db.AppSettings.Add(new AppSetting { Key = SettingKeys.AllowRegistration, Value = "true" });
            await db.SaveChangesAsync();
        }

        // Create the admin account if there is no admin user yet (e.g. first run,
        // or the previous admin was deleted externally).
        if (!db.Users.Any(u => u.Role == UserRole.Admin))
        {
            var adminSection = config.GetSection("Admin");
            var username = adminSection["Username"] ?? "admin";
            var email = adminSection["Email"] ?? "admin@nodi.local";
            var password = adminSection["Password"] ?? "Admin1234!";

            db.Users.Add(new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.Admin,
                IsActive = true
            });
            await db.SaveChangesAsync();
        }
    }
}
