using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;
using nodiCore.Services;

namespace nodiCore.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db, SettingsService settingsService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role.ToString(), u.IsActive, u.CreatedAt))
            .ToListAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (request.IsActive is not null) user.IsActive = request.IsActive.Value;
        if (request.Role is not null && Enum.TryParse<UserRole>(request.Role, out var role))
            user.Role = role;

        await db.SaveChangesAsync();
        return Ok(new UserDto(user.Id, user.Username, user.Email, user.Role.ToString(), user.IsActive, user.CreatedAt));
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role == UserRole.Admin) return BadRequest(new { message = "Cannot delete admin user." });
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        return Ok(await settingsService.GetAllAsync());
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(List<AppSettingDto> settings)
    {
        foreach (var s in settings)
            await settingsService.UpsertAsync(s.Key, s.Value);
        return Ok(await settingsService.GetAllAsync());
    }
}
