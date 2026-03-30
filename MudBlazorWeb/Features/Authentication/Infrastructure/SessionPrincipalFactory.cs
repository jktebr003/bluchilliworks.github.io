using System.Security.Claims;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal static class SessionPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticationUser user, UserSession session)
    {
        var role = ((UserType)user.UserType).ToString();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FirstName),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Email, user.EmailAddress),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Sid, user.Id.ToString()),
            new(ClaimTypes.Role, role),
            new(SessionAuthenticationDefaults.SessionIdClaimType, session.Id.ToString()),
            new(SessionAuthenticationDefaults.UserStateVersionClaimType, session.UserStateVersion),
            new(SessionAuthenticationDefaults.UsernameClaimType, user.Username)
        };

        var identity = new ClaimsIdentity(claims, SessionAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}