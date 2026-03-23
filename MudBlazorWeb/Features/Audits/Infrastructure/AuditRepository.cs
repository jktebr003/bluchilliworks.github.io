using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Audits.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Audits.Infrastructure;

public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _context;

    public AuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Audit>> GetAllAuditsAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""AuditType"",
                ""AuditUser"",
                ""TableName"",
                ""KeyValues"",
                ""OldValues"",
                ""NewValues"",
                ""ChangedColumns"",
                ""CreatedBy"",
                ""CreatedDate""
            FROM audit.""Audits""
            ORDER BY ""CreatedDate"" DESC";

        var audits = await connection.QueryAsync<Audit>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return audits.ToList();
    }
}
