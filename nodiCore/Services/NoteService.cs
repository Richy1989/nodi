using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

/// <summary>
/// Handles all CRUD operations for <see cref="Note"/> entities. All queries are
/// automatically scoped to the requesting user — notes from other users are never
/// returned or modified.
/// </summary>
public class NoteService(AppDbContext db)
{
    /// <summary>
    /// Returns notes for a user with optional filtering by state, tag, and search term.
    /// Results are ordered: pinned notes first, then by most recently updated.
    /// </summary>
    /// <param name="archived">When true, returns archived notes instead of active ones.</param>
    /// <param name="deleted">When true, returns soft-deleted (trash) notes.</param>
    /// <param name="tagId">When set, restricts results to notes that have this tag.</param>
    /// <param name="search">Case-insensitive substring match against title and content.</param>
    public async Task<List<NoteDto>> GetNotesAsync(int userId, bool archived = false, bool deleted = false, int? tagId = null, string? search = null)
    {
        var query = db.Notes
            .Include(n => n.ChecklistItems)
            .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
            .Where(n => n.UserId == userId && n.IsArchived == archived && n.IsDeleted == deleted);

        if (tagId.HasValue)
            query = query.Where(n => n.NoteTags.Any(nt => nt.TagId == tagId));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(n => n.Title.Contains(search) || (n.Content != null && n.Content.Contains(search)));

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToListAsync();

        return notes.Select(MapToDto).ToList();
    }

    /// <summary>Returns a single note by ID, or null if not found or not owned by the user.</summary>
    public async Task<NoteDto?> GetNoteAsync(int userId, int noteId)
    {
        var note = await db.Notes
            .Include(n => n.ChecklistItems)
            .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        return note is null ? null : MapToDto(note);
    }

    /// <summary>
    /// Creates a new note. Checklist items are created in the same transaction.
    /// Tags are linked afterward via <see cref="SetTagsAsync"/>; only tag IDs
    /// belonging to this user are accepted.
    /// </summary>
    public async Task<NoteDto> CreateNoteAsync(int userId, CreateNoteRequest request)
    {
        var note = new Note
        {
            UserId = userId,
            Title = request.Title,
            Content = request.Content,
            Color = request.Color,
            Type = request.Type
        };

        if (request.ChecklistItems is { Count: > 0 })
        {
            note.ChecklistItems = request.ChecklistItems
                .Select((item, idx) => new ChecklistItem
                {
                    Text = item.Text,
                    IsChecked = item.IsChecked,
                    // Use the client-supplied order if provided, otherwise fall back
                    // to the item's position in the submitted list.
                    Order = item.Order > 0 ? item.Order : idx
                }).ToList();
        }

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        if (request.TagIds is { Count: > 0 })
            await SetTagsAsync(userId, note.Id, request.TagIds);

        // Re-fetch to include navigation properties (tags, checklist items) in the response.
        return await GetNoteAsync(userId, note.Id) ?? MapToDto(note);
    }

    /// <summary>
    /// Partially updates a note. Only fields that are non-null in the request are applied,
    /// allowing clients to patch a single field without sending the full note.
    /// </summary>
    public async Task<NoteDto?> UpdateNoteAsync(int userId, int noteId, UpdateNoteRequest request)
    {
        var note = await db.Notes
            .Include(n => n.ChecklistItems)
            .Include(n => n.NoteTags)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        if (note is null) return null;

        if (request.Title is not null) note.Title = request.Title;
        if (request.Content is not null) note.Content = request.Content;
        if (request.Color is not null) note.Color = request.Color.Value;
        if (request.IsPinned is not null) note.IsPinned = request.IsPinned.Value;
        if (request.IsArchived is not null) note.IsArchived = request.IsArchived.Value;

        if (request.ChecklistItems is not null)
        {
            // Replace the checklist entirely rather than diffing, which keeps the
            // logic simple and avoids stale items when items are reordered or removed.
            db.ChecklistItems.RemoveRange(note.ChecklistItems);
            note.ChecklistItems = request.ChecklistItems
                .Select((item, idx) => new ChecklistItem
                {
                    NoteId = note.Id,
                    Text = item.Text,
                    IsChecked = item.IsChecked,
                    Order = item.Order > 0 ? item.Order : idx
                }).ToList();
        }

        note.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        if (request.TagIds is not null)
            await SetTagsAsync(userId, note.Id, request.TagIds);

        return await GetNoteAsync(userId, noteId);
    }

    /// <summary>
    /// Deletes a note. By default performs a soft delete (sets <c>IsDeleted = true</c>).
    /// Pass <paramref name="permanent"/> = true to permanently remove the record.
    /// Returns false if the note does not exist or is not owned by the user.
    /// </summary>
    public async Task<bool> DeleteNoteAsync(int userId, int noteId, bool permanent = false)
    {
        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
        if (note is null) return false;

        if (permanent)
            db.Notes.Remove(note);
        else
            note.IsDeleted = true;

        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Clears the soft-delete flag on a note, moving it back to the active list.
    /// Returns false if the note is not found in the trash.
    /// </summary>
    public async Task<bool> RestoreNoteAsync(int userId, int noteId)
    {
        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.IsDeleted);
        if (note is null) return false;
        note.IsDeleted = false;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Replaces the full set of tags on a note. Only tag IDs that belong to the
    /// requesting user are applied — any foreign tag IDs in the list are silently ignored.
    /// </summary>
    private async Task SetTagsAsync(int userId, int noteId, List<int> tagIds)
    {
        var existing = db.NoteTags.Where(nt => nt.NoteId == noteId);
        db.NoteTags.RemoveRange(existing);

        // Filter to tags owned by this user to prevent assigning another user's tags.
        var validTagIds = await db.Tags
            .Where(t => tagIds.Contains(t.Id) && t.UserId == userId)
            .Select(t => t.Id)
            .ToListAsync();

        db.NoteTags.AddRange(validTagIds.Select(tagId => new NoteTag { NoteId = noteId, TagId = tagId }));
        await db.SaveChangesAsync();
    }

    /// <summary>Maps a <see cref="Note"/> entity to its DTO representation.</summary>
    private static NoteDto MapToDto(Note n) => new(
        n.Id, n.Title, n.Content,
        n.Color.ToString(), n.Type.ToString(),
        n.IsPinned, n.IsArchived, n.IsDeleted,
        n.CreatedAt, n.UpdatedAt,
        n.ChecklistItems.OrderBy(c => c.Order).Select(c => new ChecklistItemDto(c.Id, c.Text, c.IsChecked, c.Order)).ToList(),
        n.NoteTags.Select(nt => new TagDto(nt.Tag.Id, nt.Tag.Name, 0)).ToList()
    );
}
