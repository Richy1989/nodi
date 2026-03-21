using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nodiCore.DTOs;
using nodiCore.Services;

namespace nodiCore.Controllers;

[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController(TagService tagService) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetTags()
    {
        return Ok(await tagService.GetTagsAsync(UserId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag(CreateTagRequest request)
    {
        var (tag, error) = await tagService.CreateTagAsync(UserId, request);
        if (error is not null)
            return BadRequest(new { message = error });
        return Ok(tag);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        return await tagService.DeleteTagAsync(UserId, id) ? NoContent() : NotFound();
    }
}
