using System;

using MediatR;

using Microsoft.AspNetCore.Components.Authorization;

using MudBlazorWeb.Features.Authentication.Application;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Features.Authentication.UI;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Helpers;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Authentication;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress);
    Task<AuthResult> SetPasswordAsync(string emailAddress, string verificationToken, string password);
    Task<AuthResult> ResendVerificationAsync(string emailAddress);
    Task<AuthResult> ForgotPasswordAsync(string emailAddress);
    Task<AuthResult> ResetPasswordAsync(string emailAddress, string resetToken, string newPassword);
    Task<UserResponse?> GetCurrentUserAsync();
    Task UpdateCurrentUserAsync(UserResponse user);
    Task<bool> IsUserInRoleAsync(string role);
    Task<bool> HasClaimAsync(string claimType, string claimValue);
}


public class AuthenticationService : IAuthenticationService
{
    private readonly IMediator _mediator;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly LocalStorageHelper _localStorageHelper;   

    public AuthenticationService(IMediator mediator, AuthenticationStateProvider authenticationStateProvider, LocalStorageHelper localStorageHelper)
    {
        _mediator = mediator;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorageHelper = localStorageHelper;
    }

    public async Task<UserResponse?> GetCurrentUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity == null || !user.Identity.IsAuthenticated)
            return null;

        var username = user.Identity.Name;
        return await _localStorageHelper.GetCurrentUserAsync();
    }

    public async Task UpdateCurrentUserAsync(UserResponse user)
    {
        await _localStorageHelper.SetCurrentUserAsync(user);
        // Notify authentication state provider to refresh the authentication state
        await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthenticationAsync(user);
    }

    public Task<bool> HasClaimAsync(string claimType, string claimValue)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserInRoleAsync(string role)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var result = await _mediator.Send(new LoginCommand.Command(username, password));

        if (result.Success && result.Value != null)
        {
            await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthenticationAsync(result.Value);
            return AuthResult.Success(result.Message);
        }

        return AuthResult.Failure(result.Message ?? "Invalid username or password");
    }

    public async Task LogoutAsync()
    {
        await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogoutAsync();
    }

    public async Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress)
    {
        var result = await _mediator.Send(new RegisterCommand.Command(firstName, lastName, emailAddress));
        return ToAuthResult(result, "Registration successful! Please check your email to verify your account.", "Registration failed");
    }

    public async Task<AuthResult> SetPasswordAsync(string emailAddress, string verificationToken, string password)
    {
        var result = await _mediator.Send(new SetPasswordCommand.Command(emailAddress, verificationToken, password));
        return ToAuthResult(result, "Password set successfully! You can now log in.", "Failed to set password");
    }

    public async Task<AuthResult> ResendVerificationAsync(string emailAddress)
    {
        var result = await _mediator.Send(new ResendVerificationCommand.Command(emailAddress));
        return ToAuthResult(result, "Verification email sent!", "Failed to send verification email");
    }

    public async Task<AuthResult> ForgotPasswordAsync(string emailAddress)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand.Command(emailAddress));
        return ToAuthResult(result, "If an account with that email exists, you will receive a password reset link.", "Failed to send password reset email");
    }

    public async Task<AuthResult> ResetPasswordAsync(string emailAddress, string resetToken, string newPassword)
    {
        var result = await _mediator.Send(new ResetPasswordCommand.Command(emailAddress, resetToken, newPassword));
        return ToAuthResult(result, "Your password has been reset successfully. You can now log in with your new password.", "Failed to reset password");
    }

    private static AuthResult ToAuthResult(Result<string> result, string successFallback, string errorFallback)
    {
        return result.Success
            ? AuthResult.Success(result.Message ?? successFallback)
            : AuthResult.Failure(result.Message ?? errorFallback);
    }
}

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }

    public static AuthResult Success(string? message = null) => new AuthResult { Succeeded = true, SuccessMessage = message };
    public static AuthResult Failure(string errorMessage) => new AuthResult { Succeeded = false, ErrorMessage = errorMessage };
}


