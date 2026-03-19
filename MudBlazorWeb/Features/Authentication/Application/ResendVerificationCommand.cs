using System.Globalization;

using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Extensions;

namespace MudBlazorWeb.Features.Authentication.Application;

public static class ResendVerificationCommand
{
    public record Command(string EmailAddress) : IRequest<Result<string>>;

    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public Handler(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmailAddress))
                {
                    return Failure("Email address is required", "ResendVerification.Validation");
                }

                var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress, cancellationToken);
                if (user == null)
                {
                    return new Result<string>(string.Empty, true, "ResendVerification.Success", "If an account exists with this email, a verification email has been sent.");
                }

                if (user.EmailVerified)
                {
                    return Failure("This email address has already been verified.", "ResendVerification.AlreadyVerified");
                }

                var verificationToken = SecurityExtension.CreateRandomVerificationCode(6);
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24).ToString("O", CultureInfo.InvariantCulture);
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = "system";

                await _userRepository.UpdateUserAsync(user, cancellationToken);

                var verificationLink = $"{AuthenticationCommandHelpers.GetBaseWebUrl(_configuration)}/authentication/setup-password?token={Uri.EscapeDataString(verificationToken)}&email={Uri.EscapeDataString(request.EmailAddress)}";
                var emailBody = AuthenticationCommandHelpers.BuildVerificationEmailBody(user.FirstName, verificationToken, verificationLink);
                var sent = await AuthenticationCommandHelpers.TrySendEmailAsync(
                    _configuration,
                    request.EmailAddress,
                    "BluChilliWorks - Email Verification",
                    emailBody,
                    cancellationToken);

                if (!sent)
                {
                    return Failure("Failed to send verification email. Please try again later.", "ResendVerification.EmailFailed");
                }

                return new Result<string>(user.Id.ToString(), true, "ResendVerification.Success", "Verification email has been sent. Please check your inbox.");
            }
            catch (Exception ex)
            {
                return Failure($"Failed to resend verification: {ex.Message}", "Authentication.ResendVerificationFailed");
            }
        }

        private static Result<string> Failure(string message, string? code = null)
            => new(string.Empty, false, code, message);
    }
}