using Shared.Models;

namespace Web.Features.Posts;

public record LoadPostsAction(int PageNumber = 1, int PageSize = 10);
public record LoadPostsSuccessAction(List<PostResponse> Posts, int TotalItems, int TotalPages, int PageNumber, int PageSize);
public record LoadPostsFailedAction(string ErrorMessage);
public record ChangePageAction(int PageNumber);
