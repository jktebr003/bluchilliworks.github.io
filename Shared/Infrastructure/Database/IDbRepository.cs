using MongoDB.Entities;

namespace Shared.Infrastructure.Database;

public interface IDbRepository
{
    public Task Save<T>(T item) where T : Entity;

    public Task<T> Get<T>(string id) where T : Entity;

    public Task<List<T>> GetAll<T>() where T : Entity;
}
