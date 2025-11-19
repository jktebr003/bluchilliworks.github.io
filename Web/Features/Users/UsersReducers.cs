using Fluxor;
using Shared.Models;

namespace Web.Features.Users;

public static class UsersReducers
{
    [ReducerMethod(typeof(LoadUsersAction))]
    public static UsersState ReduceLoadUsersAction(UsersState state)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceLoadUsersSuccessAction(UsersState state, LoadUsersSuccessAction action)
    {
        return state with
        {
            IsLoading = false,
            Users = action.Users,
            TotalItems = action.TotalItems,
            TotalPages = action.TotalPages,
            CurrentPage = action.PageNumber,
            PageSize = action.PageSize,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceLoadUsersFailedAction(UsersState state, LoadUsersFailedAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage,
            Users = new List<UserResponse>()
        };
    }

    // Profile Reducers
    [ReducerMethod(typeof(LoadUserProfileAction))]
    public static UsersState ReduceLoadUserProfileAction(UsersState state)
    {
        return state with
        {
            IsLoadingProfile = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceLoadUserProfileSuccessAction(UsersState state, LoadUserProfileSuccessAction action)
    {
        return state with
        {
            IsLoadingProfile = false,
            CurrentUser = action.User,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceLoadUserProfileFailedAction(UsersState state, LoadUserProfileFailedAction action)
    {
        return state with
        {
            IsLoadingProfile = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    [ReducerMethod(typeof(UpdateUserProfileAction))]
    public static UsersState ReduceUpdateUserProfileAction(UsersState state)
    {
        return state with
        {
            IsLoadingProfile = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceUpdateUserProfileSuccessAction(UsersState state, UpdateUserProfileSuccessAction action)
    {
        return state with
        {
            IsLoadingProfile = false,
            CurrentUser = action.User,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceUpdateUserProfileFailedAction(UsersState state, UpdateUserProfileFailedAction action)
    {
        return state with
        {
            IsLoadingProfile = false,
            ErrorMessage = action.ErrorMessage
        };
    }
}
