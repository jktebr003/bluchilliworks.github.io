using Fluxor;
using Shared.Models;

namespace Web.Features.Users;

public record UsersState
{
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public List<UserResponse> Users { get; init; } = new();
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

public class UsersFeatureState : Feature<UsersState>
{
    public override string GetName() => nameof(UsersState);
    protected override UsersState GetInitialState() => new UsersState { IsLoading = false };
}
