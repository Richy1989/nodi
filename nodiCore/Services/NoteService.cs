using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

public class NoteService(AppDbContext db)
{
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

    public async Task<NoteDto?> GetNoteAsync(int userId, int noteId)
    {
        var note = await db.Notes
            .Include(n => n.ChecklistItems)
            .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);

        return note is null ? null : MapToDto(note);
    }

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
                    Order = item.Order > 0 ? item.Order : idx
                }).ToList();
        }

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        if (request.TagIds is { Count: > 0 })
            await SetTagsAsync(userId, note.Id, request.TagIds);

        return await GetNoteAsync(userId, note.Id) ?? MapToDto(note);
    }

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

    public async Task<bool> RestoreNoteAsync(int userId, int noteId)
    {
        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId && n.IsDeleted);
        if (note is null) return false;
        note.IsDeleted = false;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task SetTagsAsync(int userId, int noteId, List<int> tagIds)
    {
        var existing = db.NoteTags.Where(nt => nt.NoteId == noteId);
        db.NoteTags.RemoveRange(existing);

        var validTagIds = await db.Tags
            .Where(t => tagIds.Contains(t.Id) && t.UserId == userId)
            .Select(t => t.Id)
            .ToListAsync();

        db.NoteTags.AddRange(validTagIds.Select(tagId => new NoteTag { NoteId = noteId, TagId = tagId }));
        await db.SaveChangesAsync();
    }

    private static NoteDto MapToDto(Note n) => new(
        n.Id, n.Title, n.Content,
        n.Color.ToString(), n.Type.ToString(),
        n.IsPinned, n.IsArchived, n.IsDeleted,
        n.CreatedAt, n.UpdatedAt,
        n.ChecklistItems.OrderBy(c => c.Order).Select(c => new ChecklistItemDto(c.Id, c.Text, c.IsChecked, c.Order)).ToList(),
        n.NoteTags.Select(nt => new TagDto(nt.Tag.Id, nt.Tag.Name, 0)).ToList()
    );
}
