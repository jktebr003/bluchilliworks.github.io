using MudBlazorWeb.Features.Users.UI;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

using Xunit;

using static MudBlazorWeb.Features.Users.UI.UsersActions;

namespace MudBlazorWeb.Features.Users.Tests;

public class UsersReducerTests
{
    [Fact]
    public void ReduceLoadUsersAction_ShouldSetLoadingTrue_AndClearError()
    {
        var state = new UsersState
        {
            IsLoading = false,
            ErrorMessage = "old",
            Users = new List<UserResponse> { CreateUser("1", "alice") }
        };

        var result = UsersReducers.ReduceLoadUsersAction(state);

        Assert.NotSame(state, result);
        Assert.True(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Single(result.Users);
    }

    [Fact]
    public void ReduceLoadUsersSuccessAction_ShouldPopulatePagedUsers()
    {
        var users = new List<UserResponse>
        {
            CreateUser("1", "alice"),
            CreateUser("2", "bob")
        };

        var state = new UsersState { IsLoading = true, ErrorMessage = "old" };
        var action = new LoadUsersSuccessAction(users, 20, 4, 2, 5);

        var result = UsersReducers.ReduceLoadUsersSuccessAction(state, action);

        Assert.False(result.IsLoading);
        Assert.Null(result.ErrorMessage);
        Assert.Same(users, result.Users);
        Assert.Equal(20, result.TotalItems);
        Assert.Equal(4, result.TotalPages);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public void ReduceLoadUsersFailedAction_ShouldSetError_AndClearUsers()
    {
        var state = new UsersState
        {
            IsLoading = true,
            Users = new List<UserResponse> { CreateUser("1", "alice") }
        };

        var result = UsersReducers.ReduceLoadUsersFailedAction(state, new LoadUsersFailedAction("failure"));

        Assert.False(result.IsLoading);
        Assert.Equal("failure", result.ErrorMessage);
        Assert.Empty(result.Users);
    }

    [Fact]
    public void ReduceUpdateSearchFiltersAction_ShouldActivateSearch_AndResetPage()
    {
        var state = new UsersState { CurrentPage = 3, IsSearchActive = false };

        var action = new UpdateSearchFiltersAction(
            SearchQuery: "ali",
            RoleFilter: UserType.Customer,
            GenderFilter: "female",
            PackageFilter: "gold",
            EmailVerifiedFilter: true,
            DateOfBirthFrom: new DateTime(1990, 1, 1),
            DateOfBirthTo: new DateTime(2000, 1, 1));

        var result = UsersReducers.ReduceUpdateSearchFiltersAction(state, action);

        Assert.Equal("ali", result.SearchQuery);
        Assert.Equal(UserType.Customer, result.RoleFilter);
        Assert.Equal("female", result.GenderFilter);
        Assert.Equal("gold", result.PackageFilter);
        Assert.True(result.EmailVerifiedFilter);
        Assert.Equal(1, result.CurrentPage);
        Assert.True(result.IsSearchActive);
    }

    [Fact]
    public void ReduceClearSearchFiltersAction_ShouldResetAllFilters_AndDeactivateSearch()
    {
        var state = new UsersState
        {
            SearchQuery = "ali",
            RoleFilter = UserType.Staff,
            GenderFilter = "female",
            PackageFilter = "gold",
            EmailVerifiedFilter = true,
            DateOfBirthFrom = new DateTime(1990, 1, 1),
            DateOfBirthTo = new DateTime(2000, 1, 1),
            IsSearchActive = true,
            CurrentPage = 4
        };

        var result = UsersReducers.ReduceClearSearchFiltersAction(state);

        Assert.Null(result.SearchQuery);
        Assert.Null(result.RoleFilter);
        Assert.Null(result.GenderFilter);
        Assert.Null(result.PackageFilter);
        Assert.Null(result.EmailVerifiedFilter);
        Assert.Null(result.DateOfBirthFrom);
        Assert.Null(result.DateOfBirthTo);
        Assert.False(result.IsSearchActive);
        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public void ProfileReducers_ShouldTrackLoadAndUpdateLifecycle()
    {
        var state = new UsersState
        {
            IsLoadingProfile = false,
            ErrorMessage = "old"
        };

        var loading = UsersReducers.ReduceLoadUserProfileAction(state);
        Assert.True(loading.IsLoadingProfile);
        Assert.Null(loading.ErrorMessage);

        var profile = CreateUser("1", "alice");
        var success = UsersReducers.ReduceLoadUserProfileSuccessAction(loading, new LoadUserProfileSuccessAction(profile));
        Assert.False(success.IsLoadingProfile);
        Assert.Same(profile, success.CurrentUser);
        Assert.Null(success.ErrorMessage);

        var updating = UsersReducers.ReduceUpdateUserProfileAction(success);
        Assert.True(updating.IsLoadingProfile);

        var updated = CreateUser("1", "alice-updated");
        var updatedSuccess = UsersReducers.ReduceUpdateUserProfileSuccessAction(updating, new UpdateUserProfileSuccessAction(updated));
        Assert.False(updatedSuccess.IsLoadingProfile);
        Assert.Same(updated, updatedSuccess.CurrentUser);

        var failed = UsersReducers.ReduceUpdateUserProfileFailedAction(updating, new UpdateUserProfileFailedAction("update failed"));
        Assert.False(failed.IsLoadingProfile);
        Assert.Equal("update failed", failed.ErrorMessage);
    }

    [Fact]
    public void UserDetailsAndRoleReducers_ShouldTrackDetailsLifecycle()
    {
        var state = new UsersState { IsLoadingDetails = false, ErrorMessage = "old" };

        var loadingDetails = UsersReducers.ReduceLoadUserDetailsAction(state);
        Assert.True(loadingDetails.IsLoadingDetails);
        Assert.Null(loadingDetails.ErrorMessage);

        var details = CreateUserDetails("1", "alice");
        var detailsSuccess = UsersReducers.ReduceLoadUserDetailsSuccessAction(loadingDetails, new LoadUserDetailsSuccessAction(details));
        Assert.False(detailsSuccess.IsLoadingDetails);
        Assert.Same(details, detailsSuccess.UserDetails);

        var roleLoading = UsersReducers.ReduceChangeUserRoleAction(detailsSuccess);
        Assert.True(roleLoading.IsLoadingDetails);

        var roleSuccess = UsersReducers.ReduceChangeUserRoleSuccessAction(roleLoading, new ChangeUserRoleSuccessAction("ok"));
        Assert.False(roleSuccess.IsLoadingDetails);
        Assert.Null(roleSuccess.ErrorMessage);

        var roleFailed = UsersReducers.ReduceChangeUserRoleFailedAction(roleLoading, new ChangeUserRoleFailedAction("role failed"));
        Assert.False(roleFailed.IsLoadingDetails);
        Assert.Equal("role failed", roleFailed.ErrorMessage);
    }

    [Fact]
    public void ChangeUserPasswordReducers_ShouldTrackPasswordLifecycle()
    {
        var state = new UsersState { IsLoadingDetails = false, ErrorMessage = "old" };

        var loading = UsersReducers.ReduceChangeUserPasswordAction(state);
        Assert.True(loading.IsLoadingDetails);
        Assert.Null(loading.ErrorMessage);

        var success = UsersReducers.ReduceChangeUserPasswordSuccessAction(loading, new ChangeUserPasswordSuccessAction("ok"));
        Assert.False(success.IsLoadingDetails);
        Assert.Null(success.ErrorMessage);

        var failed = UsersReducers.ReduceChangeUserPasswordFailedAction(loading, new ChangeUserPasswordFailedAction("password failed"));
        Assert.False(failed.IsLoadingDetails);
        Assert.Equal("password failed", failed.ErrorMessage);
    }

    private static UserResponse CreateUser(string id, string username)
    {
        return new UserResponse
        {
            ID = id,
            Username = username,
            EmailAddress = $"{username}@example.com",
            UserRole = UserType.Customer
        };
    }

    private static UserDetailsResponse CreateUserDetails(string id, string username)
    {
        return new UserDetailsResponse
        {
            ID = id,
            Username = username,
            EmailAddress = $"{username}@example.com",
            UserRole = UserType.Customer
        };
    }
}
