using nodeCommon;

namespace nodiCore.Models;

/// <summary>
/// Represents a single note owned by a user. Can be a plain-text note or a
/// checklist, distinguished by <see cref="Type"/>. Notes are never hard-deleted
/// by default — they move through IsDeleted → permanent removal.
/// </summary>
public class Note
{
    public int Id { get; set; }

    /// <summary>Foreign key to the owning <see cref="User"/>.</summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    /// <summary>Plain-text body. Null for checklist notes where items hold the content.</summary>
    public string? Content { get; set; }

    /// <summary>Background colour used in the UI. Defaults to the theme default colour.</summary>
    public NoteColor Color { get; set; } = NoteColor.Default;

    /// <summary>Text or Checklist. Determines whether <see cref="ChecklistItems"/> are rendered.</summary>
    public NoteType Type { get; set; } = NoteType.Text;

    /// <summary>Pinned notes appear at the top of the note grid.</summary>
    public bool IsPinned { get; set; }

    /// <summary>Archived notes are hidden from the main view but not deleted.</summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Soft-delete flag. Deleted notes are moved to the trash and can be restored
    /// or permanently removed. See <see cref="Services.NoteService.DeleteNoteAsync"/>.
    /// </summary>
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Updated whenever the note content, colour, pin, or archive state changes.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Ordered checklist items. Only relevant when <see cref="Type"/> is Checklist.</summary>
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];

    /// <summary>Join table entries linking this note to its tags.</summary>
    public ICollection<NoteTag> NoteTags { get; set; } = [];
}
