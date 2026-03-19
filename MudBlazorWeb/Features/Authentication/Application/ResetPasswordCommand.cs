using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Authentication.Application;

public static class ResetPasswordCommand
{
    public record Command(string EmailAddress, string ResetToken, string NewPassword) : IRequest<Result<string>>;

    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.ResetToken))
                {
                    return Failure("Email address and reset token are required.", "ResetPassword.Validation");
                }

                var validationMessage = AuthenticationCommandHelpers.ValidatePassword(request.NewPassword);
                if (validationMessage != null)
                {
                    return Failure(validationMessage, "ResetPassword.Validation");
                }

                var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress, cancellationToken);
                if (user == null)
                {
                    return Failure("Invalid or expired reset token. Please request a new password reset.", "ResetPassword.InvalidToken");
                }

                if (string.IsNullOrWhiteSpace(user.PasswordResetToken) ||
                    !string.Equals(user.PasswordResetToken, request.ResetToken, StringComparison.Ordinal))
                {
                    return Failure("Invalid or expired reset token. Please request a new password reset.", "ResetPassword.InvalidToken");
                }

                if (AuthenticationCommandHelpers.IsExpired(user.PasswordResetTokenExpiry))
                {
                    return Failure("Reset token has expired. Please request a new password reset.", "ResetPassword.TokenExpired");
                }

                user.HashedPassword = AuthenticationCommandHelpers.HashPassword(request.NewPassword);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = user.EmailAddress;

                await _userRepository.UpdateUserAsync(user, cancellationToken);

                return new Result<string>(user.Id.ToString(), true, "ResetPassword.Success", "Your password has been reset successfully. You can now log in with your new password.");
            }
            catch (Exception ex)
            {
                return Failure($"Failed to reset password: {ex.Message}", "Authentication.ResetPasswordFailed");
            }
        }

        private static Result<string> Failure(string message, string? code = null)
            => new(string.Empty, false, code, message);
    }
}