using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal sealed class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserSessionManager _userSessionManager;
    private readonly SessionAuthenticationOptions _sessionOptions;
    private readonly SessionCookieManager _sessionCookieManager;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserSessionManager userSessionManager,
        SessionCookieManager sessionCookieManager,
        IOptions<SessionAuthenticationOptions> sessionOptions)
        : base(options, logger, encoder)
    {
        _userSessionManager = userSessionManager;
        _sessionCookieManager = sessionCookieManager;
        _sessionOptions = sessionOptions.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(_sessionOptions.CookieName, out var rawSessionToken) || string.IsNullOrWhiteSpace(rawSessionToken))
        {
            return AuthenticateResult.NoResult();
        }

        var validation = await _userSessionManager.ValidateSessionTokenAsync(rawSessionToken, markActivity: true, Context.RequestAborted);
        if (!validation.IsAuthenticated || validation.Principal == null)
        {
            _sessionCookieManager.DeleteSessionCookie(Context);
            return AuthenticateResult.Fail(validation.FailureReason ?? "Session authentication failed.");
        }

        return AuthenticateResult.Success(new AuthenticationTicket(validation.Principal, SessionAuthenticationDefaults.AuthenticationScheme));
    }
}