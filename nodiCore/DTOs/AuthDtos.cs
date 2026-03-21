namespace nodiCore.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(string Username, string Email, string Password);

public record AuthResponse(
    string Token,
    int UserId,
    string Username,
    string Email,
    string Role
);
