using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users;

public class UsersActions
{
    public record LoadUsersAction(int PageNumber = 1, int PageSize = 10);
    public record LoadUsersSuccessAction(List<UserResponse> Users, int TotalItems, int TotalPages, int PageNumber, int PageSize);
    public record LoadUsersFailedAction(string ErrorMessage);
    public record ChangePageAction(int PageNumber);

    // Search actions
    public record UpdateSearchFiltersAction(
        string? SearchQuery = null,
        UserType? RoleFilter = null,
        string? GenderFilter = null,
        string? PackageFilter = null,
        bool? EmailVerifiedFilter = null,
        DateTime? DateOfBirthFrom = null,
        DateTime? DateOfBirthTo = null
    );
    public record SearchUsersAction(int PageNumber = 1, int PageSize = 10);
    public record SearchUsersSuccessAction(List<UserResponse> Users, int TotalItems, int TotalPages, int PageNumber, int PageSize);
    public record SearchUsersFailedAction(string ErrorMessage);
    public record ClearSearchFiltersAction;

    // Profile actions
    public record LoadUserProfileAction(string UserId);
    public record LoadUserProfileSuccessAction(UserResponse User);
    public record LoadUserProfileFailedAction(string ErrorMessage);
    public record UpdateUserProfileAction(UpdateUserRequest Request);
    public record UpdateUserProfileSuccessAction(UserResponse User);
    public record UpdateUserProfileFailedAction(string ErrorMessage);

    // User details actions (for Staff)
    public record LoadUserDetailsAction(string UserId, string? RequestingUserId);
    public record LoadUserDetailsSuccessAction(UserDetailsResponse User);
    public record LoadUserDetailsFailedAction(string ErrorMessage);
    public record ChangeUserPasswordAction(string UserId, string NewPassword, string? RequestingUserId);
    public record ChangeUserPasswordSuccessAction(string Message);
    public record ChangeUserPasswordFailedAction(string ErrorMessage);
    public record ChangeUserRoleAction(string UserId, UserType NewRole, string? RequestingUserId);
    public record ChangeUserRoleSuccessAction(string Message);
    public record ChangeUserRoleFailedAction(string ErrorMessage);

}
