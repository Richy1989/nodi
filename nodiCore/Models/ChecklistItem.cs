namespace nodiCore.Models;

public class ChecklistItem
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public Note Note { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public int Order { get; set; }
}
