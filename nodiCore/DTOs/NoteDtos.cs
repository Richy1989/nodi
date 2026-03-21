using nodeCommon;
using nodiCore.Models;

namespace nodiCore.DTOs;

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
