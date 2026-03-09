namespace MudBlazorWeb.Features.Pricing.Domain;

public interface IPackageRepository
{
    Task<List<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default);
    Task<Package?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task SavePackageAsync(Package package, CancellationToken cancellationToken = default);
}
