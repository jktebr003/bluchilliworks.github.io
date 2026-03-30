using Carter;

using MediatR;

using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.UserSessions.Application;

public static class GetUserSessionBySessionTokenQuery
{
	public record UserSessionDto(
		string? UserId,
		string? SessionTokenHash,
		int IdleDuration,
		string? LastAccessedOn,
		string? ExpiresOn,
		string? AbsoluteExpiresOn,
		string? RevokedOn,
		bool IsExpired,
		bool IsActive) : BaseAuditableDto
	{
		public static UserSessionDto FromEntity(UserSession userSession)
		{
			return new UserSessionDto(
				userSession.UserId,
				userSession.SessionTokenHash,
				userSession.IdleDuration,
				userSession.LastAccessedOn.ToString("O"),
				userSession.ExpiresOn.ToString("O"),
				userSession.AbsoluteExpiresOn.ToString("O"),
				userSession.RevokedOn?.ToString("O"),
				userSession.IsExpired,
				userSession.IsActive)
			{
				Id = userSession.Id,
				CreatedOn = userSession.CreatedOn,
				CreatedBy = userSession.CreatedBy,
				ModifiedOn = userSession.ModifiedOn,
				ModifiedBy = userSession.ModifiedBy,
				DeletedOn = userSession.DeletedOn,
				DeletedBy = userSession.DeletedBy,
				IsDeleted = userSession.IsDeleted
			};
		}
	}

	public record Query(string SessionToken) : IRequest<Result<UserSessionDto>>;

	internal sealed class Handler : IRequestHandler<Query, Result<UserSessionDto>>
	{
		private readonly IUserSessionRepository _userSessionRepository;

		public Handler(IUserSessionRepository userSessionRepository)
		{
			_userSessionRepository = userSessionRepository;
		}

		public async Task<Result<UserSessionDto>> Handle(Query request, CancellationToken cancellationToken)
		{
			try
			{
				var data = await _userSessionRepository.GetUserSessionBySessionTokenAsync(request.SessionToken);
				return new Result<UserSessionDto>(UserSessionDto.FromEntity(data), true);
			}
			catch (InvalidOperationException)
			{
				return new Result<UserSessionDto>(UserSessionDtoEmpty.Instance, false, "GetUserSessionBySessionToken.Null", "The user session with the specified session token was not found");
			}
		}

		private sealed record UserSessionDtoEmpty : UserSessionDto
		{
			public static readonly UserSessionDtoEmpty Instance = new();
			private UserSessionDtoEmpty() : base(null, null, 0, null, null, null, null, false, false) { }
		}
	}
}

public class GetUserSessionBySessionTokenQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/usersessions/session/{sessionToken}", async (string sessionToken, ISender sender) =>
		{
			var query = new GetUserSessionBySessionTokenQuery.Query(sessionToken);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("UserSessions")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
