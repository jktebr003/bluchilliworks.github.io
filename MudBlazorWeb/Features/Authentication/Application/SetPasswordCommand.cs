using MediatR;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Authentication.Application;

public static class SetPasswordCommand
{
    public record Command(string EmailAddress, string VerificationToken, string Password) : IRequest<Result<string>>;

    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IAuthenticationUserStore _userStore;

        public Handler(IAuthenticationUserStore userStore)
        {
            _userStore = userStore;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var validationMessage = AuthenticationCommandHelpers.ValidatePassword(request.Password);
                if (string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.VerificationToken))
                {
                    return Failure("Email address and verification token are required.", "SetPassword.Validation");
                }

                if (validationMessage != null)
                {
                    return Failure(validationMessage, "SetPassword.Validation");
                }

                var user = await _userStore.GetByEmailAddressAsync(request.EmailAddress, cancellationToken);
                if (user == null)
                {
                    return Failure("User not found", "SetPassword.UserNotFound");
                }

                if (string.IsNullOrWhiteSpace(user.EmailVerificationToken) ||
                    !string.Equals(user.EmailVerificationToken, request.VerificationToken, StringComparison.Ordinal))
                {
                    return Failure("Invalid verification token", "SetPassword.InvalidToken");
                }

                if (AuthenticationCommandHelpers.IsExpired(user.EmailVerificationTokenExpiry))
                {
                    return Failure("Verification token has expired. Please request a new one.", "SetPassword.TokenExpired");
                }

                user.HashedPassword = AuthenticationCommandHelpers.HashPassword(request.Password);
                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationTokenExpiry = null;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = user.EmailAddress;

                await _userStore.UpdateAsync(user, cancellationToken);

                return new Result<string>(user.Id.ToString(), true, "SetPassword.Success", "Password set successfully. You can now log in.");
            }
            catch (Exception ex)
            {
                return Failure($"Failed to set password: {ex.Message}", "Authentication.SetPasswordFailed");
            }
        }

        private static Result<string> Failure(string message, string? code = null)
            => new(string.Empty, false, code, message);
    }
}