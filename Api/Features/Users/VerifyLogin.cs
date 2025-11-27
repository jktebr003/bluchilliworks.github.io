using Api.Infrastructure.Database.MongoDb.Repositories;
using Api.Services;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Shared.Models;

namespace Api.Features.Users;

public static class VerifyLogin
{
    public class Command : IRequest<ApiResult<UserResponse>>
    {
        public string? EmailAddress { get; set; }
        public string? Password { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format");
            
            RuleFor(c => c.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, ApiResult<UserResponse>>
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

        public async Task<ApiResult<UserResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return new ApiResult<UserResponse>(new UserResponse(), false, "VerifyLogin.Validation", errors);
            }

            // Find user by email
            var user = await _userRepository.GetUserByEmailAddressAsync(request.EmailAddress);
            if (user == null)
            {
                return new ApiResult<UserResponse>(new UserResponse(), false, "VerifyLogin.InvalidCredentials", "Invalid email or password");
            }

            // Check if email is verified
            if (!user.EmailVerified)
            {
                return new ApiResult<UserResponse>(new UserResponse(), false, "VerifyLogin.EmailNotVerified", "Please verify your email address before logging in. Check your inbox for the verification email.");
            }

            // Verify password
            bool passwordValid = false;

            // Check new hashed password
            if (!string.IsNullOrEmpty(user.HashedPassword) && !string.IsNullOrEmpty(request.Password))
            {
                passwordValid = _passwordHashingService.VerifyPassword(user.HashedPassword, request.Password);
            }
            else
            {
                return new ApiResult<UserResponse>(new UserResponse(), false, "VerifyLogin.PasswordNotSet", "Please complete your password setup. Check your email for instructions.");
            }

            if (!passwordValid)
            {
                return new ApiResult<UserResponse>(new UserResponse(), false, "VerifyLogin.InvalidCredentials", "Invalid email or password");
            }

            // Map user to response
            var userResponse = new UserResponse
            {
                ID = user.ID,
                Name = user.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                EmailAddress = user.EmailAddress,
                TelephoneNumber = user.TelephoneNumber,
                MobileNumber = user.MobileNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                EmailVerified = user.EmailVerified,
                UserRole = (Shared.Enums.UserType)(user.UserType ?? 0)
            };

            return new ApiResult<UserResponse>(userResponse, true, "VerifyLogin.Success", "Login successful");
        }
    }
}

public class VerifyLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("users/verify-login", async (VerifyLoginRequest request, ISender sender) =>
        {
            var result = await sender.Send(request.Adapt<VerifyLogin.Command>());
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AllowAnonymous(); // Allow anonymous access for login verification
    }
}
