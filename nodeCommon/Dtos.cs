namespace nodeCommon;

public record AuthResponse(string Token, int UserId, string Username, string Email, string Role);

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

public record TagDto(int Id, string Name, int NoteCount);

public record UserDto(int Id, string Username, string Email, string Role, bool IsActive, DateTime CreatedAt);

public record AppSettingDto(string Key, string Value);
