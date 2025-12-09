using HandlebarsDotNet;

using Microsoft.AspNetCore.Components.Authorization;

using Shared.Models;
using Shared.Enums;

using Web.Shared;
using Web.Shared.Helpers;
using Blazor.SubtleCrypto;

namespace Web.Features.Authentication;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress);
    Task<AuthResult> SetPasswordAsync(string emailAddress, string verificationToken, string password);
    Task<AuthResult> ResendVerificationAsync(string emailAddress);
    Task<UserResponse?> GetCurrentUserAsync();
    Task UpdateCurrentUserAsync(UserResponse user);
    Task<bool> IsUserInRoleAsync(string role);
    Task<bool> HasClaimAsync(string claimType, string claimValue);
}


public class AuthenticationService : IAuthenticationService
{
    private readonly WebApiClient _webApiClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly LocalStorageHelper _localStorageHelper;   

    public AuthenticationService(WebApiClient webApiClient, AuthenticationStateProvider authenticationStateProvider, LocalStorageHelper localStorageHelper)
    {
        _webApiClient = webApiClient;
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

    public async Task<bool> HasClaimAsync(string claimType, string claimValue)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsUserInRoleAsync(string role)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            // Check API connectivity before making the call
            if (!await _webApiClient.IsApiHealthyAsync())
            {
                return AuthResult.Failure("Unable to connect to the API. Please check your network connection.");
            }

            var verifyLoginRequest = new VerifyLoginRequest
            {
                EmailAddress = username,
                Password = password
            };

            var result = await _webApiClient.Post<VerifyLoginRequest, ApiResult<UserResponse>>(
                new WebApiClientInfo<VerifyLoginRequest>
                {
                    Method = "/users/verify-login",
                    Request = verifyLoginRequest
                }
            );

            if (result.Success && result.Value != null)
            {
                // Notify authentication state change
                await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthenticationAsync(result.Value);
                return AuthResult.Success();
            }
            else
            {
                return AuthResult.Failure(result.Message ?? "Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Login failed: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogoutAsync();
    }

    public async Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress)
    {
        try
        {
            // Check API connectivity before making the call
            if (!await _webApiClient.IsApiHealthyAsync())
            {
                return AuthResult.Failure("Unable to connect to the API. Please check your network connection.");
            }

            // Create the user request - no password needed at registration
            var createUserRequest = new CreateUserRequest
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = emailAddress,
                Username = emailAddress, // Use email as username
                Name = $"{firstName} {lastName}",
                PackageId = "default-package-id", // You may need to adjust this based on your application logic
                UserType = (int)UserType.Customer, // Adjust based on your enum
                Avatar = 17,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            var result = await _webApiClient.Post<CreateUserRequest, ApiResult<string>>(
                new WebApiClientInfo<CreateUserRequest>
                {
                    Method = "/users",
                    Request = createUserRequest
                }
            );

            if (result.Success)
            {
                return AuthResult.Success(result.Message ?? "Registration successful! Please check your email to verify your account.");
            }
            else
            {
                return AuthResult.Failure(result.Message ?? "Registration failed");
            }
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Registration failed: {ex.Message}");
        }
    }

    public async Task<AuthResult> SetPasswordAsync(string emailAddress, string verificationToken, string password)
    {
        try
        {
            // Check API connectivity before making the call
            if (!await _webApiClient.IsApiHealthyAsync())
            {
                return AuthResult.Failure("Unable to connect to the API. Please check your network connection.");
            }

            var setPasswordRequest = new SetPasswordRequest
            {
                EmailAddress = emailAddress,
                VerificationToken = verificationToken,
                Password = password
            };

            var result = await _webApiClient.Post<SetPasswordRequest, ApiResult<string>>(
                new WebApiClientInfo<SetPasswordRequest>
                {
                    Method = "/users/set-password",
                    Request = setPasswordRequest
                }
            );

            if (result.Success)
            {
                return AuthResult.Success(result.Message ?? "Password set successfully! You can now log in.");
            }
            else
            {
                return AuthResult.Failure(result.Message ?? "Failed to set password");
            }
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Failed to set password: {ex.Message}");
        }
    }

    public async Task<AuthResult> ResendVerificationAsync(string emailAddress)
    {
        try
        {
            // Check API connectivity before making the call
            if (!await _webApiClient.IsApiHealthyAsync())
            {
                return AuthResult.Failure("Unable to connect to the API. Please check your network connection.");
            }

            var resendRequest = new ResendVerificationRequest
            {
                EmailAddress = emailAddress
            };

            var result = await _webApiClient.Post<ResendVerificationRequest, ApiResult<string>>(
                new WebApiClientInfo<ResendVerificationRequest>
                {
                    Method = "/users/resend-verification",
                    Request = resendRequest
                }
            );

            if (result.Success)
            {
                return AuthResult.Success(result.Message ?? "Verification email sent!");
            }
            else
            {
                return AuthResult.Failure(result.Message ?? "Failed to send verification email");
            }
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Failed to resend verification: {ex.Message}");
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

