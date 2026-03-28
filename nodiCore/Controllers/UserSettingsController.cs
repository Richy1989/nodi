using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nodiCore.Services;

namespace nodiCore.Controllers;

/// <summary>
/// Endpoints for reading and updating the authenticated user's personal settings
/// (currently theme preference). Requires a valid JWT token.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize]
public class UserSettingsController(UserSettingsService userSettings) : ControllerBase
{
    /// <summary>Extracts the authenticated user's ID from the JWT NameIdentifier claim.</summary>
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the current user's settings. Returns 404 if the user no longer exists.</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await userSettings.GetAsync(UserId);
        return settings is null ? NotFound() : Ok(settings);
    }

    /// <summary>
    /// Updates the user's theme. Accepts "Light" or "Dark"; any other value is
    /// coerced to "Dark". Returns the saved settings on success.
    /// </summary>
    [HttpPut("settings/theme")]
    public async Task<IActionResult> SetTheme([FromBody] string theme)
    {
        var settings = await userSettings.SetThemeAsync(UserId, theme);
        return settings is null ? NotFound() : Ok(settings);
    }
}
