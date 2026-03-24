using System.ComponentModel.DataAnnotations;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.UserSessions.Domain;

public class UserSession : BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? SessionToken { get; set; }
    public int IdleDuration { get; set; }
    public string? LastAccessedOn { get; set; }
    public string? ExpiresOn { get; set; }
    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
}