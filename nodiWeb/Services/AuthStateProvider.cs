using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using nodeCommon;

namespace nodiWeb.Services;

public class AuthStateProvider(ProtectedLocalStorage storage) : AuthenticationStateProvider
{
    private const string StorageKey = "nodi_auth";

    private AuthResponse? _currentUser;

    public AuthResponse? CurrentUser => _currentUser;

    /// <summary>
    /// False until TryRestoreFromStorageAsync has completed its first check.
    /// RedirectToLogin waits for this before navigating, preventing a premature
    /// redirect on page refresh before the stored session has been read.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(BuildState());

    public async Task SetUserAsync(AuthResponse? user)
    {
        _currentUser = user;
        try
        {
            if (user is not null)
                await storage.SetAsync(StorageKey, user);
            else
                await storage.DeleteAsync(StorageKey);
        }
        catch { /* JS not available during prerender — storage write is best-effort */ }

        NotifyAuthenticationStateChanged(Task.FromResult(BuildState()));
    }

    /// <summary>
    /// Reads the persisted session from browser storage and marks the provider as
    /// initialized. Must be called from OnAfterRenderAsync(firstRender: true) where
    /// JS interop is available.
    /// </summary>
    public async Task TryRestoreFromStorageAsync()
    {
        if (IsInitialized) return;

        try
        {
            var result = await storage.GetAsync<AuthResponse>(StorageKey);
            if (result.Success && result.Value is not null)
                _currentUser = result.Value;
        }
        catch { /* corrupted or missing storage entry — stay logged out */ }

        IsInitialized = true;
        NotifyAuthenticationStateChanged(Task.FromResult(BuildState()));
    }

    private AuthenticationState BuildState()
    {
        if (_currentUser is null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _currentUser.UserId.ToString()),
            new Claim(ClaimTypes.Name, _currentUser.Username),
            new Claim(ClaimTypes.Email, _currentUser.Email),
            new Claim(ClaimTypes.Role, _currentUser.Role)
        };

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }
}
