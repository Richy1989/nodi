using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

/// <summary>
/// Manages user-scoped tags. All operations are restricted to tags owned by
/// the requesting user — tags from other users are never visible or modifiable.
/// </summary>
public class TagService(AppDbContext db)
{
    /// <summary>
    /// Returns all tags for a user, alphabetically sorted. The <c>NoteCount</c>
    /// on each tag reflects only active (non-deleted, non-archived) notes.
    /// </summary>
    public async Task<List<TagDto>> GetTagsAsync(int userId)
    {
        return await db.Tags
            .Where(t => t.UserId == userId)
            .Select(t => new TagDto(
                t.Id, t.Name,
                // Count only active notes to match what the user sees in the main view.
                t.NoteTags.Count(nt => !nt.Note.IsDeleted && !nt.Note.IsArchived)
            ))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new tag. Returns an error if the name is blank or a tag with
    /// the same name already exists for this user.
    /// </summary>
    public async Task<(TagDto? Tag, string? Error)> CreateTagAsync(int userId, CreateTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (null, "Tag name cannot be empty.");

        if (await db.Tags.AnyAsync(t => t.UserId == userId && t.Name == request.Name))
            return (null, "Tag already exists.");

        var tag = new Tag { UserId = userId, Name = request.Name.Trim() };
        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        return (new TagDto(tag.Id, tag.Name, 0), null);
    }

    /// <summary>
    /// Permanently deletes a tag. Associated <see cref="NoteTag"/> join rows are
    /// removed automatically via cascading deletes defined in EF Core.
    /// Returns false if the tag is not found or not owned by the user.
    /// </summary>
    public async Task<bool> DeleteTagAsync(int userId, int tagId)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.UserId == userId);
        if (tag is null) return false;
        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
        return true;
    }
}
