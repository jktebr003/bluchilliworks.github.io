using HandlebarsDotNet;

using Microsoft.AspNetCore.Components.Authorization;

using Shared.Models;
using Shared.Enums;

using Web.Shared;
using Web.Shared.Helpers;

namespace Web.Features.Authentication;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress);
    Task<UserResponse?> GetCurrentUserAsync();
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
        var users = await _webApiClient.Get<ApiResult<List<UserResponse>>>(
                new WebApiClientInfo<object>
                {
                    Method = $"/users/emailAddress/{username}",
                    Request = string.Empty
                }
            );
        if (users.Success && !users.Value.Any())
            return AuthResult.Failure("Invalid username");

        var user = users.Value.First();
        if (string.IsNullOrEmpty(password) || !string.Equals(password, user.DecryptedPassword, StringComparison.Ordinal))
            return AuthResult.Failure("Invalid password");

        // Notify authentication state change
        await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserAuthenticationAsync(user);

        return AuthResult.Success();
    }

    public async Task LogoutAsync()
    {
        await ((DatabaseAuthenticationStateProvider)_authenticationStateProvider).NotifyUserLogoutAsync();
    }

    public async Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress)
    {
        try
        {
            // Create the user request
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
                return AuthResult.Success();
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
}

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static AuthResult Success() => new AuthResult { Succeeded = true };
    public static AuthResult Failure(string errorMessage) => new AuthResult { Succeeded = false, ErrorMessage = errorMessage };
}

