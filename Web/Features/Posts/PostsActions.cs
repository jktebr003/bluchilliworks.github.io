using Shared.Models;

namespace Web.Features.Posts;

public record LoadPostsAction(int PageNumber = 1, int PageSize = 10);
public record LoadPostsSuccessAction(List<PostResponse> Posts, int TotalItems, int TotalPages, int PageNumber, int PageSize);
public record LoadPostsFailedAction(string ErrorMessage);
public record ChangePageAction(int PageNumber);

// Single post actions
public record LoadPostDetailAction(string PostId);
public record LoadPostDetailSuccessAction(PostResponse Post);
public record LoadPostDetailFailedAction(string ErrorMessage);

// Update post actions
public record UpdatePostAction(UpdatePostRequest Request);
public record UpdatePostSuccessAction(PostResponse Post);
public record UpdatePostFailedAction(string ErrorMessage);
