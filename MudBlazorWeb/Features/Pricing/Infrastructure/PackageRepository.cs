using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.Pricing.Infrastructure;

public class PackageRepository : IPackageRepository
{
    private readonly AppDbContext _context;

    public PackageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

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
            FROM ""Packages""
            WHERE ""IsDeleted"" = false
            ORDER BY ""CreatedOn"" DESC";

        var packages = await connection.QueryAsync<Package>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return packages.ToList();
    }

    public async Task<Package?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

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
            FROM ""Packages""
            WHERE ""Id"" = @Id AND ""IsDeleted"" = false";

        var package = await connection.QuerySingleOrDefaultAsync<Package>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return package;
    }

    public async Task SavePackageAsync(Package package, CancellationToken cancellationToken = default)
    {
        await _context.AddAsync(package, cancellationToken);
        _context.SaveChanges(package.CreatedBy, cancellationToken);
    }
}
