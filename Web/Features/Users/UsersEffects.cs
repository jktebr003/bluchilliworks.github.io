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

    [EffectMethod]
    public async Task HandleLoadUserProfile(LoadUserProfileAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Get<ApiResult<UserResponse>>(
                new WebApiClientInfo<object>
                {
                    Method = $"/users/{action.UserId}",
                    Request = string.Empty
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new LoadUserProfileSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new LoadUserProfileFailedAction(result?.Message ?? "Failed to load user profile"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadUserProfileFailedAction($"Error loading user profile: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateUserProfile(UpdateUserProfileAction action, IDispatcher dispatcher)
    {
        try
        {
            var result = await _apiClient.Put<UpdateUserRequest, ApiResult<UserResponse>>(
                new WebApiClientInfo<UpdateUserRequest>
                {
                    Method = $"/users/{action.Request.Id}",
                    Request = action.Request
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new UpdateUserProfileSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new UpdateUserProfileFailedAction(result?.Message ?? "Failed to update user profile"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new UpdateUserProfileFailedAction($"Error updating user profile: {ex.Message}"));
        }
    }
}
