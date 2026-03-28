using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;
using nodiCore.Services;

namespace nodiCore.Controllers;

/// <summary>
/// Admin-only endpoints for user and application settings management.
/// All routes require the Admin role — regular users receive 403 Forbidden.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db, SettingsService settingsService) : ControllerBase
{
    /// <summary>Returns all registered users, sorted alphabetically by username.</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users
            .OrderBy(u => u.Username)
            .Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role.ToString(), u.IsActive, u.CreatedAt))
            .ToListAsync();
        return Ok(users);
    }

    /// <summary>
    /// Updates a user's active state or role. Only non-null fields are applied.
    /// Invalid role strings are silently ignored (the enum parse fails and the field
    /// is left unchanged).
    /// </summary>
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

    /// <summary>
    /// Permanently deletes a user account. Admin accounts are protected — attempting
    /// to delete an admin returns 400 to prevent accidental lockout.
    /// </summary>
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

    /// <summary>Returns all application-level settings (e.g. AllowRegistration).</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        return Ok(await settingsService.GetAllAsync());
    }

    /// <summary>
    /// Batch-upserts application settings. Each item in the list is individually
    /// created or updated. Returns the full settings list after applying changes.
    /// </summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(List<AppSettingDto> settings)
    {
        foreach (var s in settings)
            await settingsService.UpsertAsync(s.Key, s.Value);
        return Ok(await settingsService.GetAllAsync());
    }
}
