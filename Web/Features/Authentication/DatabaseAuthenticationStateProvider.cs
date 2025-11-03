using HandlebarsDotNet;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

using Shared.Models;

using System.Security.Claims;

using Web.Shared.Helpers;

namespace Web.Features.Authentication;

public class DatabaseAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageHelper _localStorageHelper;

    public DatabaseAuthenticationStateProvider(LocalStorageHelper localStorageHelper)
    {
        _localStorageHelper = localStorageHelper;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Check if we have a user session
        var user = await _localStorageHelper.GetCurrentUserAsync();
        if (user != null)
        {
            var claims = BuildClaims(user);
            principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "DatabaseAuthentication"));
        }

        return new AuthenticationState(principal);
    }

    public async Task NotifyUserAuthenticationAsync(UserResponse user)
    {
        // Store user in session (you might want to use a more secure method)
        await _localStorageHelper.SetCurrentUserAsync(user);

        var claims = BuildClaims(user);
        var identity = new ClaimsIdentity(claims, "DatabaseAuthentication");
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task NotifyUserLogoutAsync()
    {
        // Clear session
        await _localStorageHelper.RemoveCurrentUserAsync();

        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private List<Claim> BuildClaims(UserResponse user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{user.FirstName}"),
            new Claim(ClaimTypes.Surname, $"{user.LastName}"),
            new Claim(ClaimTypes.Email, $"{user.EmailAddress}"),
            new Claim(ClaimTypes.Role, $"{user.UserRole}"),
            new Claim(ClaimTypes.Sid, $"{user.ID}")
        };

        // Add roles as claims
        //foreach (var userRole in user.UserRoles)
        //{
        //    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        //}

        // Add custom claims
        //foreach (var userClaim in user.UserClaims)
        //{
        //    claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
        //}

        return claims;
    }
}
