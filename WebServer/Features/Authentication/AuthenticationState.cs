using Fluxor;

using Shared.Models;

namespace WebServer.Features.Authentication;

public record AuthenticationState
{
    public bool IsBusy { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsAuthenticated { get; init; }
    public UserResponse? User { get; init; }
}

public class AuthenticationFeatureState : Feature<AuthenticationState>
{
    public override string GetName() => nameof(AuthenticationState);
    protected override AuthenticationState GetInitialState() => new AuthenticationState { IsAuthenticated = false, IsBusy = false };
}
