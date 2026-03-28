namespace nodiCore.DTOs;

/// <summary>Request body for POST /api/auth/login.</summary>
public record LoginRequest(string Username, string Password);

/// <summary>Request body for POST /api/auth/register.</summary>
public record RegisterRequest(string Username, string Email, string Password);
