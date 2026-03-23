using MudBlazorWeb.Features.Authentication.UI;

using Xunit;

namespace MudBlazorWeb.Features.Authentication.Tests;

public class AuthenticationReducerTests
{
    [Fact]
    public void ReduceSetBusy_ShouldUpdateBusyFlagOnly()
    {
        var state = new AuthenticationState
        {
            IsBusy = false,
            IsAuthenticated = true,
            ErrorMessage = "old"
        };

        var result = AuthenticationReducers.ReduceSetBusy(state, new SetBusyAction(true));

        Assert.NotSame(state, result);
        Assert.True(result.IsBusy);
        Assert.True(result.IsAuthenticated);
        Assert.Equal("old", result.ErrorMessage);
    }

    [Fact]
    public void ReduceLoginSuccess_ShouldAuthenticate_AndClearError()
    {
        var state = new AuthenticationState
        {
            IsBusy = true,
            IsAuthenticated = false,
            ErrorMessage = "login error"
        };

        var result = AuthenticationReducers.ReduceLoginSuccess(state, new LoginSuccessAction(true));

        Assert.False(result.IsBusy);
        Assert.True(result.IsAuthenticated);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ReduceLoginFailed_ShouldClearBusy_AndSetError()
    {
        var state = new AuthenticationState
        {
            IsBusy = true,
            IsAuthenticated = true,
            ErrorMessage = null
        };

        var result = AuthenticationReducers.ReduceLoginFailed(state, new LoginFailedAction("invalid credentials"));

        Assert.False(result.IsBusy);
        Assert.False(result.IsAuthenticated);
        Assert.Equal("invalid credentials", result.ErrorMessage);
    }

    [Fact]
    public void RegisterReducers_ShouldTrackSuccessAndFailure()
    {
        var state = new AuthenticationState { IsBusy = true, ErrorMessage = "old" };

        var success = AuthenticationReducers.ReduceRegisterSuccess(state, new RegisterSuccessAction(true));
        Assert.False(success.IsBusy);
        Assert.Null(success.ErrorMessage);

        var failed = AuthenticationReducers.ReduceRegisterFailed(state, new RegisterFailedAction("registration failed"));
        Assert.False(failed.IsBusy);
        Assert.Equal("registration failed", failed.ErrorMessage);
    }

    [Fact]
    public void PasswordLifecycleReducers_ShouldTrackSetForgotReset()
    {
        var state = new AuthenticationState { IsBusy = true, ErrorMessage = "old" };

        var setSuccess = AuthenticationReducers.ReduceSetPasswordSuccess(state, new SetPasswordSuccessAction(true));
        Assert.False(setSuccess.IsBusy);
        Assert.Null(setSuccess.ErrorMessage);

        var forgotFailed = AuthenticationReducers.ReduceForgotPasswordFailed(state, new ForgotPasswordFailedAction("forgot failed"));
        Assert.False(forgotFailed.IsBusy);
        Assert.Equal("forgot failed", forgotFailed.ErrorMessage);

        var resetSuccess = AuthenticationReducers.ReduceResetPasswordSuccess(state, new ResetPasswordSuccessAction(true));
        Assert.False(resetSuccess.IsBusy);
        Assert.Null(resetSuccess.ErrorMessage);
    }

    [Fact]
    public void ReduceAuthenticationFailed_ShouldClearBusyAndSetError()
    {
        var state = new AuthenticationState { IsBusy = true, ErrorMessage = null };

        var result = AuthenticationReducers.ReduceAuthenticationFailed(state, new AuthenticationFailedAction("unexpected"));

        Assert.False(result.IsBusy);
        Assert.Equal("unexpected", result.ErrorMessage);
    }
}
