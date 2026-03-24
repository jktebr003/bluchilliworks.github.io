namespace MudBlazorWeb.Features.UserSessions.Domain;

public interface IUserSessionRepository
{
    public Task<List<UserSession>> FilterUserSessionsByUserIdAsync(string userId);
    public Task<List<UserSession>> GetAllUserSessionsAsync();    
    public Task<UserSession> GetUserSessionByIdAsync(string id);
    public Task<UserSession> GetUserSessionBySessionTokenAsync(string sessionToken);
    public Task SaveUserSessionAsync(UserSession userSession);
    public Task UpdateUserSessionAsync(UserSession userSession);
}