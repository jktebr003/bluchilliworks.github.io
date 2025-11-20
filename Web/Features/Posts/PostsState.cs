using Fluxor;
using Shared.Models;

namespace Web.Features.Posts;

public record PostsState
{
    public bool IsLoading { get; init; }
    public bool IsLoadingDetail { get; init; }
    public bool IsUpdating { get; init; }
    public bool IsCreating { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DetailErrorMessage { get; init; }
    public string? UpdateErrorMessage { get; init; }
    public string? CreateErrorMessage { get; init; }
    public List<PostResponse> Posts { get; init; } = new();
    public PostResponse? CurrentPost { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

public class PostsFeatureState : Feature<PostsState>
{
    public override string GetName() => nameof(PostsState);
    protected override PostsState GetInitialState() => new PostsState 
    { 
        IsLoading = false, 
        IsLoadingDetail = false,
        IsUpdating = false,
        IsCreating = false
    };
}
