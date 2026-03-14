using System;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Contact.Domain;

public class Message : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string EmailAddress { get; set; }

    public string Subject { get; set; }

    public string? Body { get; set; }

    public DateTime? SentOn { get; set; }

    public int Status { get; set; }

    public int AttemptCount { get; set; } = 0;

    public int MaxRetries { get; set; } = 3;

    public DateTime? LastAttemptedOn { get; set; }

    public string? LastErrorMessage { get; set; }

    // public Guid HangfireJobId { get; set; }
}
