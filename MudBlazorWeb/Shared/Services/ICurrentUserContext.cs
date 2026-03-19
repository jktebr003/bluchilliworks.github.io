using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Shared.Services;

public interface ICurrentUserContext
{
    Task<UserResponse?> GetCurrentUserAsync();
    Task UpdateCurrentUserAsync(UserResponse user);
}