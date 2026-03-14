using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Audits.Domain;

public interface IAuditRepository
{
    Task<List<Audit>> GetAllAuditsAsync(CancellationToken cancellationToken = default);
}
