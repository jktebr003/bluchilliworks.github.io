using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal sealed class SessionCookieManager
{
    private readonly SessionAuthenticationOptions _options;

    public SessionCookieManager(IOptions<SessionAuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public void AppendSessionCookie(HttpContext httpContext, string sessionToken, DateTimeOffset absoluteExpiresOn)
    {
        httpContext.Response.Cookies.Append(
            _options.CookieName,
            sessionToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = absoluteExpiresOn,
                Path = "/"
            });
    }

    public void DeleteSessionCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(
            _options.CookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Path = "/"
            });
    }
}