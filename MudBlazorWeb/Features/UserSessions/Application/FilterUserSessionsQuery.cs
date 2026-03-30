using Carter;

using MediatR;

using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.UserSessions.Application;

public static class FilterUserSessionsQuery
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

	public record Query(string UserId) : IRequest<Result<List<UserSessionDto>>>;

	internal sealed class Handler : IRequestHandler<Query, Result<List<UserSessionDto>>>
	{
		private readonly IUserSessionRepository _userSessionRepository;

		public Handler(IUserSessionRepository userSessionRepository)
		{
			_userSessionRepository = userSessionRepository;
		}

		public async Task<Result<List<UserSessionDto>>> Handle(Query request, CancellationToken cancellationToken)
		{
			var data = await _userSessionRepository.FilterUserSessionsByUserIdAsync(request.UserId);
			var response = data.Select(UserSessionDto.FromEntity).ToList();

			return new Result<List<UserSessionDto>>(response, true);
		}
	}
}

public class FilterUserSessionsQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/usersessions/filter/{userId}", async (string userId, ISender sender) =>
		{
			var query = new FilterUserSessionsQuery.Query(userId);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("UserSessions")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
