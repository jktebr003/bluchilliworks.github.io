using Shared.Models;

using Web.Shared;

namespace Web.Services;

public interface IAuthenticationService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<AuthResult> RegisterAsync(string username, string email, string password, string[]? roles = null);
    //Task<ApplicationUser?> GetCurrentUserAsync();
    Task<bool> IsUserInRoleAsync(string role);
    Task<bool> HasClaimAsync(string claimType, string claimValue);
}


public class AuthenticationService : IAuthenticationService
{
    private readonly WebApiClient _webApiClient;

    public AuthenticationService(WebApiClient webApiClient)
    {
        _webApiClient = webApiClient;
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

        return AuthResult.Success();
    }

    public async Task LogoutAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password, string[]? roles = null)
    {
        throw new NotImplementedException();
    }
}

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static AuthResult Success() => new AuthResult { Succeeded = true };
    public static AuthResult Failure(string errorMessage) => new AuthResult { Succeeded = false, ErrorMessage = errorMessage };
}

