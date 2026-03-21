namespace nodiWeb.Services;

public class AuthTokenHandler(AuthStateProvider authState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = authState.CurrentUser?.Token;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
