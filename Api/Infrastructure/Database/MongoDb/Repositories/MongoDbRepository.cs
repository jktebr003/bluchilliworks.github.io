using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Repositories;

public class MongoDbRepository : IMongoDbRepository
{
    public Task<T> Get<T>(string id) where T : Entity
        => DB.Find<T>()
             .Match(b => b.ID == id)
             .ExecuteSingleAsync();

    public Task<List<T>> GetAll<T>() where T : Entity
        => DB.Find<T>()
             .Match(_ => true)
             .ExecuteAsync();

    public Task Save<T>(T item) where T : Entity
        => item.SaveAsync();
}