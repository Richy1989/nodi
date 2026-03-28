using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Services;

namespace nodiCore.Controllers;

/// <summary>
/// Endpoints for managing user-scoped tags. All routes require a valid JWT token.
/// Tags from other users are never visible or modifiable through these endpoints.
/// </summary>
[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController(TagService tagService) : ControllerBase
{
    /// <summary>Extracts the authenticated user's ID from the JWT NameIdentifier claim.</summary>
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns all tags for the authenticated user, alphabetically sorted.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        return Ok(await tagService.GetTagsAsync(UserId));
    }

    /// <summary>
    /// Creates a new tag. Returns 400 if the name is blank or already exists for this user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTag(CreateTagRequest request)
    {
        var (tag, error) = await tagService.CreateTagAsync(UserId, request);
        if (error is not null)
            return BadRequest(new { message = error });
        return Ok(tag);
    }

    /// <summary>
    /// Permanently deletes a tag and removes it from all notes.
    /// Returns 204 on success, 404 if not found or not owned by the user.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        return await tagService.DeleteTagAsync(UserId, id) ? NoContent() : NotFound();
    }
}
