using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Services;

namespace nodiCore.Controllers;

[ApiController]
[Route("api/notes")]
[Authorize]
public class NotesController(NoteService noteService, ILogger<NotesController> logger) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetNotes(
        [FromQuery] bool archived = false,
        [FromQuery] bool deleted = false,
        [FromQuery] int? tagId = null,
        [FromQuery] string? search = null)
    {
        logger.LogInformation("GetNotes | user={UserId} archived={Archived} deleted={Deleted} tagId={TagId} search={Search}",
            UserId, archived, deleted, tagId, search);

        return Ok(await noteService.GetNotesAsync(UserId, archived, deleted, tagId, search));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNote(int id)
    {
        logger.LogInformation("GetNote | user={UserId} id={NoteId}", UserId, id);

        var note = await noteService.GetNoteAsync(UserId, id);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNote(CreateNoteRequest request)
    {
        logger.LogInformation("CreateNote | user={UserId} payload={@Request}", UserId, request);

        var note = await noteService.CreateNoteAsync(UserId, request);
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, UpdateNoteRequest request)
    {
        logger.LogInformation("UpdateNote | user={UserId} id={NoteId} payload={@Request}", UserId, id, request);

        var note = await noteService.UpdateNoteAsync(UserId, id, request);
        return note is null ? NotFound() : Ok(note);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        logger.LogInformation("DeleteNote | user={UserId} id={NoteId}", UserId, id);

        return await noteService.DeleteNoteAsync(UserId, id) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        logger.LogInformation("PermanentDelete | user={UserId} id={NoteId}", UserId, id);

        return await noteService.DeleteNoteAsync(UserId, id, permanent: true) ? NoContent() : NotFound();
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreNote(int id)
    {
        logger.LogInformation("RestoreNote | user={UserId} id={NoteId}", UserId, id);

        return await noteService.RestoreNoteAsync(UserId, id) ? NoContent() : NotFound();
    }
}
