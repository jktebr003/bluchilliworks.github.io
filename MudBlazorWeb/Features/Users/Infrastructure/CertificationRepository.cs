using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class CertificationRepository : ICertificationRepository
{
    private readonly AppDbContext _context;

    public CertificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Certification>> GetCertificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""UserId"",
                ""Title"",
                ""Institution"",
                ""Year""
            FROM users.""Certifications""
            WHERE ""UserId"" = @UserId";

        var rows = await connection.QueryAsync<Certification>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task<Certification?> GetCertificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""UserId"",
                ""Title"",
                ""Institution"",
                ""Year""
            FROM users.""Certifications""
            WHERE ""Id"" = @Id";

        return await connection.QuerySingleOrDefaultAsync<Certification>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task CreateCertificationAsync(Certification certification, CancellationToken cancellationToken = default)
    {
        await _context.Certifications.AddAsync(certification, cancellationToken);
        _context.SaveChanges(certification.CreatedBy ?? "System", cancellationToken);
    }

    public async Task UpdateCertificationAsync(Certification certification, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Certifications
            .AsTracking()
            .SingleOrDefaultAsync(c => c.Id == certification.Id, cancellationToken);

        if (tracked == null)
        {
            throw new InvalidOperationException($"Certification with ID {certification.Id} was not found.");
        }

        var currentRecord = _context.Set<Certification>().Entry(tracked);
        currentRecord.CurrentValues.SetValues(certification);

        _context.SaveChanges(certification.ModifiedBy ?? "System", cancellationToken);
    }
}
