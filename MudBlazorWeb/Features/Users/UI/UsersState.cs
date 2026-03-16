using Fluxor;

using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users.UI;

public record UsersState
{
    public bool IsLoading { get; init; }
    public bool IsLoadingProfile { get; init; }
    public bool IsLoadingDetails { get; init; }
    public string? ErrorMessage { get; init; }
    public List<UserResponse> Users { get; init; } = new();
    public UserResponse? CurrentUser { get; init; }
    public UserDetailsResponse? UserDetails { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }

    // Search filter properties
    public string? SearchQuery { get; init; }
    public UserType? RoleFilter { get; init; }
    public string? GenderFilter { get; init; }
    public string? PackageFilter { get; init; }
    public bool? EmailVerifiedFilter { get; init; }
    public DateTime? DateOfBirthFrom { get; init; }
    public DateTime? DateOfBirthTo { get; init; }
    public bool IsSearchActive { get; init; }
}

public class UsersFeatureState : Feature<UsersState>
{
    public override string GetName() => nameof(UsersState);
    protected override UsersState GetInitialState() => new UsersState
    {
        IsLoading = false,
        IsLoadingProfile = false,
        IsLoadingDetails = false
    };
}

