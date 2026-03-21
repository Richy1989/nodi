namespace nodiCore.Models;

public class Tag
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<NoteTag> NoteTags { get; set; } = [];
}

public class NoteTag
{
    public int NoteId { get; set; }
    public Note Note { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
