using System.Net;

namespace nodiWeb.Services;

public class AuthTokenHandler(AuthStateProvider authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = authState.CurrentUser?.Token;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new("Bearer", token);

        var response = await base.SendAsync(request, ct);

        // Expired or revoked token — clear the session so AuthorizeRouteView
        // re-evaluates and RedirectToLogin sends the user back to /login.
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            await authState.SetUserAsync(null);

        return response;
    }
}
