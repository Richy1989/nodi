namespace nodiCore.Models;

/// <summary>
/// A user-defined label that can be applied to multiple notes.
/// Tags are scoped to a user — two users can have tags with the same name without conflict.
/// </summary>
public class Tag
{
    public int Id { get; set; }

    /// <summary>Foreign key to the owning <see cref="User"/>.</summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>Join table entries linking this tag to its notes.</summary>
    public ICollection<NoteTag> NoteTags { get; set; } = [];
}

/// <summary>
/// Many-to-many join entity between <see cref="Note"/> and <see cref="Tag"/>.
/// Uses a composite primary key (NoteId, TagId) — see <c>AppDbContext.OnModelCreating</c>.
/// </summary>
public class NoteTag
{
    public int NoteId { get; set; }
    public Note Note { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
