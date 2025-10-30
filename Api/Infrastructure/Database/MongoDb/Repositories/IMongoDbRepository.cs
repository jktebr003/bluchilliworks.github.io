using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Repositories;

public interface IMongoDbRepository
{
    public Task Save<T>(T item) where T : Entity;

    public Task<T> Get<T>(string id) where T : Entity;

    public Task<List<T>> GetAll<T>() where T : Entity;
}
