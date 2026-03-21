using nodeCommon;
using SQLite;

namespace nodiApp.Models;

public enum SyncStatus { Synced, PendingCreate, PendingUpdate, PendingDelete }

[Table("Notes")]
public class Note
{
    [PrimaryKey, AutoIncrement]
    public int LocalId { get; set; }

    public int? ServerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public NoteColor Color { get; set; } = NoteColor.Default;
    public NoteType Type { get; set; } = NoteType.Text;
    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public SyncStatus SyncStatus { get; set; } = SyncStatus.PendingCreate;
}

[Table("ChecklistItems")]
public class ChecklistItem
{
    [PrimaryKey, AutoIncrement]
    public int LocalId { get; set; }
    public int NoteLocalId { get; set; }
    public int? ServerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public int Order { get; set; }
}

[Table("Tags")]
public class Tag
{
    [PrimaryKey, AutoIncrement]
    public int LocalId { get; set; }
    public int? ServerId { get; set; }
    public string Name { get; set; } = string.Empty;
}

[Table("NoteTags")]
public class NoteTag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int NoteLocalId { get; set; }
    public int TagLocalId { get; set; }
}
