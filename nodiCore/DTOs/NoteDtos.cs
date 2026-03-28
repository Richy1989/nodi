using nodeCommon;
using nodiCore.Models;

namespace nodiCore.DTOs;

/// <summary>A single checklist item within a create or update request.</summary>
public record CreateChecklistItemRequest(string Text, bool IsChecked = false, int Order = 0);

/// <summary>Request body for POST /api/notes.</summary>
public record CreateNoteRequest(
    string Title,
    string? Content,
    NoteColor Color = NoteColor.Default,
    NoteType Type = NoteType.Text,
    List<CreateChecklistItemRequest>? ChecklistItems = null,
    /// <summary>IDs of tags to attach. Only tags belonging to the requesting user are accepted.</summary>
    List<int>? TagIds = null
);

/// <summary>
/// Request body for PUT /api/notes/{id}. All fields are optional — only non-null
/// fields are applied, enabling partial updates without a full round-trip.
/// </summary>
public record UpdateNoteRequest(
    string? Title,
    string? Content,
    NoteColor? Color,
    bool? IsPinned,
    bool? IsArchived,
    /// <summary>When provided, replaces the entire checklist (full replacement, not diff).</summary>
    List<CreateChecklistItemRequest>? ChecklistItems,
    /// <summary>When provided, replaces the full tag set. Pass an empty list to remove all tags.</summary>
    List<int>? TagIds
);
