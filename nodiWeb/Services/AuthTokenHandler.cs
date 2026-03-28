using System.Net;

namespace nodiWeb.Services;

/// <summary>
/// HTTP message handler that automatically attaches the current user's JWT
/// to every outgoing request made through the "nodiApi" HttpClient, and
/// handles token expiry globally so individual API calls don't have to.
/// </summary>
/// <remarks>
/// Registered as a <see cref="DelegatingHandler"/> in Program.cs and added
/// to the "nodiApi" HttpClient pipeline via AddHttpMessageHandler.
/// </remarks>
public class AuthTokenHandler(AuthStateProvider authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        // Attach the Bearer token if a user is currently signed in.
        var token = authState.CurrentUser?.Token;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new("Bearer", token);

        var response = await base.SendAsync(request, ct);

        // If the API returns 401 the token is expired or revoked.
        // Clearing the user triggers NotifyAuthenticationStateChanged, which
        // causes AuthorizeRouteView to re-evaluate → shows RedirectToLogin
        // → navigates to /login. ApiService methods return empty/null instead
        // of throwing, so no circuit crash occurs.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            await authState.SetUserAsync(null);

        return response;
    }
}
