namespace nodiCore.DTOs;

/// <summary>
/// Request body for PUT /api/admin/users/{id}. Both fields are optional so
/// admins can update only the active state or only the role in a single call.
/// </summary>
public record UpdateUserRequest(bool? IsActive, string? Role);
