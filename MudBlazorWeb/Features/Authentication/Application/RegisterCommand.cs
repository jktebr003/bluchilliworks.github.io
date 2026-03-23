using System.Globalization;

using MediatR;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Extensions;

namespace MudBlazorWeb.Features.Authentication.Application;

public static class RegisterCommand
{
    public record Command(string FirstName, string LastName, string EmailAddress) : IRequest<Result<string>>;

    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IAuthenticationUserStore _userStore;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public Handler(IAuthenticationUserStore userStore, IConfiguration configuration, IEmailSender emailSender)
        {
            _userStore = userStore;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmailAddress) ||
                    string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName))
                {
                    return Failure("Please provide email address, first name, and last name.", "CreateUser.Validation");
                }

                var existingUser = await _userStore.GetByEmailAddressAsync(request.EmailAddress, cancellationToken);
                if (existingUser != null)
                {
                    return Failure("A user with this email address already exists.", "CreateUser.UserExists");
                }

                var now = DateTime.UtcNow;
                var verificationToken = SecurityExtension.CreateRandomVerificationCode(6);
                var tokenExpiry = now.AddHours(24).ToString("O", CultureInfo.InvariantCulture);

                var user = new AuthenticationUser
                {
                    Id = Guid.NewGuid(),
                    Name = $"{request.FirstName} {request.LastName}".Trim(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Username = request.EmailAddress,
                    EmailAddress = request.EmailAddress,
                    HashedPassword = string.Empty,
                    EmailVerified = false,
                    EmailVerificationToken = verificationToken,
                    EmailVerificationTokenExpiry = tokenExpiry,
                    PasswordResetToken = null,
                    PasswordResetTokenExpiry = null,
                    PackageId = Guid.Empty,
                    Avatar = 17,
                    UserType = (int)UserType.Customer,
                    CreatedOn = now,
                    CreatedBy = "system"
                };

                await _userStore.SaveAsync(user, cancellationToken);

                var verificationLink = $"{AuthenticationCommandHelpers.GetBaseWebUrl(_configuration)}/authentication/setup-password?token={Uri.EscapeDataString(verificationToken)}&email={Uri.EscapeDataString(request.EmailAddress)}";
                var emailBody = AuthenticationCommandHelpers.BuildVerificationEmailBody(request.FirstName, verificationToken, verificationLink);

                await _emailSender.SendAsync(
                    request.EmailAddress,
                    "Welcome to BluChilliWorks - Verify Your Email",
                    emailBody,
                    cancellationToken);

                return new Result<string>(user.Id.ToString(), true, "CreateUser.Success", "Registration successful. Please check your email to verify your account and set up your password.");
            }
            catch (Exception ex)
            {
                return Failure($"Registration failed: {ex.Message}", "Authentication.RegisterFailed");
            }
        }

        private static Result<string> Failure(string message, string? code = null)
            => new(string.Empty, false, code, message);
    }
}