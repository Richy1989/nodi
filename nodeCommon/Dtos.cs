namespace nodeCommon;

/// <summary>
/// Returned by the API after a successful login or registration.
/// Contains the JWT token and basic user information.
/// </summary>
public record AuthResponse(string Token, int UserId, string Username, string Email, string Role);

/// <summary>
/// A single item within a checklist note.
/// </summary>
public record ChecklistItemDto(int Id, string Text, bool IsChecked, int Order);

/// <summary>
/// Full representation of a note as returned by the API.
/// <para>
/// <see cref="Color"/> and <see cref="Type"/> are serialised as strings
/// matching the <see cref="NoteColor"/> and <see cref="NoteType"/> enum names.
/// </para>
/// </summary>
public record NoteDto(
    int Id,
    string Title,
    string? Content,
    /// <see cref="NoteColor"/>
    string Color,
    /// <see cref="NoteType"/>
    string Type,
    bool IsPinned,
    bool IsArchived,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ChecklistItemDto> ChecklistItems,
    List<TagDto> Tags
);

/// <summary>
/// A tag used to organise notes. <see cref="NoteCount"/> reflects how many
/// notes the tag is currently attached to.
/// </summary>
public record TagDto(int Id, string Name, int NoteCount);

/// <summary>
/// Basic user information returned by admin endpoints.
/// </summary>
public record UserDto(int Id, string Username, string Email, string Role, bool IsActive, DateTime CreatedAt);

/// <summary>
/// A single application-wide configuration entry (key/value pair).
/// </summary>
public record AppSettingDto(string Key, string Value);
