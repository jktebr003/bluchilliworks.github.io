using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

[Collection("usersessions")]
public class UserSession : BaseAuditableEntity
{
    [Field("userid")]
    public string? UserId { get; set; }

    [Field("sessiontoken")]
    public string? SessionToken { get; set; }

    [Field("idleduration")]
    public int IdleDuration { get; set; }

    [Field("lastaccessedon")]
    public string? LastAccessedOn { get; set; }

    [Field("expireson")]
    public string? ExpiresOn { get; set; }

    [Field("isexpired")]
    public bool IsExpired { get; set; }

    [Field("isactive")]
    public bool IsActive { get; set; }
}
