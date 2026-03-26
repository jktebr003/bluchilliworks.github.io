using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Job>> GetJobsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""UserId"",
                ""Company"",
                ""Position"",
                ""StartDate"",
                ""EndDate"",
                ""Responsibilities""
            FROM users.""Jobs""
            WHERE ""UserId"" = @UserId AND ""IsDeleted"" = false";

        var rows = await connection.QueryAsync<Job>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task<Job?> GetJobAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                ""Id"",
                ""UserId"",
                ""Company"",
                ""Position"",
                ""StartDate"",
                ""EndDate"",
                ""Responsibilities""
            FROM users.""Jobs""
            WHERE ""Id"" = @Id";

        return await connection.QuerySingleOrDefaultAsync<Job>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task CreateJobAsync(Job job, CancellationToken cancellationToken = default)
    {
        await _context.Jobs.AddAsync(job, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateJobAsync(Job job, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Jobs
            .AsTracking()
            .SingleOrDefaultAsync(j => j.Id == job.Id, cancellationToken);

        if (tracked == null)
        {
            throw new InvalidOperationException($"Job with ID {job.Id} was not found.");
        }

        _context.Entry(tracked).CurrentValues.SetValues(job);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
