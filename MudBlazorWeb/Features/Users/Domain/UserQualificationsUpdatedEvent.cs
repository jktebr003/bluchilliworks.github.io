using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users.Domain;

public sealed record UserQualificationsUpdatedEvent(
    Guid UserId,
    IEnumerable<QualificationResponse> IncomingQualifications,
    string DeletedBy = "system") : IDomainEvent;
