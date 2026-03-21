namespace nodiCore.DTOs;

public record TagDto(int Id, string Name, int NoteCount);

public record CreateTagRequest(string Name);
