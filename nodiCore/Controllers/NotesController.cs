using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Services;

namespace nodiCore.Controllers;

/// <summary>
/// CRUD endpoints for notes. All routes require a valid JWT token.
/// Every operation is automatically scoped to the authenticated user's notes.
/// </summary>
[ApiController]
[Route("api/notes")]
[Authorize]
public class NotesController(NoteService noteService, ILogger<NotesController> logger) : ControllerBase
{
    /// <summary>Extracts the authenticated user's ID from the JWT NameIdentifier claim.</summary>
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Returns notes for the authenticated user. Defaults to active (non-archived,
    /// non-deleted) notes. Use <paramref name="archived"/> or <paramref name="deleted"/>
    /// to fetch those views.
    /// </summary>
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

    /// <summary>Returns a single note by ID. Returns 404 if not found or not owned by the user.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNote(int id)
    {
        logger.LogInformation("GetNote | user={UserId} id={NoteId}", UserId, id);

        var note = await noteService.GetNoteAsync(UserId, id);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>Creates a new note. Returns 201 Created with the note location header.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateNote(CreateNoteRequest request)
    {
        logger.LogInformation("CreateNote | user={UserId} payload={@Request}", UserId, request);

        var note = await noteService.CreateNoteAsync(UserId, request);
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    /// <summary>
    /// Updates an existing note. Only non-null fields in the request body are applied.
    /// Returns 404 if the note doesn't exist or belongs to another user.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, UpdateNoteRequest request)
    {
        logger.LogInformation("UpdateNote | user={UserId} id={NoteId} payload={@Request}", UserId, id, request);

        var note = await noteService.UpdateNoteAsync(UserId, id, request);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>
    /// Soft-deletes a note (moves it to the trash). Returns 204 on success, 404 if not found.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        logger.LogInformation("DeleteNote | user={UserId} id={NoteId}", UserId, id);

        return await noteService.DeleteNoteAsync(UserId, id) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Permanently removes a note from the database. Intended for notes already in the
    /// trash. Returns 204 on success, 404 if not found.
    /// </summary>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        logger.LogInformation("PermanentDelete | user={UserId} id={NoteId}", UserId, id);

        return await noteService.DeleteNoteAsync(UserId, id, permanent: true) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Restores a soft-deleted note back to the active list.
    /// Returns 204 on success, 404 if the note is not in the trash.
    /// </summary>
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreNote(int id)
    {
        logger.LogInformation("RestoreNote | user={UserId} id={NoteId}", UserId, id);

        return await noteService.RestoreNoteAsync(UserId, id) ? NoContent() : NotFound();
    }
}
