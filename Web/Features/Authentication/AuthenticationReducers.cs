using Fluxor;

namespace Web.Features.Authentication;

public static class AuthenticationReducers
{
    [ReducerMethod]
    public static AuthenticationState ReduceSetBusy(AuthenticationState state, SetBusyAction action) =>
        state with { IsBusy = action.IsBusy };

    [ReducerMethod]
    public static AuthenticationState ReduceLoginSuccess(AuthenticationState state, LoginSuccessAction action) =>
        state with { IsBusy = false, IsAuthenticated = true, ErrorMessage = null };

    [ReducerMethod]
    public static AuthenticationState ReduceLoginFailed(AuthenticationState state, LoginFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage, IsAuthenticated = false };
}
