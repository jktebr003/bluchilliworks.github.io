using Fluxor;
using Shared.Models;
using Web.Shared;
using Web.Features.Authentication;

namespace Web.Features.Users;

public class UsersEffects
{
    private readonly WebApiClient _apiClient;
    private readonly IAuthenticationService _authService;
    private readonly IState<UsersState> _usersState;

    public UsersEffects(WebApiClient apiClient, IAuthenticationService authService, IState<UsersState> usersState)
    {
        _apiClient = apiClient;
        _authService = authService;
        _usersState = usersState;
    }

    [EffectMethod]
    public async Task HandleLoadUsers(LoadUsersAction action, IDispatcher dispatcher)
    {
        try
        {
            // Check API connectivity before making the call
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new LoadUsersFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

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
            // Check API connectivity before making the call
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new LoadUserProfileFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

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
            // Check API connectivity before making the call
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new UpdateUserProfileFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

            var result = await _apiClient.Put<UpdateUserRequest, ApiResult<string>>(
                new WebApiClientInfo<UpdateUserRequest>
                {
                    Method = $"/users",
                    Request = action.Request
                }
            );

            if (result?.Success == true && !string.IsNullOrEmpty(result.Value))
            {
                // Fetch the updated user data
                var userResult = await _apiClient.Get<ApiResult<UserResponse>>(
                    new WebApiClientInfo<object>
                    {
                        Method = $"/users/{result.Value}",
                        Request = string.Empty
                    }
                );

                if (userResult?.Success == true && userResult.Value != null)
                {
                    // Update local storage with the updated user data
                    await _authService.UpdateCurrentUserAsync(userResult.Value);
                    
                    dispatcher.Dispatch(new UpdateUserProfileSuccessAction(userResult.Value));
                }
                else
                {
                    dispatcher.Dispatch(new UpdateUserProfileFailedAction(userResult?.Message ?? "Failed to retrieve updated user profile"));
                }
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

    [EffectMethod]
    public async Task HandleLoadUserDetails(LoadUserDetailsAction action, IDispatcher dispatcher)
    {
        try
        {
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new LoadUserDetailsFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

            var result = await _apiClient.Get<ApiResult<UserDetailsResponse>>(
                new WebApiClientInfo<object>
                {
                    Method = $"/users/{action.UserId}/details",
                    Request = $"?requestingUserId={action.RequestingUserId}"
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new LoadUserDetailsSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new LoadUserDetailsFailedAction(result?.Message ?? "Failed to load user details"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadUserDetailsFailedAction($"Error loading user details: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleChangeUserPassword(ChangeUserPasswordAction action, IDispatcher dispatcher)
    {
        try
        {
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new ChangeUserPasswordFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

            var request = new ChangeUserPasswordRequest
            {
                NewPassword = action.NewPassword,
                RequestingUserId = action.RequestingUserId
            };

            var result = await _apiClient.Put<ChangeUserPasswordRequest, ApiResult<string>>(
                new WebApiClientInfo<ChangeUserPasswordRequest>
                {
                    Method = $"/users/{action.UserId}/password",
                    Request = request
                }
            );

            if (result?.Success == true)
            {
                dispatcher.Dispatch(new ChangeUserPasswordSuccessAction(result.Value ?? "Password changed successfully"));
            }
            else
            {
                dispatcher.Dispatch(new ChangeUserPasswordFailedAction(result?.Message ?? "Failed to change password"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ChangeUserPasswordFailedAction($"Error changing password: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleChangeUserRole(ChangeUserRoleAction action, IDispatcher dispatcher)
    {
        try
        {
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new ChangeUserRoleFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

            var request = new ChangeUserRoleRequest
            {
                NewRole = action.NewRole,
                RequestingUserId = action.RequestingUserId
            };

            var result = await _apiClient.Put<ChangeUserRoleRequest, ApiResult<string>>(
                new WebApiClientInfo<ChangeUserRoleRequest>
                {
                    Method = $"/users/{action.UserId}/role",
                    Request = request
                }
            );

            if (result?.Success == true)
            {
                dispatcher.Dispatch(new ChangeUserRoleSuccessAction(result.Value ?? "Role changed successfully"));
                // Reload user details to show updated role
                dispatcher.Dispatch(new LoadUserDetailsAction(action.UserId, action.RequestingUserId));
            }
            else
            {
                dispatcher.Dispatch(new ChangeUserRoleFailedAction(result?.Message ?? "Failed to change role"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new ChangeUserRoleFailedAction($"Error changing role: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleSearchUsers(SearchUsersAction action, IDispatcher dispatcher)
    {
        try
        {
            // Check API connectivity before making the call
            if (!await _apiClient.IsApiHealthyAsync())
            {
                dispatcher.Dispatch(new SearchUsersFailedAction("Unable to connect to the API. Please check your network connection."));
                return;
            }

            // Get current state to access search filters
            var state = await GetCurrentStateAsync();
            
            // Build query string with search filters
            var queryParams = new List<string>
            {
                $"pageSize={action.PageSize}",
                $"pageNumber={action.PageNumber}"
            };

            if (!string.IsNullOrWhiteSpace(state.SearchQuery))
            {
                queryParams.Add($"search={Uri.EscapeDataString(state.SearchQuery)}");
            }

            if (state.RoleFilter.HasValue)
            {
                queryParams.Add($"role={state.RoleFilter.Value}");
            }

            if (!string.IsNullOrWhiteSpace(state.GenderFilter))
            {
                queryParams.Add($"gender={Uri.EscapeDataString(state.GenderFilter)}");
            }

            if (!string.IsNullOrWhiteSpace(state.PackageFilter))
            {
                queryParams.Add($"package={Uri.EscapeDataString(state.PackageFilter)}");
            }

            if (state.EmailVerifiedFilter.HasValue)
            {
                queryParams.Add($"emailVerified={state.EmailVerifiedFilter.Value}");
            }

            if (state.DateOfBirthFrom.HasValue)
            {
                queryParams.Add($"dobFrom={state.DateOfBirthFrom.Value:yyyy-MM-dd}");
            }

            if (state.DateOfBirthTo.HasValue)
            {
                queryParams.Add($"dobTo={state.DateOfBirthTo.Value:yyyy-MM-dd}");
            }

            var queryString = string.Join("&", queryParams);

            var result = await _apiClient.Get<PagedApiResult<List<UserResponse>>>(
                new WebApiClientInfo<object> 
                { 
                    Method = $"/users/search", 
                    Request = $"?{queryString}"
                }
            );

            if (result?.Success == true && result.Value != null)
            {
                dispatcher.Dispatch(new SearchUsersSuccessAction(
                    result.Value,
                    result.TotalItems,
                    result.TotalPages,
                    action.PageNumber,
                    action.PageSize
                ));
            }
            else
            {
                dispatcher.Dispatch(new SearchUsersFailedAction(result?.Message ?? "Failed to search users"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SearchUsersFailedAction($"Error searching users: {ex.Message}"));
        }
    }

    private async Task<UsersState> GetCurrentStateAsync()
    {
        // Return the current state from the injected IState
        return _usersState.Value;
    }
}
