using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users.Domain;

public sealed record UserJobsUpdatedEvent(
    Guid UserId,
    IEnumerable<JobResponse> IncomingJobs,
    string DeletedBy = "system") : IDomainEvent;
