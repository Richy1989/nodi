namespace nodiCore.Models;

/// <summary>
/// A single item within a checklist-type <see cref="Note"/>.
/// Items are replaced wholesale on update — the existing list is removed and rebuilt
/// from the request payload.
/// </summary>
public class ChecklistItem
{
    public int Id { get; set; }

    /// <summary>Foreign key to the parent <see cref="Note"/>.</summary>
    public int NoteId { get; set; }
    public Note Note { get; set; } = null!;

    /// <summary>Display text of the checklist entry.</summary>
    public string Text { get; set; } = string.Empty;

    public bool IsChecked { get; set; }

    /// <summary>
    /// Zero-based display order within the note. When not explicitly provided by the
    /// client, the position in the submitted list is used as a fallback.
    /// </summary>
    public int Order { get; set; }
}
