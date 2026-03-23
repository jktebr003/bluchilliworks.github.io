using Fluxor;

using Microsoft.AspNetCore.Components;

using MudBlazorWeb.Features.Authentication.UI;

using Xunit;

namespace MudBlazorWeb.Features.Authentication.Tests;

public class AuthenticationEffectsTests
{
    [Fact]
    public async Task HandleLogin_ShouldDispatchBusyThenSuccess_AndNavigateHome()
    {
        var authService = new FakeAuthenticationService
        {
            LoginHandler = (_, _) => Task.FromResult(AuthResult.Success("ok"))
        };

        var navigationManager = new TestNavigationManager();
        var dispatcher = new FakeDispatcher();
        var effects = new AuthenticationEffects(authService, navigationManager);

        await effects.HandleLogin(new LoginAction("alice@example.com", "Password1!"), dispatcher);

        Assert.Collection(
            dispatcher.DispatchedActions,
            action => Assert.IsType<SetBusyAction>(action),
            action => Assert.IsType<LoginSuccessAction>(action));

        Assert.Equal("/", navigationManager.LastUri);
    }

    [Fact]
    public async Task HandleLogin_ShouldUseFallbackMessage_WhenFailureHasNoErrorMessage()
    {
        var authService = new FakeAuthenticationService
        {
            LoginHandler = (_, _) => Task.FromResult(AuthResult.Failure(string.Empty))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuthenticationEffects(authService, new TestNavigationManager());

        await effects.HandleLogin(new LoginAction("alice@example.com", "Password1!"), dispatcher);

        Assert.Collection(
            dispatcher.DispatchedActions,
            action => Assert.IsType<SetBusyAction>(action),
            action =>
            {
                var failed = Assert.IsType<LoginFailedAction>(action);
                Assert.Equal("Login failed. Please try again.", failed.ErrorMessage);
            });
    }

    [Fact]
    public async Task HandleRegister_ShouldDispatchSuccess_AndNavigateToSetupPassword()
    {
        var authService = new FakeAuthenticationService
        {
            RegisterHandler = (_, _, _) => Task.FromResult(AuthResult.Success("ok"))
        };

        var navigationManager = new TestNavigationManager();
        var dispatcher = new FakeDispatcher();
        var effects = new AuthenticationEffects(authService, navigationManager);

        await effects.HandleRegister(new RegisterAction("Alice", "Admin", "alice+qa@example.com"), dispatcher);

        Assert.Collection(
            dispatcher.DispatchedActions,
            action => Assert.IsType<SetBusyAction>(action),
            action => Assert.IsType<RegisterSuccessAction>(action));

        Assert.Equal("/authentication/setup-password?email=alice%2Bqa%40example.com", navigationManager.LastUri);
    }

    [Fact]
    public async Task HandleResetPassword_ShouldDispatchSuccess_AndNavigateToLogin()
    {
        var authService = new FakeAuthenticationService
        {
            ResetPasswordHandler = (_, _, _) => Task.FromResult(AuthResult.Success("ok"))
        };

        var navigationManager = new TestNavigationManager();
        var dispatcher = new FakeDispatcher();
        var effects = new AuthenticationEffects(authService, navigationManager);

        await effects.HandleResetPassword(new ResetPasswordAction("alice@example.com", "123456", "Password1!"), dispatcher);

        Assert.Collection(
            dispatcher.DispatchedActions,
            action => Assert.IsType<SetBusyAction>(action),
            action => Assert.IsType<ResetPasswordSuccessAction>(action));

        Assert.Equal("/authentication/login", navigationManager.LastUri);
    }

    [Fact]
    public async Task HandleForgotPassword_ShouldDispatchFailure_WhenServiceThrows()
    {
        var authService = new FakeAuthenticationService
        {
            ForgotPasswordHandler = _ => throw new InvalidOperationException("smtp down")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new AuthenticationEffects(authService, new TestNavigationManager());

        await effects.HandleForgotPassword(new ForgotPasswordAction("alice@example.com"), dispatcher);

        Assert.Collection(
            dispatcher.DispatchedActions,
            action => Assert.IsType<SetBusyAction>(action),
            action =>
            {
                var failed = Assert.IsType<ForgotPasswordFailedAction>(action);
                Assert.Contains("Failed to process forgot password request. Please try again.", failed.ErrorMessage);
                Assert.Contains("smtp down", failed.ErrorMessage);
            });
    }

    private sealed class FakeDispatcher : IDispatcher
    {
        public List<object> DispatchedActions { get; } = new();
        public event EventHandler<ActionDispatchedEventArgs>? ActionDispatched;

        public void Dispatch(object action)
        {
            DispatchedActions.Add(action);
            ActionDispatched?.Invoke(this, new ActionDispatchedEventArgs(action));
        }
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public string? LastUri { get; private set; }

        public TestNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            LastUri = ToBaseRelativePath(ToAbsoluteUri(uri).AbsoluteUri);
            LastUri = string.IsNullOrWhiteSpace(LastUri) ? "/" : $"/{LastUri}";
        }
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public Func<string, string, Task<AuthResult>>? LoginHandler { get; init; }
        public Func<string, string, string, Task<AuthResult>>? RegisterHandler { get; init; }
        public Func<string, string, string, Task<AuthResult>>? SetPasswordHandler { get; init; }
        public Func<string, Task<AuthResult>>? ResendVerificationHandler { get; init; }
        public Func<string, Task<AuthResult>>? ForgotPasswordHandler { get; init; }
        public Func<string, string, string, Task<AuthResult>>? ResetPasswordHandler { get; init; }

        public Task<AuthResult> LoginAsync(string username, string password)
            => LoginHandler?.Invoke(username, password) ?? Task.FromResult(AuthResult.Failure("No login handler configured"));

        public Task LogoutAsync() => Task.CompletedTask;

        public Task<AuthResult> RegisterAsync(string firstName, string lastName, string emailAddress)
            => RegisterHandler?.Invoke(firstName, lastName, emailAddress) ?? Task.FromResult(AuthResult.Failure("No register handler configured"));

        public Task<AuthResult> SetPasswordAsync(string emailAddress, string verificationToken, string password)
            => SetPasswordHandler?.Invoke(emailAddress, verificationToken, password) ?? Task.FromResult(AuthResult.Failure("No set password handler configured"));

        public Task<AuthResult> ResendVerificationAsync(string emailAddress)
            => ResendVerificationHandler?.Invoke(emailAddress) ?? Task.FromResult(AuthResult.Failure("No resend verification handler configured"));

        public Task<AuthResult> ForgotPasswordAsync(string emailAddress)
            => ForgotPasswordHandler?.Invoke(emailAddress) ?? Task.FromResult(AuthResult.Failure("No forgot password handler configured"));

        public Task<AuthResult> ResetPasswordAsync(string emailAddress, string resetToken, string newPassword)
            => ResetPasswordHandler?.Invoke(emailAddress, resetToken, newPassword) ?? Task.FromResult(AuthResult.Failure("No reset password handler configured"));

        public Task<MudBlazorWeb.Shared.Models.UserResponse?> GetCurrentUserAsync()
            => Task.FromResult<MudBlazorWeb.Shared.Models.UserResponse?>(null);

        public Task UpdateCurrentUserAsync(MudBlazorWeb.Shared.Models.UserResponse user)
            => Task.CompletedTask;

        public Task<bool> IsUserInRoleAsync(string role)
            => Task.FromResult(false);

        public Task<bool> HasClaimAsync(string claimType, string claimValue)
            => Task.FromResult(false);
    }
}
