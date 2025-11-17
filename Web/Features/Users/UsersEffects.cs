using Fluxor;
using Shared.Models;
using Web.Shared;

namespace Web.Features.Users;

public class UsersEffects
{
    private readonly WebApiClient _apiClient;

    public UsersEffects(WebApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [EffectMethod]
    public async Task HandleLoadUsers(LoadUsersAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Get<PagedApiResult<List<UserResponse>>>(
                new WebApiClientInfo<object> 
                { 
                    Method = $"/users", 
                    Request = $"?pageSize={action.PageSize}&pageNumber={action.PageNumber}"
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new LoadUsersSuccessAction(
                    result.Value,
                    result.TotalItems,
                    result.TotalPages,
                    action.PageNumber,
                    action.PageSize
                ));
            }
            else
            {
                dispatcher.Dispatch(new LoadUsersFailedAction(result?.Message ?? "Failed to load users"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadUsersFailedAction($"Error loading users: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleChangePage(ChangePageAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadUsersAction(action.PageNumber));
    }
}
