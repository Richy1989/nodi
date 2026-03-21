using nodiCore.Models;

namespace nodiCore.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        await db.Database.EnsureCreatedAsync();

        if (!db.AppSettings.Any())
        {
            db.AppSettings.Add(new AppSetting { Key = SettingKeys.AllowRegistration, Value = "true" });
            await db.SaveChangesAsync();
        }

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
