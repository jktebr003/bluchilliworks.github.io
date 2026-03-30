using System;
using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using MudBlazorWeb.Features.Authentication.Application;
using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.Authentication.Infrastructure;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Authentication.UI;

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
    private readonly IAuthenticationUserStore _authenticationUserStore;
    private readonly IJSRuntime _jsRuntime;

    public AuthenticationService(
        IMediator mediator,
        AuthenticationStateProvider authenticationStateProvider,
        IAuthenticationUserStore authenticationUserStore,
        IJSRuntime jsRuntime)
    {
        _mediator = mediator;
        _authenticationStateProvider = authenticationStateProvider;
        _authenticationUserStore = authenticationUserStore;
        _jsRuntime = jsRuntime;
    }

    public async Task<UserResponse?> GetCurrentUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return null;
        }

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return BuildUserResponseFromClaims(principal);
        }

        var user = await _authenticationUserStore.GetByIdAsync(userId);
        return user == null ? BuildUserResponseFromClaims(principal) : AuthenticationCommandHelpers.MapToUserResponse(user);
    }

    public async Task UpdateCurrentUserAsync(UserResponse user)
    {
        var currentState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (currentState.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (_authenticationStateProvider is not IHostEnvironmentAuthenticationStateProvider hostAuthenticationStateProvider)
        {
            return;
        }

        var refreshedPrincipal = BuildPrincipal(user, currentState.User);
        hostAuthenticationStateProvider.SetAuthenticationState(
            Task.FromResult(new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(refreshedPrincipal)));
    }

    public async Task<bool> HasClaimAsync(string claimType, string claimValue)
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.HasClaim(claimType, claimValue);
    }

    public async Task<bool> IsUserInRoleAsync(string role)
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.IsInRole(role);
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<BrowserAuthResult>(
                "sessionAuth.login",
                "/api/auth/session/login",
                username,
                password);

            return result.ToAuthResult();
        }
        catch (JSException ex)
        {
            return AuthResult.Failure($"Login failed. {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sessionAuth.logout", "/api/auth/session/logout");
        }
        finally
        {
            if (_authenticationStateProvider is IHostEnvironmentAuthenticationStateProvider hostAuthenticationStateProvider)
            {
                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                hostAuthenticationStateProvider.SetAuthenticationState(
                    Task.FromResult(new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(anonymous)));
            }
        }
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

    private static UserResponse? BuildUserResponseFromClaims(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return new UserResponse
        {
            ID = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(ClaimTypes.Sid) ?? string.Empty,
            FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.Identity?.Name,
            LastName = principal.FindFirstValue(ClaimTypes.Surname),
            EmailAddress = principal.FindFirstValue(ClaimTypes.Email),
            Username = principal.FindFirstValue(SessionAuthenticationDefaults.UsernameClaimType),
            CreatedBy = string.Empty,
            CreatedOn = string.Empty
        };
    }

    private static ClaimsPrincipal BuildPrincipal(UserResponse user, ClaimsPrincipal currentPrincipal)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new(ClaimTypes.Email, user.EmailAddress ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.ID ?? string.Empty),
            new(ClaimTypes.Sid, user.ID ?? string.Empty),
            new(ClaimTypes.Role, user.UserRole.ToString()),
            new(SessionAuthenticationDefaults.UsernameClaimType, user.Username ?? string.Empty)
        };

        CopyClaimIfPresent(currentPrincipal, claims, SessionAuthenticationDefaults.SessionIdClaimType);
        CopyClaimIfPresent(currentPrincipal, claims, SessionAuthenticationDefaults.UserStateVersionClaimType);

        return new ClaimsPrincipal(new ClaimsIdentity(claims, SessionAuthenticationDefaults.AuthenticationScheme));
    }

    private static void CopyClaimIfPresent(ClaimsPrincipal currentPrincipal, List<Claim> claims, string claimType)
    {
        var value = currentPrincipal.FindFirstValue(claimType);
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(claimType, value));
        }
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

internal sealed class BrowserAuthResult
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }

    public AuthResult ToAuthResult()
    {
        return Succeeded
            ? AuthResult.Success(SuccessMessage)
            : AuthResult.Failure(string.IsNullOrWhiteSpace(ErrorMessage) ? "Authentication request failed." : ErrorMessage);
    }
}


