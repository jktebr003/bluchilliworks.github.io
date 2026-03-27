using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class QualificationRepository : IQualificationRepository
{
    private readonly AppDbContext _context;

    public QualificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Qualification>> GetQualificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""UserId"",
                ""Title"",
                ""Institution"",
                ""Year""
            FROM users.""Qualifications""
            WHERE ""UserId"" = @UserId";

        var rows = await connection.QueryAsync<Qualification>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task<Qualification?> GetQualificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""UserId"",
                ""Title"",
                ""Institution"",
                ""Year""
            FROM users.""Qualifications""
            WHERE ""Id"" = @Id";

        return await connection.QuerySingleOrDefaultAsync<Qualification>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task CreateQualificationAsync(Qualification qualification, CancellationToken cancellationToken = default)
    {
        await _context.Qualifications.AddAsync(qualification, cancellationToken);
        _context.SaveChanges(qualification.CreatedBy ?? "System", cancellationToken);
    }

    public async Task UpdateQualificationAsync(Qualification qualification, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Qualifications
            .AsTracking()
            .SingleOrDefaultAsync(q => q.Id == qualification.Id, cancellationToken);

        if (tracked == null)
        {
            throw new InvalidOperationException($"Qualification with ID {qualification.Id} was not found.");
        }

        var currentRecord = _context.Set<Qualification>().Entry(tracked);
        currentRecord.CurrentValues.SetValues(qualification);

        _context.SaveChanges(qualification.ModifiedBy ?? "System", cancellationToken);
    }
}

