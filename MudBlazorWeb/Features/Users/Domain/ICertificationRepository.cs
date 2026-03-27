namespace MudBlazorWeb.Features.Users.Domain;

public interface ICertificationRepository
{
    Task<List<Certification>> GetCertificationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Certification?> GetCertificationAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateCertificationAsync(Certification certification, CancellationToken cancellationToken = default);
    Task UpdateCertificationAsync(Certification certification, CancellationToken cancellationToken = default);
}
