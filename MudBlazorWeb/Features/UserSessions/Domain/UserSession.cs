using System.ComponentModel.DataAnnotations;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.UserSessions.Domain;

public class UserSession : BaseAuditableEntity
{
    [Key]
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

    public bool HasExpired(DateTimeOffset now)
    {
        return IsExpired
            || !IsActive
            || RevokedOn.HasValue
            || ExpiresOn <= now
            || AbsoluteExpiresOn <= now;
    }
}