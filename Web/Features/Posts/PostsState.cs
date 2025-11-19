using Fluxor;
using Shared.Models;

namespace Web.Features.Posts;

public record PostsState
{
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public List<PostResponse> Posts { get; init; } = new();
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

public class PostsFeatureState : Feature<PostsState>
{
    public override string GetName() => nameof(PostsState);
    protected override PostsState GetInitialState() => new PostsState { IsLoading = false };
}
