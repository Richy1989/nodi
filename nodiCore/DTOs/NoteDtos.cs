using nodiCore.Models;

namespace nodiCore.DTOs;

public record ChecklistItemDto(int Id, string Text, bool IsChecked, int Order);

public record NoteDto(
    int Id,
    string Title,
    string? Content,
    string Color,
    string Type,
    bool IsPinned,
    bool IsArchived,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ChecklistItemDto> ChecklistItems,
    List<TagDto> Tags
);

public record CreateChecklistItemRequest(string Text, bool IsChecked = false, int Order = 0);

public record CreateNoteRequest(
    string Title,
    string? Content,
    NoteColor Color = NoteColor.Default,
    NoteType Type = NoteType.Text,
    List<CreateChecklistItemRequest>? ChecklistItems = null,
    List<int>? TagIds = null
);

public record UpdateNoteRequest(
    string? Title,
    string? Content,
    NoteColor? Color,
    bool? IsPinned,
    bool? IsArchived,
    List<CreateChecklistItemRequest>? ChecklistItems,
    List<int>? TagIds
);
