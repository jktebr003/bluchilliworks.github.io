using Api.Filters;
using Api.Services;
using Carter;
using FluentValidation;
using MediatR;
using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class ChangeUserPassword
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string UserId { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? RequestingUserId { get; set; } // Staff member making the change
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("User ID is required");
            
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
            // Verify requesting user is staff
            if (!string.IsNullOrEmpty(request.RequestingUserId))
            {
                var requestingUser = await _userRepository.GetUserByIdAsync(request.RequestingUserId);
                if (requestingUser == null || requestingUser.UserType != (int)UserType.Staff)
                {
                    return new ApiResult<string>(
                        string.Empty, 
                        false, 
                        "ChangeUserPassword.Unauthorized", 
                        "Only staff members can change user passwords"
                    );
                }
            }

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "ChangeUserPassword.Validation", errors);
            }

            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return new ApiResult<string>(
                    string.Empty, 
                    false, 
                    "ChangeUserPassword.NotFound", 
                    "The user with the specified ID was not found"
                );
            }

            // Hash the new password
            user.HashedPassword = _passwordHashingService.HashPassword(request.NewPassword);
            
            await _userRepository.SaveUserAsync(user);

            return new ApiResult<string>("Password updated successfully", true);
        }
    }
}

public class ChangeUserPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{userId}/password", async (string userId, ChangeUserPasswordRequest request, ISender sender) =>
        {
            var command = new ChangeUserPassword.Command
            {
                UserId = userId,
                NewPassword = request.NewPassword,
                RequestingUserId = request.RequestingUserId
            };

            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
