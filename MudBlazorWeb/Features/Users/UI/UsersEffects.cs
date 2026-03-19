using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Users.Application;
using MudBlazorWeb.Shared.Models;
using MudBlazorWeb.Shared.Services;

using static MudBlazorWeb.Features.Users.UI.UsersActions;

namespace MudBlazorWeb.Features.Users.UI;

public class UsersEffects
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IState<UsersState> _usersState;

    public UsersEffects(IMediator mediator, ICurrentUserContext currentUserContext, IState<UsersState> usersState)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
        _usersState = usersState;
    }

    [EffectMethod]
    public async Task HandleLoadUsers(LoadUsersAction action, IDispatcher dispatcher)
    {
        try
        {
            await Task.Yield();

            var result = await _mediator.Send(new GetUsersQuery.Query(action.PageSize, action.PageNumber));

            if (result.Success && result.Value != null)
            {
                var users = result.Value.Select(MapToUserResponse).ToList();

                dispatcher.Dispatch(new LoadUsersSuccessAction(
                    users,
                    result.TotalItems,
                    result.TotalPages,
                    result.PageNumber ?? action.PageNumber,
                    result.PageSize ?? action.PageSize
                ));
            }
            else
            {
                dispatcher.Dispatch(new LoadUsersFailedAction(result.Message ?? "Failed to load users"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadUsersFailedAction($"Error loading users: {ex.Message}"));
        }
    }

    [EffectMethod]
    public Task HandleChangePage(ChangePageAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadUsersAction(action.PageNumber));
        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task HandleLoadUserProfile(LoadUserProfileAction action, IDispatcher dispatcher)
    {
        try
        {
            await Task.Yield();

            if (!Guid.TryParse(action.UserId, out var userId))
            {
                dispatcher.Dispatch(new LoadUserProfileFailedAction("Invalid user ID."));
                return;
            }

            var result = await _mediator.Send(new GetUserByIdQuery.Query(userId));

            if (result.Success && result.Value != null)
            {
                dispatcher.Dispatch(new LoadUserProfileSuccessAction(MapToUserResponse(result.Value)));
            }
            else
            {
                dispatcher.Dispatch(new LoadUserProfileFailedAction(result.Message ?? "Failed to load user profile"));
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
            await Task.Yield();

            var result = await _mediator.Send(new UpdateUserCommand.Command(action.Request));

            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                if (!Guid.TryParse(result.Value, out var updatedUserId))
                {
                    dispatcher.Dispatch(new UpdateUserProfileFailedAction("Failed to parse updated user identifier."));
                    return;
                }

                var userResult = await _mediator.Send(new GetUserByIdQuery.Query(updatedUserId));

                if (userResult.Success && userResult.Value != null)
                {
                    var updatedUser = MapToUserResponse(userResult.Value);
                    await _currentUserContext.UpdateCurrentUserAsync(updatedUser);

                    dispatcher.Dispatch(new UpdateUserProfileSuccessAction(updatedUser));
                }
                else
                {
                    dispatcher.Dispatch(new UpdateUserProfileFailedAction(userResult.Message ?? "Failed to retrieve updated user profile"));
                }
            }
            else
            {
                dispatcher.Dispatch(new UpdateUserProfileFailedAction(result.Message ?? "Failed to update user profile"));
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
            await Task.Yield();

            if (!Guid.TryParse(action.UserId, out var userId))
            {
                dispatcher.Dispatch(new LoadUserDetailsFailedAction("Invalid user ID."));
                return;
            }

            Guid? requestingUserId = null;
            if (!string.IsNullOrWhiteSpace(action.RequestingUserId) && Guid.TryParse(action.RequestingUserId, out var parsedRequestingUserId))
            {
                requestingUserId = parsedRequestingUserId;
            }

            var result = await _mediator.Send(new GetUserDetailsQuery.Query(userId, requestingUserId));

            if (result.Success && result.Value != null)
            {
                dispatcher.Dispatch(new LoadUserDetailsSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new LoadUserDetailsFailedAction(result.Message ?? "Failed to load user details"));
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
            await Task.Yield();

            if (!Guid.TryParse(action.UserId, out var userId))
            {
                dispatcher.Dispatch(new ChangeUserPasswordFailedAction("Invalid user ID."));
                return;
            }

            Guid? requestingUserId = null;
            if (!string.IsNullOrWhiteSpace(action.RequestingUserId) && Guid.TryParse(action.RequestingUserId, out var parsedRequestingUserId))
            {
                requestingUserId = parsedRequestingUserId;
            }

            var result = await _mediator.Send(new ChangeUserPasswordCommand.Command(userId, action.NewPassword, requestingUserId));

            if (result.Success)
            {
                dispatcher.Dispatch(new ChangeUserPasswordSuccessAction(result.Value ?? "Password changed successfully"));
            }
            else
            {
                dispatcher.Dispatch(new ChangeUserPasswordFailedAction(result.Message ?? "Failed to change password"));
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
            await Task.Yield();

            if (!Guid.TryParse(action.UserId, out var userId))
            {
                dispatcher.Dispatch(new ChangeUserRoleFailedAction("Invalid user ID."));
                return;
            }

            Guid? requestingUserId = null;
            if (!string.IsNullOrWhiteSpace(action.RequestingUserId) && Guid.TryParse(action.RequestingUserId, out var parsedRequestingUserId))
            {
                requestingUserId = parsedRequestingUserId;
            }

            var result = await _mediator.Send(new ChangeUserRoleCommand.Command(userId, action.NewRole, requestingUserId));

            if (result.Success)
            {
                dispatcher.Dispatch(new ChangeUserRoleSuccessAction(result.Value ?? "Role changed successfully"));
                dispatcher.Dispatch(new LoadUserDetailsAction(action.UserId, action.RequestingUserId));
            }
            else
            {
                dispatcher.Dispatch(new ChangeUserRoleFailedAction(result.Message ?? "Failed to change role"));
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
            await Task.Yield();

            var state = _usersState.Value;

            var result = await _mediator.Send(new SearchUserQuery.Query(
                state.SearchQuery,
                state.RoleFilter,
                state.GenderFilter,
                state.PackageFilter,
                state.EmailVerifiedFilter,
                state.DateOfBirthFrom,
                state.DateOfBirthTo,
                action.PageSize,
                action.PageNumber));

            if (result.Success && result.Value != null)
            {
                var users = result.Value.Select(MapToUserResponse).ToList();

                dispatcher.Dispatch(new SearchUsersSuccessAction(
                    users,
                    result.TotalItems,
                    result.TotalPages,
                    result.PageNumber ?? action.PageNumber,
                    result.PageSize ?? action.PageSize
                ));
            }
            else
            {
                dispatcher.Dispatch(new SearchUsersFailedAction(result.Message ?? "Failed to search users"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new SearchUsersFailedAction($"Error searching users: {ex.Message}"));
        }
    }

    private static UserResponse MapToUserResponse(GetUsersQuery.UserDto dto)
    {
        return new UserResponse
        {
            ID = dto.Id.ToString(),
            Name = dto.Name,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            EmailAddress = dto.EmailAddress,
            TelephoneNumber = dto.TelephoneNumber,
            MobileNumber = dto.MobileNumber,
            EmailVerified = dto.EmailVerified,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            UserRole = dto.UserRole,
            Skills = dto.Skills,
            Hobbies = dto.Hobbies,
            CreatedOn = dto.CreatedOn.ToString("O"),
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn?.ToString("O"),
            ModifiedBy = dto.ModifiedBy,
            DeletedOn = dto.DeletedOn?.ToString("O"),
            DeletedBy = dto.DeletedBy,
            IsDeleted = dto.IsDeleted
        };
    }

    private static UserResponse MapToUserResponse(SearchUserQuery.UserDto dto)
    {
        return new UserResponse
        {
            ID = dto.Id.ToString(),
            Name = dto.Name,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            EmailAddress = dto.EmailAddress,
            TelephoneNumber = dto.TelephoneNumber,
            MobileNumber = dto.MobileNumber,
            EmailVerified = dto.EmailVerified,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            UserRole = dto.UserRole,
            Skills = dto.Skills,
            Hobbies = dto.Hobbies,
            CreatedOn = dto.CreatedOn.ToString("O"),
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn?.ToString("O"),
            ModifiedBy = dto.ModifiedBy,
            DeletedOn = dto.DeletedOn?.ToString("O"),
            DeletedBy = dto.DeletedBy,
            IsDeleted = dto.IsDeleted
        };
    }

    private static UserResponse MapToUserResponse(GetUserByIdQuery.UserDto dto)
    {
        return new UserResponse
        {
            ID = dto.Id.ToString(),
            Name = dto.Name,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            EmailAddress = dto.EmailAddress,
            TelephoneNumber = dto.TelephoneNumber,
            MobileNumber = dto.MobileNumber,
            EmailVerified = dto.EmailVerified,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            UserRole = dto.UserRole,
            Skills = dto.Skills,
            Hobbies = dto.Hobbies,
            CreatedOn = dto.CreatedOn.ToString("O"),
            CreatedBy = dto.CreatedBy,
            ModifiedOn = dto.ModifiedOn?.ToString("O"),
            ModifiedBy = dto.ModifiedBy,
            DeletedOn = dto.DeletedOn?.ToString("O"),
            DeletedBy = dto.DeletedBy,
            IsDeleted = dto.IsDeleted
        };
    }
}

