using System.Security.Claims;
using System.Security.Cryptography;

using Microsoft.Extensions.Options;

using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.UserSessions.Domain;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal interface IUserSessionManager
{
    Task<SessionCreationResult> CreateSessionAsync(AuthenticationUser user, CancellationToken cancellationToken = default);
    Task<SessionValidationResult> ValidateSessionTokenAsync(string rawSessionToken, bool markActivity, CancellationToken cancellationToken = default);
    Task<SessionValidationResult> ValidatePrincipalAsync(ClaimsPrincipal principal, bool markActivity, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

internal sealed record SessionCreationResult(string SessionToken, ClaimsPrincipal Principal, DateTimeOffset AbsoluteExpiresOn);

internal sealed record SessionValidationResult(bool IsAuthenticated, ClaimsPrincipal? Principal, string? FailureReason = null);

internal sealed class UserSessionManager : IUserSessionManager
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IAuthenticationUserStore _authenticationUserStore;
    private readonly SessionAuthenticationOptions _options;

    public UserSessionManager(
        IUserSessionRepository userSessionRepository,
        IAuthenticationUserStore authenticationUserStore,
        IOptions<SessionAuthenticationOptions> options)
    {
        _userSessionRepository = userSessionRepository;
        _authenticationUserStore = authenticationUserStore;
        _options = options.Value;
    }

    public async Task<SessionCreationResult> CreateSessionAsync(AuthenticationUser user, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var rawSessionToken = GenerateToken();

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id.ToString(),
            SessionTokenHash = SessionTokenHasher.HashToken(rawSessionToken),
            IdleDuration = _options.IdleTimeoutMinutes,
            LastAccessedOn = now,
            ExpiresOn = now.AddMinutes(_options.IdleTimeoutMinutes),
            AbsoluteExpiresOn = now.AddHours(_options.AbsoluteLifetimeHours),
            UserStateVersion = SessionUserStateVersionFactory.Create(user),
            IsExpired = false,
            IsActive = true,
            CreatedOn = now.UtcDateTime,
            CreatedBy = user.EmailAddress
        };

        await _userSessionRepository.SaveUserSessionAsync(session);

        return new SessionCreationResult(
            rawSessionToken,
            SessionPrincipalFactory.Create(user, session),
            session.AbsoluteExpiresOn);
    }

    public async Task<SessionValidationResult> ValidateSessionTokenAsync(string rawSessionToken, bool markActivity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawSessionToken))
        {
            return Failed("Session token was missing.");
        }

        var sessionTokenHash = SessionTokenHasher.HashToken(rawSessionToken);
        var session = await _userSessionRepository.FindUserSessionByTokenHashAsync(sessionTokenHash, cancellationToken);
        return await ValidateSessionAsync(session, markActivity, cancellationToken);
    }

    public async Task<SessionValidationResult> ValidatePrincipalAsync(ClaimsPrincipal principal, bool markActivity, CancellationToken cancellationToken = default)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return Failed("The current principal is anonymous.");
        }

        if (!TryGetSessionId(principal, out var sessionId))
        {
            return Failed("Session claim was missing.");
        }

        var session = await _userSessionRepository.FindUserSessionByIdAsync(sessionId, cancellationToken);
        return await ValidateSessionAsync(session, markActivity, cancellationToken);
    }

    public async Task RevokeSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (!TryGetSessionId(principal, out var sessionId))
        {
            return;
        }

        var session = await _userSessionRepository.FindUserSessionByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        session.IsActive = false;
        session.RevokedOn = now;
        session.IsExpired = session.ExpiresOn <= now || session.AbsoluteExpiresOn <= now || session.IsExpired;
        session.ModifiedOn = now.UtcDateTime;
        session.ModifiedBy = principal.FindFirstValue(ClaimTypes.Email) ?? principal.Identity?.Name ?? "System";

        await _userSessionRepository.UpdateUserSessionAsync(session);
    }

    private async Task<SessionValidationResult> ValidateSessionAsync(UserSession? session, bool markActivity, CancellationToken cancellationToken)
    {
        if (session == null)
        {
            return Failed("Session was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        if (session.HasExpired(now))
        {
            await MarkExpiredAsync(session, now);
            return Failed("Session expired.");
        }

        if (!Guid.TryParse(session.UserId, out var userId))
        {
            await InvalidateAsync(session, now, "Session user is invalid.");
            return Failed("Session user is invalid.");
        }

        var user = await _authenticationUserStore.GetByIdAsync(userId, cancellationToken);
        if (user == null || user.IsDeleted || !user.EmailVerified)
        {
            await InvalidateAsync(session, now, "Session user is no longer valid.");
            return Failed("Session user is no longer valid.");
        }

        var expectedUserStateVersion = SessionUserStateVersionFactory.Create(user);
        if (!string.Equals(session.UserStateVersion, expectedUserStateVersion, StringComparison.Ordinal))
        {
            await InvalidateAsync(session, now, "User security state changed.");
            return Failed("User security state changed.");
        }

        if (markActivity && ShouldRefreshActivity(session, now))
        {
            session.LastAccessedOn = now;
            session.ExpiresOn = Min(now.AddMinutes(session.IdleDuration), session.AbsoluteExpiresOn);
            session.ModifiedOn = now.UtcDateTime;
            session.ModifiedBy = user.EmailAddress;
            await _userSessionRepository.UpdateUserSessionAsync(session);
        }

        return new SessionValidationResult(true, SessionPrincipalFactory.Create(user, session));
    }

    private async Task MarkExpiredAsync(UserSession session, DateTimeOffset now)
    {
        if (!session.IsExpired || session.IsActive)
        {
            session.IsExpired = true;
            session.IsActive = false;
            session.ModifiedOn = now.UtcDateTime;
            session.ModifiedBy = session.ModifiedBy ?? "System";
            await _userSessionRepository.UpdateUserSessionAsync(session);
        }
    }

    private async Task InvalidateAsync(UserSession session, DateTimeOffset now, string modifiedBy)
    {
        session.IsActive = false;
        session.RevokedOn = now;
        session.ModifiedOn = now.UtcDateTime;
        session.ModifiedBy = modifiedBy;

        await _userSessionRepository.UpdateUserSessionAsync(session);
    }

    private bool ShouldRefreshActivity(UserSession session, DateTimeOffset now)
    {
        var refreshThreshold = TimeSpan.FromSeconds(_options.ActivityRefreshThrottleSeconds);
        return now - session.LastAccessedOn >= refreshThreshold;
    }

    private static DateTimeOffset Min(DateTimeOffset left, DateTimeOffset right)
    {
        return left <= right ? left : right;
    }

    private static bool TryGetSessionId(ClaimsPrincipal principal, out Guid sessionId)
    {
        var rawSessionId = principal.FindFirstValue(SessionAuthenticationDefaults.SessionIdClaimType);
        return Guid.TryParse(rawSessionId, out sessionId);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static SessionValidationResult Failed(string reason)
    {
        return new SessionValidationResult(false, null, reason);
    }
}