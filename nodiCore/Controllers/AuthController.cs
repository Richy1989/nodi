using Microsoft.AspNetCore.Mvc;
using nodeCommon;
using nodiCore.DTOs;
using nodiCore.Services;

namespace nodiCore.Controllers;

/// <summary>
/// Public endpoints for user authentication. No [Authorize] attribute — these
/// endpoints must remain accessible without a token.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// Returns 401 if the credentials are invalid or the account is inactive.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { message = "Invalid username or password." });
        return Ok(result);
    }

    /// <summary>
    /// Registers a new user account and returns a JWT token on success.
    /// Returns 400 if registration is disabled or validation fails.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var (response, error) = await authService.RegisterAsync(request);
        if (error is not null)
            return BadRequest(new { message = error });
        return Ok(response);
    }
}
