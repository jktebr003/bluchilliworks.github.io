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
}
