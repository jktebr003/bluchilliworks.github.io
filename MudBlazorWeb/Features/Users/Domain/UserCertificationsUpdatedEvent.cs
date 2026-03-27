using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Users.Domain;

public sealed record UserCertificationsUpdatedEvent(
    Guid UserId,
    IEnumerable<CertificationResponse> IncomingCertifications,
    string DeletedBy = "system") : IDomainEvent;
