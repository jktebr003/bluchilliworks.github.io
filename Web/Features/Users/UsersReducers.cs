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

    // User Details Reducers (for Staff)
    [ReducerMethod(typeof(LoadUserDetailsAction))]
    public static UsersState ReduceLoadUserDetailsAction(UsersState state)
    {
        return state with
        {
            IsLoadingDetails = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceLoadUserDetailsSuccessAction(UsersState state, LoadUserDetailsSuccessAction action)
    {
        return state with
        {
            IsLoadingDetails = false,
            UserDetails = action.User,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceLoadUserDetailsFailedAction(UsersState state, LoadUserDetailsFailedAction action)
    {
        return state with
        {
            IsLoadingDetails = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    [ReducerMethod(typeof(ChangeUserPasswordAction))]
    public static UsersState ReduceChangeUserPasswordAction(UsersState state)
    {
        return state with
        {
            IsLoadingDetails = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceChangeUserPasswordSuccessAction(UsersState state, ChangeUserPasswordSuccessAction action)
    {
        return state with
        {
            IsLoadingDetails = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceChangeUserPasswordFailedAction(UsersState state, ChangeUserPasswordFailedAction action)
    {
        return state with
        {
            IsLoadingDetails = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    [ReducerMethod(typeof(ChangeUserRoleAction))]
    public static UsersState ReduceChangeUserRoleAction(UsersState state)
    {
        return state with
        {
            IsLoadingDetails = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceChangeUserRoleSuccessAction(UsersState state, ChangeUserRoleSuccessAction action)
    {
        return state with
        {
            IsLoadingDetails = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceChangeUserRoleFailedAction(UsersState state, ChangeUserRoleFailedAction action)
    {
        return state with
        {
            IsLoadingDetails = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    // Search Reducers
    [ReducerMethod]
    public static UsersState ReduceUpdateSearchFiltersAction(UsersState state, UpdateSearchFiltersAction action)
    {
        return state with
        {
            SearchQuery = action.SearchQuery,
            RoleFilter = action.RoleFilter,
            GenderFilter = action.GenderFilter,
            PackageFilter = action.PackageFilter,
            EmailVerifiedFilter = action.EmailVerifiedFilter,
            DateOfBirthFrom = action.DateOfBirthFrom,
            DateOfBirthTo = action.DateOfBirthTo,
            IsSearchActive = !string.IsNullOrWhiteSpace(action.SearchQuery) ||
                           action.RoleFilter.HasValue ||
                           !string.IsNullOrWhiteSpace(action.GenderFilter) ||
                           !string.IsNullOrWhiteSpace(action.PackageFilter) ||
                           action.EmailVerifiedFilter.HasValue ||
                           action.DateOfBirthFrom.HasValue ||
                           action.DateOfBirthTo.HasValue,
            CurrentPage = 1 // Reset to first page when filters change
        };
    }

    [ReducerMethod(typeof(SearchUsersAction))]
    public static UsersState ReduceSearchUsersAction(UsersState state)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static UsersState ReduceSearchUsersSuccessAction(UsersState state, SearchUsersSuccessAction action)
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
    public static UsersState ReduceSearchUsersFailedAction(UsersState state, SearchUsersFailedAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage,
            Users = new List<UserResponse>()
        };
    }

    [ReducerMethod(typeof(ClearSearchFiltersAction))]
    public static UsersState ReduceClearSearchFiltersAction(UsersState state)
    {
        return state with
        {
            SearchQuery = null,
            RoleFilter = null,
            GenderFilter = null,
            PackageFilter = null,
            EmailVerifiedFilter = null,
            DateOfBirthFrom = null,
            DateOfBirthTo = null,
            IsSearchActive = false,
            CurrentPage = 1
        };
    }
}
