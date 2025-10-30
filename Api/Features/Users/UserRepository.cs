using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using MongoDB.Entities;

namespace Api.Features.Users;

public class UserRepository : IUserRepository
{
    private readonly IMongoDbRepository _mongoDbRepository;

    public UserRepository(IMongoDbRepository mongoDbRepository)
        => _mongoDbRepository = mongoDbRepository;

    public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress)
        => DB.Find<User>()
             .Match(b => b.EmailAddress == emailAddress)
             .ExecuteAsync();

    public Task<List<User>> GetAllUsersAsync()
        => _mongoDbRepository.GetAll<User>();

    public Task<User> GetUserByIdAsync(string id)
        => _mongoDbRepository.Get<User>(id);

    public Task SaveUserAsync(User user)
        => _mongoDbRepository.Save(user);
}
