using Carter;

using MediatR;

using MudBlazorWeb.Features.Audits.Domain;
using MudBlazorWeb.Filters;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Audits.Application;

public static class GetAuditsQuery
{
    public record AuditDto(
        Guid Id,
        string? AuditType,
        string? AuditUser,
        string? TableName,
        string? KeyValues,
        string? OldValues,
        string? NewValues,
        string? ChangedColumns,
        string? CreatedBy,
        DateTime? CreatedDate)
    {
        public static AuditDto FromEntity(Audit audit)
        {
            return new AuditDto(
                audit.Id,
                audit.AuditType,
                audit.AuditUser,
                audit.TableName,
                audit.KeyValues,
                audit.OldValues,
                audit.NewValues,
                audit.ChangedColumns,
                audit.CreatedBy,
                audit.CreatedDate);
        }
    }

    public record Query(int? PageSize, int? PageNumber) : IRequest<PagedResult<List<AuditDto>>>;

    internal sealed class Handler : IRequestHandler<Query, PagedResult<List<AuditDto>>>
    {
        private readonly IAuditRepository _auditRepository;

        public Handler(IAuditRepository auditRepository) => _auditRepository = auditRepository;

        public async Task<PagedResult<List<AuditDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allData = await _auditRepository.GetAllAuditsAsync(cancellationToken);

            int totalItems = allData.Count;
            int pageSize = request.PageSize.GetValueOrDefault(totalItems);
            int pageNumber = request.PageNumber.GetValueOrDefault(1);

            if (pageSize <= 0) pageSize = totalItems;
            if (pageNumber <= 0) pageNumber = 1;

            int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            int skip = (pageNumber - 1) * pageSize;

            List<Audit> pagedData;
            if (skip < totalItems && pageSize > 0)
            {
                int take = Math.Min(pageSize, totalItems - skip);
                if (allData is List<Audit> list)
                {
                    pagedData = list.GetRange(skip, take);
                }
                else
                {
                    pagedData = new List<Audit>(take);
                    for (int i = skip; i < skip + take; i++)
                    {
                        pagedData.Add(allData[i]);
                    }
                }
            }
            else
            {
                pagedData = allData;
            }

            var response = new List<AuditDto>(pagedData.Count);
            foreach (var audit in pagedData)
            {
                response.Add(AuditDto.FromEntity(audit));
            }

            return new PagedResult<List<AuditDto>>(response, true, totalPages, totalItems, pageNumber, pageSize);
        }
    }
}

public class GetAuditsQueryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/audits", async (int? pageSize, int? pageNumber, ISender sender) =>
        {
            var query = new GetAuditsQuery.Query(pageSize, pageNumber);

            var result = await sender.Send(query);

            return Results.Ok(result);
        }).WithTags("Audits")
          .AddEndpointFilter<AuthenticationFilter>();
    }
}
