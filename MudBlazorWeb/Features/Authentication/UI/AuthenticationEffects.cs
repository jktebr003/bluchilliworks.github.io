using System;

using Fluxor;
using Microsoft.AspNetCore.Components;

namespace MudBlazorWeb.Features.Authentication.UI;

public class AuthenticationEffects
{
	private readonly IAuthenticationService _authenticationService;
	private readonly NavigationManager _navigationManager;

	public AuthenticationEffects(IAuthenticationService authenticationService, NavigationManager navigationManager)
	{
		_authenticationService = authenticationService;
		_navigationManager = navigationManager;
	}

	[EffectMethod]
	public Task HandleLogin(LoginAction action, IDispatcher dispatcher)
	{
		return ExecuteAsync(
			dispatcher,
			() => _authenticationService.LoginAsync(action.Username, action.Password),
			() =>
			{
				dispatcher.Dispatch(new LoginSuccessAction(true));
				_navigationManager.NavigateTo("/");
			},
			errorMessage => dispatcher.Dispatch(new LoginFailedAction(errorMessage)),
			"Login failed. Please try again.");
	}

	[EffectMethod]
	public Task HandleRegister(RegisterAction action, IDispatcher dispatcher)
	{
		return ExecuteAsync(
			dispatcher,
			() => _authenticationService.RegisterAsync(action.FirstName, action.LastName, action.EmailAddress),
			() =>
			{
				dispatcher.Dispatch(new RegisterSuccessAction(true));
				_navigationManager.NavigateTo($"/authentication/setup-password?email={Uri.EscapeDataString(action.EmailAddress)}");
			},
			errorMessage => dispatcher.Dispatch(new RegisterFailedAction(errorMessage)),
			"Registration failed. Please try again.");
	}

	[EffectMethod]
	public Task HandleSetPassword(SetPasswordAction action, IDispatcher dispatcher)
	{
		return ExecuteAsync(
			dispatcher,
			() => _authenticationService.SetPasswordAsync(action.EmailAddress, action.VerificationToken, action.Password),
			() =>
			{
				dispatcher.Dispatch(new SetPasswordSuccessAction(true));
				_navigationManager.NavigateTo("/authentication/login");
			},
			errorMessage => dispatcher.Dispatch(new SetPasswordFailedAction(errorMessage)),
			"Failed to set password. Please try again.");
	}

	[EffectMethod]
	public Task HandleResendVerification(ResendVerificationAction action, IDispatcher dispatcher)
	{
		return ExecuteAsync(
			dispatcher,
			() => _authenticationService.ResendVerificationAsync(action.EmailAddress),
			() => dispatcher.Dispatch(new ResendVerificationSuccessAction(true)),
			errorMessage => dispatcher.Dispatch(new ResendVerificationFailedAction(errorMessage)),
			"Failed to resend verification email. Please try again.");
	}

	[EffectMethod]
	public Task HandleForgotPassword(ForgotPasswordAction action, IDispatcher dispatcher)
	{
		return ExecuteAsync(
			dispatcher,
			() => _authenticationService.ForgotPasswordAsync(action.EmailAddress),
			() => dispatcher.Dispatch(new ForgotPasswordSuccessAction(true)),
			errorMessage => dispatcher.Dispatch(new ForgotPasswordFailedAction(errorMessage)),
			"Failed to process forgot password request. Please try again.");
	}

	[EffectMethod]
	public Task HandleResetPassword(ResetPasswordAction action, IDispatcher dispatcher)
	{
		return ExecuteAsync(
			dispatcher,
			() => _authenticationService.ResetPasswordAsync(action.EmailAddress, action.ResetToken, action.NewPassword),
			() =>
			{
				dispatcher.Dispatch(new ResetPasswordSuccessAction(true));
				_navigationManager.NavigateTo("/authentication/login");
			},
			errorMessage => dispatcher.Dispatch(new ResetPasswordFailedAction(errorMessage)),
			"Failed to reset password. Please try again.");
	}

	private static async Task ExecuteAsync(
		IDispatcher dispatcher,
		Func<Task<AuthResult>> operation,
		Action onSuccess,
		Action<string> onFailure,
		string fallbackErrorMessage)
	{
		try
		{
			dispatcher.Dispatch(new SetBusyAction(true));

			var result = await operation();

			if (result.Succeeded)
			{
				onSuccess();
				return;
			}

			var errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage)
				? fallbackErrorMessage
				: result.ErrorMessage;

			onFailure(errorMessage);
		}
		catch (Exception ex)
		{
			onFailure($"{fallbackErrorMessage} {ex.Message}");
		}
	}

}
