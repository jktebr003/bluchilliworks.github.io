using Shared.Models;

namespace Web.Features.Users;

public record LoadUsersAction(int PageNumber = 1, int PageSize = 10);
public record LoadUsersSuccessAction(List<UserResponse> Users, int TotalItems, int TotalPages, int PageNumber, int PageSize);
public record LoadUsersFailedAction(string ErrorMessage);
public record ChangePageAction(int PageNumber);
