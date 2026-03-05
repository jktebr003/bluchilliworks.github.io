using Blazor.SubtleCrypto;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Shared.Extensions;

namespace WebServer.Features.Authentication;

public class AuthenticationEffects
{
    private readonly IAuthenticationService _authService;
    private readonly NavigationManager _navigationManager;
    private readonly ICryptoService _cryptoService;

    public AuthenticationEffects(IAuthenticationService authService, NavigationManager navigationManager, ICryptoService cryptoService)
    {
        _authService = authService;
        _navigationManager = navigationManager;
        _cryptoService = cryptoService;
    }

    [EffectMethod]
    public async Task HandleLoginAction(LoginAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));
        var result = await _authService.LoginAsync(action.Username, action.Password);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new LoginSuccessAction(result.Succeeded));
            _navigationManager.NavigateTo("/", true); // <-- Redirect after success
        }
        else
        {
            dispatcher.Dispatch(new LoginFailedAction(result.ErrorMessage ?? "Login failed"));
        }
    }

    [EffectMethod]
    public async Task HandleRegisterAction(RegisterAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));

        // No longer need to generate password - it's done during email verification
        var result = await _authService.RegisterAsync(action.FirstName, action.LastName, action.EmailAddress);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new RegisterSuccessAction(result.Succeeded));
            _navigationManager.NavigateTo("/authentication/login", true); // <-- Redirect to login with message to check email
        }
        else
        {
            dispatcher.Dispatch(new RegisterFailedAction(result.ErrorMessage ?? "Registration failed"));
        }
    }

    [EffectMethod]
    public async Task HandleSetPasswordAction(SetPasswordAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));

        var result = await _authService.SetPasswordAsync(action.EmailAddress, action.VerificationToken, action.Password);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new SetPasswordSuccessAction(result.Succeeded));
            _navigationManager.NavigateTo("/authentication/login?verified=true", true); // <-- Redirect to login after password setup
        }
        else
        {
            dispatcher.Dispatch(new SetPasswordFailedAction(result.ErrorMessage ?? "Failed to set password"));
        }
    }

    [EffectMethod]
    public async Task HandleResendVerificationAction(ResendVerificationAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));

        var result = await _authService.ResendVerificationAsync(action.EmailAddress);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new ResendVerificationSuccessAction(result.Succeeded));
        }
        else
        {
            dispatcher.Dispatch(new ResendVerificationFailedAction(result.ErrorMessage ?? "Failed to resend verification"));
        }
    }

    [EffectMethod]
    public async Task HandleForgotPasswordAction(ForgotPasswordAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));

        var result = await _authService.ForgotPasswordAsync(action.EmailAddress);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new ForgotPasswordSuccessAction(result.Succeeded));
        }
        else
        {
            dispatcher.Dispatch(new ForgotPasswordFailedAction(result.ErrorMessage ?? "Failed to send reset email"));
        }
    }

    [EffectMethod]
    public async Task HandleResetPasswordAction(ResetPasswordAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SetBusyAction(true));

        var result = await _authService.ResetPasswordAsync(action.EmailAddress, action.ResetToken, action.NewPassword);
        if (result.Succeeded)
        {
            dispatcher.Dispatch(new ResetPasswordSuccessAction(result.Succeeded));
            _navigationManager.NavigateTo("/authentication/login?reset=success", true);
        }
        else
        {
            dispatcher.Dispatch(new ResetPasswordFailedAction(result.ErrorMessage ?? "Failed to reset password"));
        }
    }
}
