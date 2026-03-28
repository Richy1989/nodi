using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

/// <summary>
/// Handles user authentication and registration, issuing JWT tokens for
/// successful logins. Token configuration (key, issuer, expiry) is read
/// from <c>appsettings.json</c> under the <c>Jwt</c> section.
/// </summary>
public class AuthService(AppDbContext db, IConfiguration config)
{
    /// <summary>
    /// Validates credentials and returns an <see cref="AuthResponse"/> containing
    /// a JWT token. Returns null if the username doesn't exist, the account is
    /// inactive, or the password doesn't match.
    /// </summary>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        // Only active users can log in; inactive accounts are treated as not found.
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Username == request.Username && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return new AuthResponse(GenerateToken(user), user.Id, user.Username, user.Email, user.Role.ToString());
    }

    /// <summary>
    /// Creates a new user account if registration is enabled and the username/email
    /// are unique. Returns the auth response (including a token) on success, or an
    /// error message on failure.
    /// </summary>
    public async Task<(AuthResponse? Response, string? Error)> RegisterAsync(RegisterRequest request)
    {
        // Registration can be disabled by an admin via the AllowRegistration setting.
        var allowReg = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == SettingKeys.AllowRegistration);
        if (allowReg?.Value != "true")
            return (null, "Registration is currently disabled.");

        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return (null, "Username already taken.");

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return (null, "Email already in use.");

        if (request.Password.Length < 8)
            return (null, "Password must be at least 8 characters.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (new AuthResponse(GenerateToken(user), user.Id, user.Username, user.Email, user.Role.ToString()), null);
    }

    /// <summary>
    /// Builds and signs a JWT token embedding the user's ID, username, email,
    /// and role as standard claims. Expiry defaults to 72 hours if not configured.
    /// </summary>
    private string GenerateToken(User user)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
        var issuer = config["Jwt:Issuer"] ?? "nodiCore";
        var audience = config["Jwt:Audience"] ?? "nodiClients";
        var expiryHours = config.GetValue<int>("Jwt:ExpiryHours", 72);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
