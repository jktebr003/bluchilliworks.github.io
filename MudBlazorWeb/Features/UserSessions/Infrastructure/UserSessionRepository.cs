using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Authentication.Infrastructure;
using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;

namespace MudBlazorWeb.Features.UserSessions.Infrastructure;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AppDbContext _context;

    private const string SessionSelectColumns = @"
                ""Id"",
                ""UserId"",
                ""SessionTokenHash"",
                ""IdleDuration"",
                ""LastAccessedOn"",
                ""ExpiresOn"",
                ""AbsoluteExpiresOn"",
                ""RevokedOn"",
                ""UserStateVersion"",
                ""IsExpired"",
                ""IsActive"",
                ""CreatedOn"",
                ""CreatedBy"",
                ""ModifiedOn"",
                ""ModifiedBy"",
                ""DeletedOn"",
                ""DeletedBy"",
                ""IsDeleted""";

    public UserSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserSession>> FilterUserSessionsByUserIdAsync(string userId)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = $@"
            SELECT
{SessionSelectColumns}
            FROM users.""UserSessions""
            WHERE ""IsDeleted"" = false AND ""UserId"" = @UserId
            ORDER BY ""CreatedOn"" DESC";

        var rows = await connection.QueryAsync<UserSessionRow>(
            new CommandDefinition(sql, new { UserId = userId }));

        return rows.Select(MapRowToUserSession).ToList();
    }

    public async Task<List<UserSession>> GetAllUserSessionsAsync()
    {
        var connection = _context.Database.GetDbConnection();

        var sql = $@"
            SELECT
{SessionSelectColumns}
            FROM users.""UserSessions""
            WHERE ""IsDeleted"" = false
            ORDER BY ""CreatedOn"" DESC";

        var rows = await connection.QueryAsync<UserSessionRow>(
            new CommandDefinition(sql));

        return rows.Select(MapRowToUserSession).ToList();
    }

    public async Task<UserSession> GetUserSessionByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            throw new ArgumentException("User session ID must be a valid GUID.", nameof(id));
        }

        var row = await FindUserSessionByIdAsync(parsedId);

        return row == null
            ? throw new InvalidOperationException($"User session with ID {id} was not found.")
            : row;
    }

    public async Task<UserSession> GetUserSessionBySessionTokenAsync(string sessionToken)
    {
        var row = await FindUserSessionByTokenHashAsync(SessionTokenHasher.HashToken(sessionToken));

        return row == null
            ? throw new InvalidOperationException($"User session with token {sessionToken} was not found.")
            : row;
    }

    public async Task<UserSession?> FindUserSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(existing => existing.Id == id && !existing.IsDeleted, cancellationToken);
    }

    public async Task<UserSession?> FindUserSessionByTokenHashAsync(string sessionTokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(existing => existing.SessionTokenHash == sessionTokenHash && !existing.IsDeleted, cancellationToken);
    }

    public async Task SaveUserSessionAsync(UserSession userSession)
    {
        await _context.Set<UserSession>().AddAsync(userSession);
        _context.SaveChanges(userSession.CreatedBy, default);
    }

    public async Task UpdateUserSessionAsync(UserSession userSession)
    {
        var trackedUserSession = await _context.Set<UserSession>()
            .AsTracking()
            .SingleOrDefaultAsync(existing => existing.Id == userSession.Id);

        if (trackedUserSession == null)
        {
            throw new InvalidOperationException($"User session with ID {userSession.Id} was not found.");
        }

        var currentRecord = _context.Set<UserSession>().Entry(trackedUserSession);
        currentRecord.CurrentValues.SetValues(userSession);

        _context.SaveChanges(userSession.ModifiedBy ?? "System", default);
    }

    private static UserSession MapRowToUserSession(UserSessionRow row)
    {
        return new UserSession
        {
            Id = row.Id,
            UserId = row.UserId,
            SessionTokenHash = row.SessionTokenHash,
            IdleDuration = row.IdleDuration,
            LastAccessedOn = row.LastAccessedOn,
            ExpiresOn = row.ExpiresOn,
            AbsoluteExpiresOn = row.AbsoluteExpiresOn,
            RevokedOn = row.RevokedOn,
            UserStateVersion = row.UserStateVersion,
            IsExpired = row.IsExpired,
            IsActive = row.IsActive,
            CreatedOn = row.CreatedOn,
            CreatedBy = row.CreatedBy,
            ModifiedOn = row.ModifiedOn,
            ModifiedBy = row.ModifiedBy,
            DeletedOn = row.DeletedOn,
            DeletedBy = row.DeletedBy,
            IsDeleted = row.IsDeleted,
        };
    }

    private sealed class UserSessionRow
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string SessionTokenHash { get; set; } = string.Empty;
        public int IdleDuration { get; set; }
        public DateTimeOffset LastAccessedOn { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
        public DateTimeOffset AbsoluteExpiresOn { get; set; }
        public DateTimeOffset? RevokedOn { get; set; }
        public string UserStateVersion { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}