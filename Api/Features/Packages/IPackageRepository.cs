using Api.Infrastructure.Database.MongoDb.Entities;

namespace Api.Features.Packages;

public interface IPackageRepository
{
    //public Task<List<Package>> FilterPackagesByUserIdAsync(string userId);
    Task<List<Package>> GetAllPackagesAsync();
    Task<Package?> GetPackageByIdAsync(string id);
    //Task<string> CreatePostAsync(Post post);
    //Task<bool> UpdatePostAsync(Post post);
    //Task<bool> DeletePostAsync(string id);
    public Task SavePackageAsync(Package package);
}
