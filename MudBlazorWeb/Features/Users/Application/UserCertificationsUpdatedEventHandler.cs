using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Users.Application;

internal sealed class UserCertificationsUpdatedEventHandler(ICertificationRepository certificationRepository)
    : IDomainEventHandler<UserCertificationsUpdatedEvent>
{
    public async Task Handle(UserCertificationsUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var existingCertifications = await certificationRepository.GetCertificationsByUserIdAsync(domainEvent.UserId, cancellationToken);
        var existingCertificationsById = existingCertifications.ToDictionary(c => c.Id);

        // Track which existing certification IDs are referenced by the incoming list
        var incomingCertificationIds = domainEvent.IncomingCertifications
            .Where(c => Guid.TryParse(c.ID, out _))
            .Select(c => Guid.Parse(c.ID!))
            .ToHashSet();

        foreach (var incoming in domainEvent.IncomingCertifications)
        {
            var isExisting = Guid.TryParse(incoming.ID, out var certificationId) && existingCertificationsById.ContainsKey(certificationId);

            if (isExisting)
            {
                var existing = existingCertificationsById[certificationId];
                existing.Title = incoming.Title;
                existing.Institution = incoming.Institution;
                existing.Year = incoming.Year;

                await certificationRepository.UpdateCertificationAsync(existing, cancellationToken);
            }
            else
            {
                var certification = new Certification
                {
                    Id = Guid.NewGuid(),
                    UserId = domainEvent.UserId,
                    Title = incoming.Title,
                    Institution = incoming.Institution,
                    Year = incoming.Year
                };

                await certificationRepository.CreateCertificationAsync(certification, cancellationToken);
            }
        }

        // Soft-delete any existing certifications that were not present in the incoming list
        var removedCertifications = existingCertifications.Where(c => !incomingCertificationIds.Contains(c.Id));
        foreach (var removed in removedCertifications)
        {
            removed.IsDeleted = true;
            removed.DeletedOn = DateTime.UtcNow;
            removed.DeletedBy = domainEvent.DeletedBy;

            await certificationRepository.UpdateCertificationAsync(removed, cancellationToken);
        }
    }
}

