namespace MudBlazorWeb.Features.Users.Domain;

public interface IJobRepository
{
    Task<List<Job>> GetJobsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Job?> GetJobAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateJobAsync(Job job, CancellationToken cancellationToken = default);
    Task UpdateJobAsync(Job job, CancellationToken cancellationToken = default);
}
