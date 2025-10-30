using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using MongoDB.Entities;

namespace Api.Features.UserSessions;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly IMongoDbRepository _mongoDbRepository;

    public UserSessionRepository(IMongoDbRepository mongoDbRepository)
        => _mongoDbRepository = mongoDbRepository;

    public Task<List<UserSession>> FilterUserSessionsByUserIdAsync(string userId)
        => DB.Find<UserSession>()
             .Match(b => b.UserId == userId)
             .ExecuteAsync();

    public Task<List<UserSession>> GetAllUserSessionsAsync()
        => _mongoDbRepository.GetAll<UserSession>();

    public Task<UserSession> GetUserSessionByIdAsync(string id)
        => _mongoDbRepository.Get<UserSession>(id);

    public Task<UserSession?> GetUserSessionBySessionKeyAsync(string sessionKey)
        => DB.Find<UserSession>()
             .Match(b => b.SessionKey == sessionKey)
             .ExecuteFirstAsync();

    public Task SaveUserSessionAsync(UserSession userSession)
        => _mongoDbRepository.Save(userSession);
}
