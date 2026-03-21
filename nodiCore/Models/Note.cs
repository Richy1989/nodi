namespace nodiCore.Models;

public enum NoteColor
{
    Default, Salmon, Peach, LightYellow, Mint, Cyan, SkyBlue, CornflowerBlue, Lavender, HotPink, Tan, Silver
}

public enum NoteType { Text, Checklist }

public class Note
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public NoteColor Color { get; set; } = NoteColor.Default;
    public NoteType Type { get; set; } = NoteType.Text;

    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];
    public ICollection<NoteTag> NoteTags { get; set; } = [];
}
