using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Authentication.Application;

public static class LoginCommand
{
    public record Command(string Username, string Password) : IRequest<Result<UserResponse>>;

    internal sealed class Handler : IRequestHandler<Command, Result<UserResponse>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Failure("Email address and password are required.", "VerifyLogin.Validation");
                }

                var user = await _userRepository.GetUserByEmailAddressAsync(request.Username, cancellationToken);
                if (user == null)
                {
                    return Failure("Invalid email or password", "VerifyLogin.InvalidCredentials");
                }

                if (!user.EmailVerified)
                {
                    return Failure("Please verify your email address before logging in. Check your inbox for the verification email.", "VerifyLogin.EmailNotVerified");
                }

                if (string.IsNullOrWhiteSpace(user.HashedPassword))
                {
                    return Failure("Please complete your password setup. Check your email for instructions.", "VerifyLogin.PasswordNotSet");
                }

                if (!AuthenticationCommandHelpers.VerifyPassword(user.HashedPassword, request.Password))
                {
                    return Failure("Invalid email or password", "VerifyLogin.InvalidCredentials");
                }

                return new Result<UserResponse>(AuthenticationCommandHelpers.MapToUserResponse(user), true, "VerifyLogin.Success", "Login successful");
            }
            catch (Exception ex)
            {
                return Failure($"Login failed: {ex.Message}", "Authentication.LoginFailed");
            }
        }

        private static Result<UserResponse> Failure(string message, string? code = null)
            => new(AuthenticationCommandHelpers.EmptyUserResponse(), false, code, message);
    }
}