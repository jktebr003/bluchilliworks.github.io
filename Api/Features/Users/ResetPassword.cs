using Api.Infrastructure.Database.MongoDb.Repositories;
using Api.Services;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Models;

namespace Api.Features.Users;

/// <summary>
/// Handles password reset with token verification
/// </summary>
public static class ResetPassword
{
    /// <summary>
    /// Command to reset password
    /// </summary>
    public class Command : IRequest<ApiResult<string>>
    {
        /// <summary>
        /// Email address of the user
        /// </summary>
        public string? EmailAddress { get; set; }
        /// <summary>
        /// Password reset token
        /// </summary>
        public string? ResetToken { get; set; }
        /// <summary>
        /// New password
        /// </summary>
        public string? NewPassword { get; set; }
    }

    /// <summary>
    /// Validator for reset password command
    /// </summary>
    public class Validator : AbstractValidator<Command>
    {
        /// <summary>
        /// Initializes validator rules
        /// </summary>
        public Validator()
        {
            RuleFor(c => c.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format");
            
            RuleFor(c => c.ResetToken)
                .NotEmpty().WithMessage("Reset token is required");
            
            RuleFor(c => c.NewPassword)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<Command> _validator;
        private readonly IPasswordHashingService _passwordHashingService;

        public Handler(
            IUserRepository userRepository, 
            IValidator<Command> validator,
            IPasswordHashingService passwordHashingService)
        {
            _userRepository = userRepository;
            _validator = validator;
            _passwordHashingService = passwordHashingService;
        }

        public async Task<ApiResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "ResetPassword.Validation", errors);
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress);
            if (user == null)
            {
                return new ApiResult<string>(string.Empty, false, "ResetPassword.InvalidToken", 
                    "Invalid or expired reset token. Please request a new password reset.");
            }

            // Verify reset token
            if (string.IsNullOrEmpty(user.PasswordResetToken) || 
                user.PasswordResetToken != request.ResetToken)
            {
                return new ApiResult<string>(string.Empty, false, "ResetPassword.InvalidToken", 
                    "Invalid or expired reset token. Please request a new password reset.");
            }

            // Check token expiry
            if (!string.IsNullOrEmpty(user.PasswordResetTokenExpiry))
            {
                if (DateTime.TryParse(user.PasswordResetTokenExpiry, out var expiry))
                {
                    if (expiry < DateTime.UtcNow)
                    {
                        return new ApiResult<string>(string.Empty, false, "ResetPassword.TokenExpired", 
                            "Reset token has expired. Please request a new password reset.");
                    }
                }
            }

            // Hash the new password
            var hashedPassword = _passwordHashingService.HashPassword(request.NewPassword!);

            // Update user password and clear reset token
            user.HashedPassword = hashedPassword;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.ModifiedOn = DateTime.UtcNow.ToString("o");
            user.ModifiedBy = user.EmailAddress;

            await _userRepository.SaveUserAsync(user);

            return new ApiResult<string>(user.ID, true, "ResetPassword.Success", 
                "Your password has been reset successfully. You can now log in with your new password.");
        }
    }
}

/// <summary>
/// Endpoint for password reset functionality
/// </summary>
public class ResetPasswordEndpoint : ICarterModule
{
    /// <summary>
    /// Configures the reset password route
    /// </summary>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/reset-password", async (ResetPasswordRequest request, ISender sender) =>
        {
            var result = await sender.Send(request.Adapt<ResetPassword.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AllowAnonymous(); // Allow anonymous access for password reset
    }
}
