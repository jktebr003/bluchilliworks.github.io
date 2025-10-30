using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;

using MongoDB.Entities;

namespace Api.Features.Users;

public interface IUserRepository
{
    public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress);
    public Task<List<User>> GetAllUsersAsync();
    public Task<User> GetUserByIdAsync(string id);    
    public Task SaveUserAsync(User user);
}
