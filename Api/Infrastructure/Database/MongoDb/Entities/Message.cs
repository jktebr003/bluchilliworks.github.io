using MongoDB.Entities;

namespace Api.Infrastructure.Database.MongoDb.Entities;

[Collection("messages")]
public class Message : BaseAuditableEntity
{
    [Field("name")]
    public string? Name { get; set; }

    [Field("email")]
    public string? EmailAddress { get; set; }

    [Field("subject")]
    public string? Subject { get; set; }

    [Field("message")]
    public string? Body { get; set; }

    [Field("sentdate")]
    public string? SentOn { get; set; }
}
