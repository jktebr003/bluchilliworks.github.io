using System.Globalization;

using MediatR;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Extensions;

namespace MudBlazorWeb.Features.Authentication.Application;

public static class ForgotPasswordCommand
{
    public record Command(string EmailAddress) : IRequest<Result<string>>;

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
                if (string.IsNullOrWhiteSpace(request.EmailAddress))
                {
                    return Failure("Email address is required", "ForgotPassword.Validation");
                }

                var user = await _userStore.GetByEmailAddressAsync(request.EmailAddress, cancellationToken);
                if (user == null)
                {
                    return new Result<string>(string.Empty, true, "ForgotPassword.Success", "If an account with that email exists, you will receive a password reset link.");
                }

                var resetToken = SecurityExtension.CreateRandomVerificationCode(6);
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1).ToString("O", CultureInfo.InvariantCulture);
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = "system";

                await _userStore.UpdateAsync(user, cancellationToken);

                var resetLink = $"{AuthenticationCommandHelpers.GetBaseWebUrl(_configuration)}/authentication/reset-password?email={Uri.EscapeDataString(request.EmailAddress)}&token={Uri.EscapeDataString(resetToken)}";
                var emailBody = AuthenticationCommandHelpers.BuildResetPasswordEmailBody(user.FirstName, resetToken, resetLink);

                await _emailSender.SendAsync(
                    request.EmailAddress,
                    "Password Reset Request",
                    emailBody,
                    cancellationToken);

                return new Result<string>(string.Empty, true, "ForgotPassword.Success", "If an account with that email exists, you will receive a password reset link.");
            }
            catch (Exception ex)
            {
                return Failure($"Failed to send password reset email: {ex.Message}", "Authentication.ForgotPasswordFailed");
            }
        }

        private static Result<string> Failure(string message, string? code = null)
            => new(string.Empty, false, code, message);
    }
}