using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Users.Application;

internal sealed class UserQualificationsUpdatedEventHandler(IQualificationRepository qualificationRepository)
    : IDomainEventHandler<UserQualificationsUpdatedEvent>
{
    public async Task Handle(UserQualificationsUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var existingQualifications = await qualificationRepository.GetQualificationsByUserIdAsync(domainEvent.UserId, cancellationToken);
        var existingQualificationsById = existingQualifications.ToDictionary(q => q.Id);

        // Track which existing qualification IDs are referenced by the incoming list
        var incomingQualificationIds = domainEvent.IncomingQualifications
            .Where(q => Guid.TryParse(q.ID, out _))
            .Select(q => Guid.Parse(q.ID!))
            .ToHashSet();

        foreach (var incoming in domainEvent.IncomingQualifications)
        {
            var isExisting = Guid.TryParse(incoming.ID, out var qualificationId) && existingQualificationsById.ContainsKey(qualificationId);

            if (isExisting)
            {
                var existing = existingQualificationsById[qualificationId];
                existing.Title = incoming.Title;
                existing.Institution = incoming.Institution;
                existing.Year = incoming.Year;

                await qualificationRepository.UpdateQualificationAsync(existing, cancellationToken);
            }
            else
            {
                var qualification = new Qualification
                {
                    Id = Guid.NewGuid(),
                    UserId = domainEvent.UserId,
                    Title = incoming.Title,
                    Institution = incoming.Institution,
                    Year = incoming.Year
                };

                await qualificationRepository.CreateQualificationAsync(qualification, cancellationToken);
            }
        }

        // Soft-delete any existing qualifications that were not present in the incoming list
        var removedQualifications = existingQualifications.Where(q => !incomingQualificationIds.Contains(q.Id));
        foreach (var removed in removedQualifications)
        {
            removed.IsDeleted = true;
            removed.DeletedOn = DateTime.UtcNow;
            removed.DeletedBy = domainEvent.DeletedBy;

            await qualificationRepository.UpdateQualificationAsync(removed, cancellationToken);
        }
    }
}
