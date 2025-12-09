using Api.Filters;
using Carter;
using MediatR;
using Shared.Enums;
using Shared.Models;

namespace Api.Features.Users;

public static class GetUserDetails
{
    public class Query : IRequest<ApiResult<UserDetailsResponse?>>
    {
        public string Id { get; set; } = string.Empty;
        public string? RequestingUserId { get; set; } // To verify staff role
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<UserDetailsResponse?>>
    {
        private readonly IUserRepository _userRepository;

        public Handler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<ApiResult<UserDetailsResponse?>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Verify requesting user is staff
            if (!string.IsNullOrEmpty(request.RequestingUserId))
            {
                var requestingUser = await _userRepository.GetUserByIdAsync(request.RequestingUserId);
                if (requestingUser == null || requestingUser.UserType != (int)UserType.Staff)
                {
                    return new ApiResult<UserDetailsResponse?>(
                        default(UserDetailsResponse), 
                        false, 
                        "GetUserDetails.Unauthorized", 
                        "Only staff members can access detailed user information"
                    );
                }
            }

            var user = await _userRepository.GetUserByIdAsync(request.Id);
            if (user == null)
            {
                return new ApiResult<UserDetailsResponse?>(
                    default(UserDetailsResponse), 
                    false, 
                    "GetUserDetails.NotFound", 
                    "The user with the specified ID was not found"
                );
            }

            var response = new UserDetailsResponse
            {
                ID = user.ID,
                Name = user.Name,
                Username = user.Username,
                EmailAddress = user.EmailAddress,
                EmailVerified = user.EmailVerified,
                EmailVerificationToken = user.EmailVerificationToken,
                EmailVerificationTokenExpiry = user.EmailVerificationTokenExpiry,
                IsDeleted = user.IsDeleted,
                HashedPassword = user.HashedPassword,
                UserRole = user.UserType is not null ? (UserType)user.UserType : UserType.None,
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy
            };

            return new ApiResult<UserDetailsResponse?>(response, true);
        }
    }
}

public class GetUserDetailsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}/details", async (string id, string? requestingUserId, ISender sender) =>
        {
            var result = await sender.Send(new GetUserDetails.Query 
            { 
                Id = id,
                RequestingUserId = requestingUserId
            });
            return Results.Ok(result);
        })
        .WithTags("Users")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
