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
        return await _context.Packages
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<Package?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Packages
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task SavePackageAsync(Package package, CancellationToken cancellationToken = default)
    {
        await _context.AddAsync(package, cancellationToken);
        _context.SaveChanges(package.CreatedBy, cancellationToken);
    }
}
