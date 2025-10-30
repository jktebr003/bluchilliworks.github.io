using Api.Filters;
using Api.Infrastructure.Database.MongoDb.Entities;

using Carter;

using MediatR;

using Shared.Models;

namespace Api.Features.UserSessions;

public static class GetUserSessions
{
    public class Query : IRequest<PagedApiResult<List<UserSessionResponse>>>
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, PagedApiResult<List<UserSessionResponse>>>
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public Handler(IUserSessionRepository userSessionRepository) => _userSessionRepository = userSessionRepository;

        public async Task<PagedApiResult<List<UserSessionResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Fetch all user sessions (repository does not support server-side paging)
            var allData = await _userSessionRepository.GetAllUserSessionsAsync();

            int totalItems = allData.Count;
            int pageSize = request.PageSize.GetValueOrDefault(totalItems);
            int pageNumber = request.PageNumber.GetValueOrDefault(1);

            // Clamp pageSize and pageNumber to valid ranges
            if (pageSize <= 0) pageSize = totalItems;
            if (pageNumber <= 0) pageNumber = 1;

            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            int skip = (pageNumber - 1) * pageSize;

            // Efficiently get the paged data
            List<UserSession> pagedData;
            if (skip < totalItems && pageSize > 0)
            {
                int take = Math.Min(pageSize, totalItems - skip);
                if (allData is List<UserSession> list)
                {
                    pagedData = list.GetRange(skip, take);
                }
                else
                {
                    pagedData = new List<UserSession>(take);
                    for (int i = skip; i < skip + take; i++)
                        pagedData.Add(allData[i]);
                }
            }
            else
            {
                pagedData = allData;
            }

            // Pre-size the response list for efficiency
            var response = new List<UserSessionResponse>(pagedData.Count);
            foreach (var q in pagedData)
            {
                response.Add(new UserSessionResponse
                {
                    ID = q.ID,
                    CreatedBy = q.CreatedBy,
                    CreatedOn = q.CreatedOn,
                    IdleDuration = q.IdleDuration,
                    IsActive = q.IsActive,
                    IsExpired = q.IsExpired,
                    LastAccessedOn = q.LastAccessedOn,
                    ModifiedBy = q.ModifiedBy,
                    ModifiedOn = q.ModifiedOn,
                    SessionKey = q.SessionKey,
                    UserId = q.UserId,
                });
            }

            return new PagedApiResult<List<UserSessionResponse>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetUserSessionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("usersessions", async (int? pageSize, int? pageNumber, ISender sender) =>
            Results.Ok(await sender.Send(new GetUserSessions.Query { PageSize = pageSize, PageNumber = pageNumber }))
        )
        .WithTags("User Sessions")
        .AddEndpointFilter<AuthenticationFilter>();
    }
}
