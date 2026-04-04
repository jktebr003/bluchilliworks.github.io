using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Pricing.Infrastructure;

public class PackageRepository : IPackageRepository
{
    private readonly AppDbContext _context;
    private readonly string _connectionString;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public PackageRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT
                    ""Id"",
                    ""Name"",
                    ""Description"",
                    ""Code"",
                    ""Price"",
                    ""Type"",
                    ""CreatedOn"",
                    ""CreatedBy"",
                    ""ModifiedOn"",
                    ""ModifiedBy"",
                    ""DeletedOn"",
                    ""DeletedBy"",
                    ""IsDeleted""
                FROM catalog.""Packages""
                WHERE ""IsDeleted"" = false
                ORDER BY ""CreatedOn"" DESC";

            var packages = await connection.QueryAsync<Package>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));

            return packages.ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Package?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var sql = @"
                SELECT
                    ""Id"",
                    ""Name"",
                    ""Description"",
                    ""Code"",
                    ""Price"",
                    ""Type"",
                    ""CreatedOn"",
                    ""CreatedBy"",
                    ""ModifiedOn"",
                    ""ModifiedBy"",
                    ""DeletedOn"",
                    ""DeletedBy"",
                    ""IsDeleted""
                FROM catalog.""Packages""
                WHERE ""Id"" = @Id AND ""IsDeleted"" = false";

            var package = await connection.QuerySingleOrDefaultAsync<Package>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

            return package;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SavePackageAsync(Package package, CancellationToken cancellationToken = default)
    {
        await _context.AddAsync(package, cancellationToken);
        _context.SaveChanges(package.CreatedBy, cancellationToken);
    }
}