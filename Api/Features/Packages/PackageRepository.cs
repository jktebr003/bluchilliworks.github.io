using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;

namespace Api.Features.Packages;

public class PackageRepository : IPackageRepository
{
    private readonly IMongoDbRepository _mongoDbRepository;

    public PackageRepository(IMongoDbRepository mongoDbRepository)
        => _mongoDbRepository = mongoDbRepository;

    public Task<List<Package>> GetAllPackagesAsync()
        => _mongoDbRepository.GetAll<Package>();

    public Task<Package?> GetPackageByIdAsync(string id)
        => _mongoDbRepository.Get<Package>(id);

    public Task SavePackageAsync(Package package)
        => _mongoDbRepository.Save(package);
}
