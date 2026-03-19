using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Shared.Services;

public interface ICurrentUserAuthorizationService
{
    Task<bool> IsInRoleAsync(UserType role);
    Task<bool> IsStaffAsync();
}

public sealed class CurrentUserAuthorizationService : ICurrentUserAuthorizationService
{
    private readonly ICurrentUserContext _currentUserContext;

    public CurrentUserAuthorizationService(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public async Task<bool> IsInRoleAsync(UserType role)
    {
        var currentUser = await _currentUserContext.GetCurrentUserAsync();
        return currentUser?.UserRole == role;
    }

    public Task<bool> IsStaffAsync()
    {
        return IsInRoleAsync(UserType.Staff);
    }
}
