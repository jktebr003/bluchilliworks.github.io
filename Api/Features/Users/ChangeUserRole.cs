using Api.Filters;
using Carter;
using FluentValidation;
using MediatR;
using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class ChangeUserRole
{
    public class Command : IRequest<ApiResult<string>>
    {
        public string UserId { get; set; } = string.Empty;
        public UserType NewRole { get; set; }
        public string? RequestingUserId { get; set; } // Staff member making the change
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("User ID is required");
            
            RuleFor(c => c.NewRole)
                .IsInEnum().WithMessage("Invalid user role");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IUserRepository userRepository, IValidator<Command> validator)
        {
            _userRepository = userRepository;
            _validator = validator;
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
                        "ChangeUserRole.Unauthorized", 
                        "Only staff members can change user roles"
                    );
                }
            }

            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<string>(string.Empty, false, "ChangeUserRole.Validation", errors);
            }

            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return new ApiResult<string>(
                    string.Empty, 
                    false, 
                    "ChangeUserRole.NotFound", 
                    "The user with the specified ID was not found"
                );
            }

            user.UserType = (int)request.NewRole;
            
            await _userRepository.SaveUserAsync(user);

            return new ApiResult<string>($"User role updated to {request.NewRole} successfully", true);
        }
    }
}

public class ChangeUserRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{userId}/role", async (string userId, ChangeUserRoleRequest request, ISender sender) =>
        {
            var command = new ChangeUserRole.Command
            {
                UserId = userId,
                NewRole = request.NewRole,
                RequestingUserId = request.RequestingUserId
            };

            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
