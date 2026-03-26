using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Users.Application;

internal sealed class UserJobsUpdatedEventHandler(IJobRepository jobRepository)
    : IDomainEventHandler<UserJobsUpdatedEvent>
{
    public async Task Handle(UserJobsUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var existingJobs = await jobRepository.GetJobsByUserIdAsync(domainEvent.UserId, cancellationToken);
        var existingJobsById = existingJobs.ToDictionary(j => j.Id);

        // Track which existing job IDs are referenced by the incoming list
        var incomingJobIds = domainEvent.IncomingJobs
            .Where(j => Guid.TryParse(j.ID, out _))
            .Select(j => Guid.Parse(j.ID!))
            .ToHashSet();

        foreach (var incoming in domainEvent.IncomingJobs)
        {
            var isExisting = Guid.TryParse(incoming.ID, out var jobId) && existingJobsById.ContainsKey(jobId);

            if (isExisting)
            {
                var existing = existingJobsById[jobId];
                existing.Company = incoming.Company;
                existing.Position = incoming.Position;
                existing.StartDate = incoming.StartDate;
                existing.EndDate = incoming.EndDate;
                existing.Responsibilities = incoming.Responsibilities;

                await jobRepository.UpdateJobAsync(existing, cancellationToken);
            }
            else
            {
                var job = new Job
                {
                    Id = Guid.NewGuid(),
                    UserId = domainEvent.UserId,
                    Company = incoming.Company,
                    Position = incoming.Position,
                    StartDate = incoming.StartDate,
                    EndDate = incoming.EndDate,
                    Responsibilities = incoming.Responsibilities
                };

                await jobRepository.CreateJobAsync(job, cancellationToken);
            }
        }

        // Soft-delete any existing jobs that were not present in the incoming list
        var removedJobs = existingJobs.Where(j => !incomingJobIds.Contains(j.Id));
        foreach (var removed in removedJobs)
        {
            removed.IsDeleted = true;
            removed.DeletedOn = DateTime.UtcNow;
            removed.DeletedBy = domainEvent.DeletedBy;

            await jobRepository.UpdateJobAsync(removed, cancellationToken);
        }
    }
}
