using Carter;

using MediatR;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.Authentication.Infrastructure;
using MudBlazorWeb.Features.Authentication.UI;

namespace MudBlazorWeb.Features.Authentication.Application;

public sealed class SessionAuthenticationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/session/login", LoginAsync)
            .WithTags("Authentication");

        app.MapPost("api/auth/session/logout", LogoutAsync)
            .WithTags("Authentication");

        app.MapPost("api/auth/session/refresh", RefreshAsync)
            .WithTags("Authentication");
    }

    private static async Task<IResult> LoginAsync(
        SessionLoginRequest request,
        IMediator mediator,
        IAuthenticationUserStore authenticationUserStore,
        IUserSessionManager userSessionManager,
        SessionCookieManager sessionCookieManager,
        HttpContext httpContext)
    {
        var result = await mediator.Send(new LoginCommand.Command(request.Username, request.Password), httpContext.RequestAborted);
        if (!result.Success || result.Value == null)
        {
            return Results.BadRequest(AuthResult.Failure(result.Message ?? "Invalid username or password"));
        }

        if (!Guid.TryParse(result.Value.ID, out var userId))
        {
            return Results.BadRequest(AuthResult.Failure("The authenticated user record is invalid."));
        }

        var user = await authenticationUserStore.GetByIdAsync(userId, httpContext.RequestAborted);
        if (user == null)
        {
            return Results.BadRequest(AuthResult.Failure("The authenticated user could not be loaded."));
        }

        var session = await userSessionManager.CreateSessionAsync(user, httpContext.RequestAborted);
        sessionCookieManager.AppendSessionCookie(httpContext, session.SessionToken, session.AbsoluteExpiresOn);

        return Results.Ok(AuthResult.Success(result.Message ?? "Login successful"));
    }

    private static async Task<IResult> LogoutAsync(
        IUserSessionManager userSessionManager,
        SessionCookieManager sessionCookieManager,
        HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            await userSessionManager.RevokeSessionAsync(httpContext.User, httpContext.RequestAborted);
        }

        sessionCookieManager.DeleteSessionCookie(httpContext);
        return Results.Ok(AuthResult.Success("Logout successful"));
    }

    private static IResult RefreshAsync(HttpContext httpContext)
    {
        return httpContext.User.Identity?.IsAuthenticated == true
            ? Results.NoContent()
            : Results.Unauthorized();
    }
}

public sealed record SessionLoginRequest(string Username, string Password);