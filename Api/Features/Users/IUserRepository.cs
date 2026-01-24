using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;

using MongoDB.Entities;
using Shared.Enums;

namespace Api.Features.Users;

public interface IUserRepository
{
    public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress);
    public Task<User?> GetUserByEmailAddressAsync(string? emailAddress);
    public Task<List<User>> GetAllUsersAsync();
    public Task<User> GetUserByIdAsync(string id);    
    public Task SaveUserAsync(User user);
    public Task<List<User>> SearchUsersAsync(
        string? search = null,
        UserType? role = null,
        string? gender = null,
        string? package = null,
        bool? emailVerified = null,
        DateTime? dobFrom = null,
        DateTime? dobTo = null
    );
}
