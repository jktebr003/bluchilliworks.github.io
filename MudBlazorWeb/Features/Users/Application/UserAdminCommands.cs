using System.Security.Cryptography;
using System.Text;

using MediatR;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users.Application;

public static class GetUserDetailsQuery
{
    public record Query(Guid UserId, Guid? RequestingUserId) : IRequest<Result<UserDetailsResponse>>;

    internal sealed class Handler : IRequestHandler<Query, Result<UserDetailsResponse>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (request.RequestingUserId.HasValue)
            {
                try
                {
                    var requestingUser = await _userRepository.GetUserByIdAsync(request.RequestingUserId.Value, cancellationToken);
                    if (requestingUser.UserType != (int)UserType.Staff)
                    {
                        return new Result<UserDetailsResponse>(new UserDetailsResponse(), false, "GetUserDetails.Unauthorized", "Only staff members can access detailed user information.");
                    }
                }
                catch (InvalidOperationException)
                {
                    return new Result<UserDetailsResponse>(new UserDetailsResponse(), false, "GetUserDetails.Unauthorized", "Only staff members can access detailed user information.");
                }
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
                return new Result<UserDetailsResponse>(MapToUserDetailsResponse(user), true);
            }
            catch (InvalidOperationException)
            {
                return new Result<UserDetailsResponse>(new UserDetailsResponse(), false, "GetUserDetails.NotFound", "The user with the specified ID was not found.");
            }
        }

        private static UserDetailsResponse MapToUserDetailsResponse(User user)
        {
            return new UserDetailsResponse
            {
                ID = user.Id.ToString(),
                Name = user.Name,
                Username = user.Username,
                EmailAddress = user.EmailAddress,
                EmailVerified = user.EmailVerified,
                EmailVerificationToken = user.EmailVerificationToken,
                EmailVerificationTokenExpiry = user.EmailVerificationTokenExpiry,
                IsDeleted = user.IsDeleted,
                HashedPassword = user.HashedPassword,
                UserRole = (UserType)user.UserType,
                CreatedOn = user.CreatedOn.ToString("O"),
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn?.ToString("O"),
                ModifiedBy = user.ModifiedBy,
                DeletedOn = user.DeletedOn?.ToString("O"),
                DeletedBy = user.DeletedBy
            };
        }
    }
}

public static class ChangeUserPasswordCommand
{
    public record Command(Guid UserId, string NewPassword, Guid? RequestingUserId) : IRequest<Result<string>>;

    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.RequestingUserId.HasValue)
            {
                try
                {
                    var requestingUser = await _userRepository.GetUserByIdAsync(request.RequestingUserId.Value, cancellationToken);
                    if (requestingUser.UserType != (int)UserType.Staff)
                    {
                        return new Result<string>(string.Empty, false, "ChangeUserPassword.Unauthorized", "Only staff members can change user passwords.");
                    }
                }
                catch (InvalidOperationException)
                {
                    return new Result<string>(string.Empty, false, "ChangeUserPassword.Unauthorized", "Only staff members can change user passwords.");
                }
            }

            var validationMessage = ValidatePassword(request.NewPassword);
            if (validationMessage != null)
            {
                return new Result<string>(string.Empty, false, "ChangeUserPassword.Validation", validationMessage);
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
                user.HashedPassword = HashPassword(request.NewPassword);
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = request.RequestingUserId?.ToString() ?? "system";

                await _userRepository.UpdateUserAsync(user, cancellationToken);

                return new Result<string>("Password changed successfully", true);
            }
            catch (InvalidOperationException)
            {
                return new Result<string>(string.Empty, false, "ChangeUserPassword.NotFound", "The user with the specified ID was not found.");
            }
        }

        private static string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password is required.";
            }

            if (password.Length < 8)
            {
                return "Password must be at least 8 characters long.";
            }

            if (!password.Any(char.IsUpper))
            {
                return "Password must contain at least one uppercase letter.";
            }

            if (!password.Any(char.IsLower))
            {
                return "Password must contain at least one lowercase letter.";
            }

            if (!password.Any(char.IsDigit))
            {
                return "Password must contain at least one number.";
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return "Password must contain at least one special character.";
            }

            return null;
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}

public static class ChangeUserRoleCommand
{
    public record Command(Guid UserId, UserType NewRole, Guid? RequestingUserId) : IRequest<Result<string>>;

    internal sealed class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.RequestingUserId.HasValue)
            {
                try
                {
                    var requestingUser = await _userRepository.GetUserByIdAsync(request.RequestingUserId.Value, cancellationToken);
                    if (requestingUser.UserType != (int)UserType.Staff)
                    {
                        return new Result<string>(string.Empty, false, "ChangeUserRole.Unauthorized", "Only staff members can change user roles.");
                    }
                }
                catch (InvalidOperationException)
                {
                    return new Result<string>(string.Empty, false, "ChangeUserRole.Unauthorized", "Only staff members can change user roles.");
                }
            }

            try
            {
                var user = await _userRepository.GetUserByIdAsync(request.UserId, cancellationToken);
                user.UserType = (int)request.NewRole;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = request.RequestingUserId?.ToString() ?? "system";

                await _userRepository.UpdateUserAsync(user, cancellationToken);

                return new Result<string>($"User role updated to {request.NewRole} successfully", true);
            }
            catch (InvalidOperationException)
            {
                return new Result<string>(string.Empty, false, "ChangeUserRole.NotFound", "The user with the specified ID was not found.");
            }
        }
    }
}