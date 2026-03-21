using Microsoft.EntityFrameworkCore;
using nodiCore.Data;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Models;

namespace nodiCore.Services;

public class TagService(AppDbContext db)
{
    public async Task<List<TagDto>> GetTagsAsync(int userId)
    {
        return await db.Tags
            .Where(t => t.UserId == userId)
            .Select(t => new TagDto(
                t.Id, t.Name,
                t.NoteTags.Count(nt => !nt.Note.IsDeleted && !nt.Note.IsArchived)
            ))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

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

    public async Task<bool> DeleteTagAsync(int userId, int tagId)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.UserId == userId);
        if (tag is null) return false;
        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
        return true;
    }
}
