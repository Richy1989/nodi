using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nodiCore.Services;

namespace nodiCore.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserSettingsController(UserSettingsService userSettings) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await userSettings.GetAsync(UserId);
        return settings is null ? NotFound() : Ok(settings);
    }

    [HttpPut("settings/theme")]
    public async Task<IActionResult> SetTheme([FromBody] string theme)
    {
        var settings = await userSettings.SetThemeAsync(UserId, theme);
        return settings is null ? NotFound() : Ok(settings);
    }
}
