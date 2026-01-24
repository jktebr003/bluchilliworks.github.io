using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using MongoDB.Entities;
using Shared.Enums;

namespace Api.Features.Users;

public class UserRepository : IUserRepository
{
    private readonly IMongoDbRepository _mongoDbRepository;

    public UserRepository(IMongoDbRepository mongoDbRepository)
        => _mongoDbRepository = mongoDbRepository;

    public Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress)
        => DB.Find<User>()
             .Match(b => b.EmailAddress == emailAddress && !b.IsDeleted)
             .ExecuteAsync();

    public Task<User?> GetUserByEmailAddressAsync(string? emailAddress)
        => DB.Find<User>()
             .Match(b => b.EmailAddress == emailAddress && !b.IsDeleted)
             .ExecuteSingleAsync();

    public Task<List<User>> GetAllUsersAsync()
        => _mongoDbRepository.GetAll<User>();

    public Task<User> GetUserByIdAsync(string id)
        => _mongoDbRepository.Get<User>(id);

    public Task SaveUserAsync(User user)
        => _mongoDbRepository.Save(user);

    public async Task<List<User>> SearchUsersAsync(
        string? search = null,
        UserType? role = null,
        string? gender = null,
        string? package = null,
        bool? emailVerified = null,
        DateTime? dobFrom = null,
        DateTime? dobTo = null)
    {
        var query = DB.Find<User>().Match(u => !u.IsDeleted);

        // Apply text search filter (searches across multiple fields)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Match(u =>
                (u.Name != null && u.Name.ToLower().Contains(searchLower)) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower)) ||
                (u.Username != null && u.Username.ToLower().Contains(searchLower)) ||
                (u.EmailAddress != null && u.EmailAddress.ToLower().Contains(searchLower)) ||
                (u.MobileNumber != null && u.MobileNumber.Contains(search))
            );
        }

        // Apply role filter
        if (role.HasValue)
        {
            query = query.Match(u => u.UserType == (short)role.Value);
        }

        // Apply gender filter
        if (!string.IsNullOrWhiteSpace(gender))
        {
            query = query.Match(u => u.Gender != null && u.Gender.ToLower() == gender.ToLower());
        }

        // Apply package filter
        if (!string.IsNullOrWhiteSpace(package))
        {
            query = query.Match(u => u.Package != null && u.Package.Name != null && u.Package.Name.ToLower().Contains(package.ToLower()));
        }

        // Apply email verified filter
        if (emailVerified.HasValue)
        {
            query = query.Match(u => u.EmailVerified == emailVerified.Value);
        }

        // Apply date of birth range filters
        if (dobFrom.HasValue)
        {
            var dobFromStr = dobFrom.Value.ToString("yyyy-MM-dd");
            query = query.Match(u => u.DateOfBirth != null && string.Compare(u.DateOfBirth, dobFromStr) >= 0);
        }

        if (dobTo.HasValue)
        {
            var dobToStr = dobTo.Value.ToString("yyyy-MM-dd");
            query = query.Match(u => u.DateOfBirth != null && string.Compare(u.DateOfBirth, dobToStr) <= 0);
        }

        return await query.ExecuteAsync();
    }
}
