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
        logger.LogInformation("Listing notes for user {UserId}", UserId);
        logger.LogDebug("GetNotes | user={UserId} archived={Archived} deleted={Deleted} tagId={TagId} search={Search}",
            UserId, archived, deleted, tagId, search);

        return Ok(await noteService.GetNotesAsync(UserId, archived, deleted, tagId, search));
    }

    /// <summary>Returns a single note by ID. Returns 404 if not found or not owned by the user.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNote(int id)
    {
        logger.LogInformation("Fetching note {NoteId} for user {UserId}", id, UserId);
        logger.LogDebug("GetNote | user={UserId} id={NoteId}", UserId, id);

        var note = await noteService.GetNoteAsync(UserId, id);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>Creates a new note. Returns 201 Created with the note location header.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateNote(CreateNoteRequest request)
    {
        logger.LogDebug("CreateNote | user={UserId} payload={@Request}", UserId, request);

        var note = await noteService.CreateNoteAsync(UserId, request);
        logger.LogInformation("Note {NoteId} created for user {UserId}", note.Id, UserId);
        return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
    }

    /// <summary>
    /// Updates an existing note. Only non-null fields in the request body are applied.
    /// Returns 404 if the note doesn't exist or belongs to another user.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, UpdateNoteRequest request)
    {
        logger.LogDebug("UpdateNote | user={UserId} id={NoteId} payload={@Request}", UserId, id, request);

        var note = await noteService.UpdateNoteAsync(UserId, id, request);
        if (note is null)
        {
            logger.LogInformation("Note {NoteId} not found for user {UserId}", id, UserId);
            return NotFound();
        }
        logger.LogInformation("Note {NoteId} updated for user {UserId}", id, UserId);
        return Ok(note);
    }

    /// <summary>
    /// Soft-deletes a note (moves it to the trash). Returns 204 on success, 404 if not found.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        logger.LogDebug("DeleteNote | user={UserId} id={NoteId}", UserId, id);

        var deleted = await noteService.DeleteNoteAsync(UserId, id);
        if (deleted) logger.LogInformation("Note {NoteId} moved to trash by user {UserId}", id, UserId);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Permanently removes a note from the database. Intended for notes already in the
    /// trash. Returns 204 on success, 404 if not found.
    /// </summary>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        logger.LogDebug("PermanentDelete | user={UserId} id={NoteId}", UserId, id);

        var deleted = await noteService.DeleteNoteAsync(UserId, id, permanent: true);
        if (deleted) logger.LogInformation("Note {NoteId} permanently deleted by user {UserId}", id, UserId);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Restores a soft-deleted note back to the active list.
    /// Returns 204 on success, 404 if the note is not in the trash.
    /// </summary>
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreNote(int id)
    {
        logger.LogDebug("RestoreNote | user={UserId} id={NoteId}", UserId, id);

        var restored = await noteService.RestoreNoteAsync(UserId, id);
        if (restored) logger.LogInformation("Note {NoteId} restored by user {UserId}", id, UserId);
        return restored ? NoContent() : NotFound();
    }
}
