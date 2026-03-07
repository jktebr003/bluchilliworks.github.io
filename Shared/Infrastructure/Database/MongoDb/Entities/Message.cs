using MongoDB.Entities;
using Shared.Enums;

namespace Shared.Infrastructure.Database.MongoDb.Entities;

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

    [Field("status")]
    public MessageStatus Status { get; set; } = MessageStatus.Pending;

    [Field("attemptcount")]
    public int AttemptCount { get; set; } = 0;

    [Field("maxretries")]
    public int MaxRetries { get; set; } = 3;

    [Field("lastattempton")]
    public string? LastAttemptedOn { get; set; }

    [Field("lasterror")]
    public string? LastErrorMessage { get; set; }

    [Field("hangfirejobid")]
    public string? HangfireJobId { get; set; }
}
