using Api.Filters;
using Carter;
using MediatR;
using Shared.Models;

namespace Api.Features.UserSessions;

public static class GetUserSession
{
    public class Query : IRequest<ApiResult<UserSessionResponse>>
    {
        public string Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, ApiResult<UserSessionResponse>>
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public Handler(IUserSessionRepository userSessionRepository) => _userSessionRepository = userSessionRepository;

        public async Task<ApiResult<UserSessionResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Remove ConfigureAwait(false) for ASP.NET Core context consistency
            var data = await _userSessionRepository.GetUserSessionByIdAsync(request.Id);
            if (data == null)
            {
                // Use static empty response to avoid repeated allocations
                return new ApiResult<UserSessionResponse>(UserSessionResponseEmpty.Instance, false, "GetUserSession.Null", "The user session with the specified ID was not found");
            }

            // Use object initializer directly in return for clarity and efficiency
            return new ApiResult<UserSessionResponse>(
                new UserSessionResponse
                {
                    ID = data.ID,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    ExpiresOn = data.ExpiresOn,
                    IdleDuration = data.IdleDuration,
                    IsActive = data.IsActive,
                    IsExpired = data.IsExpired,
                    LastAccessedOn = data.LastAccessedOn,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedOn = data.ModifiedOn,
                    SessionToken = data.SessionToken,
                    UserId = data.UserId,
                },
                true
            );
        }

        // Static empty response to avoid unnecessary allocations
        private sealed class UserSessionResponseEmpty : UserSessionResponse
        {
            public static readonly UserSessionResponseEmpty Instance = new();
            private UserSessionResponseEmpty() { }
        }
    }
}

public class GetUserSessionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("usersessions/{id}", async (string id, ISender sender) =>
        {
            // Inline query creation for reduced stack usage
            var result = await sender.Send(new GetUserSession.Query { Id = id });
            return Results.Ok(result);
        })
        .WithTags("User Sessions")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
