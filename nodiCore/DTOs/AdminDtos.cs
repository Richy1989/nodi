namespace nodiCore.DTOs;

public record UserDto(
    int Id,
    string Username,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateUserRequest(bool? IsActive, string? Role);

public record AppSettingDto(string Key, string Value);
