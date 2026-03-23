using Microsoft.Extensions.Configuration;

using MudBlazorWeb.Features.Authentication.Application;
using MudBlazorWeb.Features.Authentication.Domain;

using Xunit;

namespace MudBlazorWeb.Features.Authentication.Tests;

public class AuthenticationHandlerTests
{
    [Fact]
    public async Task LoginHandler_ShouldReturnValidationFailure_WhenCredentialsMissing()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new LoginCommand.Handler(store);

        var result = await handler.Handle(new LoginCommand.Command("", ""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("VerifyLogin.Validation", result.Code);
        Assert.Equal(string.Empty, result.Value.ID);
    }

    [Fact]
    public async Task LoginHandler_ShouldReturnInvalidCredentials_WhenPasswordDoesNotMatch()
    {
        var user = CreateUser();
        user.EmailVerified = true;
        user.HashedPassword = AuthenticationCommandHelpers.HashPassword("CorrectPassword1!");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new LoginCommand.Handler(store);

        var result = await handler.Handle(new LoginCommand.Command(user.EmailAddress, "WrongPassword1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("VerifyLogin.InvalidCredentials", result.Code);
    }

    [Fact]
    public async Task LoginHandler_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var user = CreateUser();
        user.EmailVerified = true;
        user.HashedPassword = AuthenticationCommandHelpers.HashPassword("CorrectPassword1!");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new LoginCommand.Handler(store);

        var result = await handler.Handle(new LoginCommand.Command(user.EmailAddress, "CorrectPassword1!"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("VerifyLogin.Success", result.Code);
        Assert.Equal(user.EmailAddress, result.Value.EmailAddress);
        Assert.Equal(user.Id.ToString(), result.Value.ID);
    }

    [Fact]
    public async Task RegisterHandler_ShouldReturnValidationFailure_WhenRequiredDataMissing()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new RegisterCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new RegisterCommand.Command("", "Last", ""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CreateUser.Validation", result.Code);
        Assert.Null(store.LastSavedUser);
    }

    [Fact]
    public async Task RegisterHandler_ShouldCreateUser_WhenEmailIsAvailable()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new RegisterCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new RegisterCommand.Command("Alice", "Admin", "alice@example.com"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("CreateUser.Success", result.Code);
        Assert.NotNull(store.LastSavedUser);
        Assert.Equal("alice@example.com", store.LastSavedUser!.EmailAddress);
        Assert.False(store.LastSavedUser.EmailVerified);
        Assert.False(string.IsNullOrWhiteSpace(store.LastSavedUser.EmailVerificationToken));
        Assert.True(Guid.TryParse(result.Value, out _));
    }

    [Fact]
    public async Task SetPasswordHandler_ShouldReturnInvalidToken_WhenTokenDoesNotMatch()
    {
        var user = CreateUser();
        user.EmailVerificationToken = "ABC123";
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(1).ToString("O");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new SetPasswordCommand.Handler(store);

        var result = await handler.Handle(new SetPasswordCommand.Command(user.EmailAddress, "ZZZ999", "StrongPass1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("SetPassword.InvalidToken", result.Code);
        Assert.Null(store.LastUpdatedUser);
    }

    [Fact]
    public async Task SetPasswordHandler_ShouldHashPassword_AndVerifyEmail_WhenTokenValid()
    {
        var user = CreateUser();
        user.EmailVerificationToken = "ABC123";
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(1).ToString("O");
        user.EmailVerified = false;

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new SetPasswordCommand.Handler(store);

        var result = await handler.Handle(new SetPasswordCommand.Command(user.EmailAddress, "ABC123", "StrongPass1!"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("SetPassword.Success", result.Code);
        Assert.NotNull(store.LastUpdatedUser);
        Assert.True(store.LastUpdatedUser!.EmailVerified);
        Assert.Null(store.LastUpdatedUser.EmailVerificationToken);
        Assert.True(AuthenticationCommandHelpers.VerifyPassword(store.LastUpdatedUser.HashedPassword, "StrongPass1!"));
    }

    [Fact]
    public async Task ResetPasswordHandler_ShouldReturnTokenExpired_WhenExpiryPassed()
    {
        var user = CreateUser();
        user.PasswordResetToken = "RST123";
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(-5).ToString("O");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new ResetPasswordCommand.Handler(store);

        var result = await handler.Handle(new ResetPasswordCommand.Command(user.EmailAddress, "RST123", "NewStrongPass1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ResetPassword.TokenExpired", result.Code);
        Assert.Null(store.LastUpdatedUser);
    }

    [Fact]
    public async Task ForgotPasswordHandler_ShouldReturnSuccessWithoutUpdate_WhenUserDoesNotExist()
    {
        var store = new StubAuthenticationUserStore { UserByEmail = null };
        var handler = new ForgotPasswordCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new ForgotPasswordCommand.Command("nobody@example.com"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ForgotPassword.Success", result.Code);
        Assert.Null(store.LastUpdatedUser);
    }

    [Fact]
    public async Task ResendVerificationHandler_ShouldReturnAlreadyVerified_WhenUserIsVerified()
    {
        var user = CreateUser();
        user.EmailVerified = true;

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new ResendVerificationCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new ResendVerificationCommand.Command(user.EmailAddress), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ResendVerification.AlreadyVerified", result.Code);
    }

    // =========================================================================
    // Extended LoginCommand tests
    // =========================================================================

    [Fact]
    public async Task LoginHandler_ShouldReturnInvalidCredentials_WhenUserDoesNotExist()
    {
        var store = new StubAuthenticationUserStore { UserByEmail = null };
        var handler = new LoginCommand.Handler(store);

        var result = await handler.Handle(new LoginCommand.Command("nobody@example.com", "Pass1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("VerifyLogin.InvalidCredentials", result.Code);
    }

    [Fact]
    public async Task LoginHandler_ShouldReturnEmailNotVerified_WhenUserHasNotVerifiedEmail()
    {
        var user = CreateUser();
        user.EmailVerified = false;
        user.HashedPassword = AuthenticationCommandHelpers.HashPassword("Password1!");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new LoginCommand.Handler(store);

        var result = await handler.Handle(new LoginCommand.Command(user.EmailAddress, "Password1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("VerifyLogin.EmailNotVerified", result.Code);
    }

    [Fact]
    public async Task LoginHandler_ShouldReturnPasswordNotSet_WhenHashedPasswordIsEmpty()
    {
        var user = CreateUser();
        user.EmailVerified = true;
        user.HashedPassword = string.Empty;

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new LoginCommand.Handler(store);

        var result = await handler.Handle(new LoginCommand.Command(user.EmailAddress, "Password1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("VerifyLogin.PasswordNotSet", result.Code);
    }

    // =========================================================================
    // Extended RegisterCommand tests
    // =========================================================================

    [Fact]
    public async Task RegisterHandler_ShouldReturnUserExists_WhenEmailAlreadyRegistered()
    {
        var existingUser = CreateUser();
        var store = new StubAuthenticationUserStore { UserByEmail = existingUser };
        var handler = new RegisterCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new RegisterCommand.Command("Alice", "Admin", existingUser.EmailAddress), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CreateUser.UserExists", result.Code);
        Assert.Null(store.LastSavedUser);
    }

    [Fact]
    public async Task RegisterHandler_ShouldSendVerificationEmail_WhenUserIsCreated()
    {
        var store = new StubAuthenticationUserStore();
        var emailSender = new FakeEmailSender();
        var handler = new RegisterCommand.Handler(store, BuildConfiguration(), emailSender);

        await handler.Handle(new RegisterCommand.Command("Alice", "Admin", "alice@example.com"), CancellationToken.None);

        Assert.Single(emailSender.SentEmails);
        Assert.Equal("alice@example.com", emailSender.SentEmails[0].To);
        Assert.Contains("Verify", emailSender.SentEmails[0].Subject);
    }

    // =========================================================================
    // Extended SetPasswordCommand tests
    // =========================================================================

    [Fact]
    public async Task SetPasswordHandler_ShouldReturnValidationFailure_WhenEmailOrTokenMissing()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new SetPasswordCommand.Handler(store);

        var result = await handler.Handle(new SetPasswordCommand.Command("", "", "Password1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("SetPassword.Validation", result.Code);
    }

    [Fact]
    public async Task SetPasswordHandler_ShouldReturnValidationFailure_WhenPasswordTooWeak()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new SetPasswordCommand.Handler(store);

        var result = await handler.Handle(new SetPasswordCommand.Command("alice@example.com", "ABC123", "weak"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("SetPassword.Validation", result.Code);
    }

    [Fact]
    public async Task SetPasswordHandler_ShouldReturnTokenExpired_WhenVerificationTokenIsExpired()
    {
        var user = CreateUser();
        user.EmailVerificationToken = "ABC123";
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(-10).ToString("O");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new SetPasswordCommand.Handler(store);

        var result = await handler.Handle(new SetPasswordCommand.Command(user.EmailAddress, "ABC123", "StrongPass1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("SetPassword.TokenExpired", result.Code);
        Assert.Null(store.LastUpdatedUser);
    }

    // =========================================================================
    // Extended ResetPasswordCommand tests
    // =========================================================================

    [Fact]
    public async Task ResetPasswordHandler_ShouldReturnValidationFailure_WhenEmailOrTokenMissing()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new ResetPasswordCommand.Handler(store);

        var result = await handler.Handle(new ResetPasswordCommand.Command("", "", "Password1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ResetPassword.Validation", result.Code);
    }

    [Fact]
    public async Task ResetPasswordHandler_ShouldReturnInvalidToken_WhenTokenDoesNotMatch()
    {
        var user = CreateUser();
        user.PasswordResetToken = "CORRECT1";
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1).ToString("O");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new ResetPasswordCommand.Handler(store);

        var result = await handler.Handle(new ResetPasswordCommand.Command(user.EmailAddress, "WRONGTOK", "NewStrongPass1!"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ResetPassword.InvalidToken", result.Code);
    }

    [Fact]
    public async Task ResetPasswordHandler_ShouldHashPassword_AndClearToken_WhenTokenValid()
    {
        var user = CreateUser();
        user.PasswordResetToken = "RST123";
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1).ToString("O");

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var handler = new ResetPasswordCommand.Handler(store);

        var result = await handler.Handle(new ResetPasswordCommand.Command(user.EmailAddress, "RST123", "NewStrongPass1!"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ResetPassword.Success", result.Code);
        Assert.NotNull(store.LastUpdatedUser);
        Assert.Null(store.LastUpdatedUser!.PasswordResetToken);
        Assert.True(AuthenticationCommandHelpers.VerifyPassword(store.LastUpdatedUser.HashedPassword, "NewStrongPass1!"));
    }

    // =========================================================================
    // Extended ForgotPasswordCommand tests
    // =========================================================================

    [Fact]
    public async Task ForgotPasswordHandler_ShouldReturnValidationFailure_WhenEmailAddressMissing()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new ForgotPasswordCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new ForgotPasswordCommand.Command(""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ForgotPassword.Validation", result.Code);
    }

    [Fact]
    public async Task ForgotPasswordHandler_ShouldUpdateToken_AndSendEmail_WhenUserExists()
    {
        var user = CreateUser();
        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var emailSender = new FakeEmailSender();
        var handler = new ForgotPasswordCommand.Handler(store, BuildConfiguration(), emailSender);

        var result = await handler.Handle(new ForgotPasswordCommand.Command(user.EmailAddress), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ForgotPassword.Success", result.Code);
        Assert.NotNull(store.LastUpdatedUser);
        Assert.False(string.IsNullOrWhiteSpace(store.LastUpdatedUser!.PasswordResetToken));
        Assert.Single(emailSender.SentEmails);
        Assert.Equal(user.EmailAddress, emailSender.SentEmails[0].To);
    }

    // =========================================================================
    // Extended ResendVerificationCommand tests
    // =========================================================================

    [Fact]
    public async Task ResendVerificationHandler_ShouldReturnValidationFailure_WhenEmailMissing()
    {
        var store = new StubAuthenticationUserStore();
        var handler = new ResendVerificationCommand.Handler(store, BuildConfiguration(), new FakeEmailSender());

        var result = await handler.Handle(new ResendVerificationCommand.Command(""), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ResendVerification.Validation", result.Code);
    }

    [Fact]
    public async Task ResendVerificationHandler_ShouldReturnSuccess_WhenUserDoesNotExist()
    {
        var store = new StubAuthenticationUserStore { UserByEmail = null };
        var emailSender = new FakeEmailSender();
        var handler = new ResendVerificationCommand.Handler(store, BuildConfiguration(), emailSender);

        var result = await handler.Handle(new ResendVerificationCommand.Command("nobody@example.com"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ResendVerification.Success", result.Code);
        Assert.Empty(emailSender.SentEmails);
    }

    [Fact]
    public async Task ResendVerificationHandler_ShouldReturnSuccess_WhenEmailSentSuccessfully()
    {
        var user = CreateUser();
        user.EmailVerified = false;

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var emailSender = new FakeEmailSender { ShouldSucceed = true };
        var handler = new ResendVerificationCommand.Handler(store, BuildConfiguration(), emailSender);

        var result = await handler.Handle(new ResendVerificationCommand.Command(user.EmailAddress), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("ResendVerification.Success", result.Code);
        Assert.Single(emailSender.SentEmails);
        Assert.Equal(user.EmailAddress, emailSender.SentEmails[0].To);
    }

    [Fact]
    public async Task ResendVerificationHandler_ShouldReturnEmailFailed_WhenEmailSenderFails()
    {
        var user = CreateUser();
        user.EmailVerified = false;

        var store = new StubAuthenticationUserStore { UserByEmail = user };
        var emailSender = new FakeEmailSender { ShouldSucceed = false };
        var handler = new ResendVerificationCommand.Handler(store, BuildConfiguration(), emailSender);

        var result = await handler.Handle(new ResendVerificationCommand.Command(user.EmailAddress), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ResendVerification.EmailFailed", result.Code);
        Assert.Empty(emailSender.SentEmails);
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private static AuthenticationUser CreateUser()
    {
        return new AuthenticationUser
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Admin",
            Name = "Alice Admin",
            Username = "alice@example.com",
            EmailAddress = "alice@example.com",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "tests"
        };
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BaseWebUrl"] = "http://localhost:5001"
            })
            .Build();
    }

    private sealed class StubAuthenticationUserStore : IAuthenticationUserStore
    {
        public AuthenticationUser? UserByEmail { get; init; }
        public AuthenticationUser? LastSavedUser { get; private set; }
        public AuthenticationUser? LastUpdatedUser { get; private set; }

        public Task<AuthenticationUser?> GetByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default)
            => Task.FromResult(UserByEmail);

        public Task SaveAsync(AuthenticationUser user, CancellationToken cancellationToken = default)
        {
            LastSavedUser = user;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AuthenticationUser user, CancellationToken cancellationToken = default)
        {
            LastUpdatedUser = user;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public bool ShouldSucceed { get; init; } = true;
        public List<(string To, string Subject)> SentEmails { get; } = new();

        public Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (ShouldSucceed) SentEmails.Add((to, subject));
            return Task.FromResult(ShouldSucceed);
        }
    }
}
