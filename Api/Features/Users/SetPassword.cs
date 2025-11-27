using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Repositories;
using Api.Services;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Models;

namespace Api.Features.Users;

public static class SetPassword
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string? EmailAddress { get; set; }
        public string? VerificationToken { get; set; }
        public string? Password { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format");
            
            RuleFor(c => c.VerificationToken)
                .NotEmpty().WithMessage("Verification token is required");
            
            RuleFor(c => c.Password)
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
                return new ApiResult<string>(string.Empty, false, "SetPassword.Validation", errors);
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress);
            if (user == null)
            {
                return new ApiResult<string>(string.Empty, false, "SetPassword.UserNotFound", "User not found");
            }

            // Verify token
            if (string.IsNullOrEmpty(user.EmailVerificationToken) || 
                user.EmailVerificationToken != request.VerificationToken)
            {
                return new ApiResult<string>(string.Empty, false, "SetPassword.InvalidToken", "Invalid verification token");
            }

            // Check token expiry
            if (!string.IsNullOrEmpty(user.EmailVerificationTokenExpiry))
            {
                if (DateTime.TryParse(user.EmailVerificationTokenExpiry, out var expiry))
                {
                    if (expiry < DateTime.UtcNow)
                    {
                        return new ApiResult<string>(string.Empty, false, "SetPassword.TokenExpired", "Verification token has expired. Please request a new one.");
                    }
                }
            }

            // Hash the password using Argon2
            var hashedPassword = _passwordHashingService.HashPassword(request.Password);

            // Update user
            user.HashedPassword = hashedPassword;
            user.EmailVerified = true;
            user.EmailVerificationToken = null; // Clear token after use
            user.EmailVerificationTokenExpiry = null;
            user.ModifiedOn = DateTime.UtcNow.ToString("o");
            user.ModifiedBy = user.EmailAddress;

            await _userRepository.SaveUserAsync(user);

            return new ApiResult<string>(user.ID, true, "SetPassword.Success", "Password set successfully. You can now log in.");
        }
    }
}

public class SetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/set-password", async (SetPasswordRequest request, ISender sender) =>
        {
            var result = await sender.Send(request.Adapt<SetPassword.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AllowAnonymous(); // Allow anonymous access for password setup
    }
}
