using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace nodiWeb.Services;

/// <summary>
/// Prevents ASP.NET Core's authorization middleware from issuing HTTP challenges
/// for Blazor component routes. The [Authorize] attribute on Blazor pages is meant
/// for Blazor's AuthorizeRouteView, not the HTTP pipeline — this handler lets every
/// request reach the Blazor renderer so the interactive circuit can handle auth.
/// </summary>
public class PassthroughAuthorizationHandler : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
        => next(context);
}
