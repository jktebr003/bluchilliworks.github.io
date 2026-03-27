namespace MudBlazorWeb.Features.Users.Domain;

public interface IQualificationRepository
{
    Task<List<Qualification>> GetQualificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Qualification?> GetQualificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateQualificationAsync(Qualification qualification, CancellationToken cancellationToken = default);
    Task UpdateQualificationAsync(Qualification qualification, CancellationToken cancellationToken = default);
}