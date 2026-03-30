using Carter;

using MediatR;

using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.UserSessions.Application;

public static class GetUserSessionsQuery
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

	public record Query(int? PageSize, int? PageNumber) : IRequest<PagedResult<List<UserSessionDto>>>;

	internal sealed class Handler : IRequestHandler<Query, PagedResult<List<UserSessionDto>>>
	{
		private readonly IUserSessionRepository _userSessionRepository;

		public Handler(IUserSessionRepository userSessionRepository)
		{
			_userSessionRepository = userSessionRepository;
		}

		public async Task<PagedResult<List<UserSessionDto>>> Handle(Query request, CancellationToken cancellationToken)
		{
			var allData = await _userSessionRepository.GetAllUserSessionsAsync();

			int totalItems = allData.Count;
			int pageSize = request.PageSize.GetValueOrDefault(totalItems == 0 ? 1 : totalItems);
			int pageNumber = request.PageNumber.GetValueOrDefault(1);

			if (pageSize <= 0) pageSize = totalItems == 0 ? 1 : totalItems;
			if (pageNumber <= 0) pageNumber = 1;

			int totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling((double)totalItems / pageSize);
			int skip = (pageNumber - 1) * pageSize;

			List<UserSession> pagedData;
			if (skip < totalItems)
			{
				int take = Math.Min(pageSize, totalItems - skip);
				pagedData = allData.GetRange(skip, take);
			}
			else
			{
				pagedData = [];
			}

			var response = pagedData.Select(UserSessionDto.FromEntity).ToList();

			return new PagedResult<List<UserSessionDto>>(response, true, totalPages, totalItems, pageNumber, pageSize);
		}
	}
}

public class GetUserSessionsQueryEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("api/usersessions", async (int? pageSize, int? pageNumber, ISender sender) =>
		{
			var query = new GetUserSessionsQuery.Query(pageSize, pageNumber);
			var result = await sender.Send(query);

			return Results.Ok(result);
		}).WithTags("UserSessions")
		  .AddEndpointFilter<AuthenticationFilter>();
	}
}
