using System;
using Fluxor;

namespace MudBlazorWeb.Features.Authentication;

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

    [ReducerMethod]
    public static AuthenticationState ReduceRegisterSuccess(AuthenticationState state, RegisterSuccessAction action) =>
        state with { IsBusy = false, ErrorMessage = null };

    [ReducerMethod]
    public static AuthenticationState ReduceRegisterFailed(AuthenticationState state, RegisterFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static AuthenticationState ReduceSetPasswordSuccess(AuthenticationState state, SetPasswordSuccessAction action) =>
        state with { IsBusy = false, ErrorMessage = null };

    [ReducerMethod]
    public static AuthenticationState ReduceSetPasswordFailed(AuthenticationState state, SetPasswordFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static AuthenticationState ReduceResendVerificationSuccess(AuthenticationState state, ResendVerificationSuccessAction action) =>
        state with { IsBusy = false, ErrorMessage = null };

    [ReducerMethod]
    public static AuthenticationState ReduceResendVerificationFailed(AuthenticationState state, ResendVerificationFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static AuthenticationState ReduceForgotPasswordSuccess(AuthenticationState state, ForgotPasswordSuccessAction action) =>
        state with { IsBusy = false, ErrorMessage = null };

    [ReducerMethod]
    public static AuthenticationState ReduceForgotPasswordFailed(AuthenticationState state, ForgotPasswordFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static AuthenticationState ReduceResetPasswordSuccess(AuthenticationState state, ResetPasswordSuccessAction action) =>
        state with { IsBusy = false, ErrorMessage = null };

    [ReducerMethod]
    public static AuthenticationState ReduceResetPasswordFailed(AuthenticationState state, ResetPasswordFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static AuthenticationState ReduceAuthenticationFailed(AuthenticationState state, AuthenticationFailedAction action) =>
        state with { IsBusy = false, ErrorMessage = action.ErrorMessage };
}

