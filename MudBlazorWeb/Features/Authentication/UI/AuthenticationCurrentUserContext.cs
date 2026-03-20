using MudBlazorWeb.Shared.Models;
using MudBlazorWeb.Shared.Services;

namespace MudBlazorWeb.Features.Authentication.UI;

public sealed class AuthenticationCurrentUserContext : ICurrentUserContext
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationCurrentUserContext(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<UserResponse?> GetCurrentUserAsync()
    {
        return _authenticationService.GetCurrentUserAsync();
    }

    public Task UpdateCurrentUserAsync(UserResponse user)
    {
        return _authenticationService.UpdateCurrentUserAsync(user);
    }
}