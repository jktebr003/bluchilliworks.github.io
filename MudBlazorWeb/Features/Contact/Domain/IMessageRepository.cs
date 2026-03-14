namespace MudBlazorWeb.Features.Contact.Domain;

public interface IMessageRepository
{
    Task<List<Message>> GetAllMessagesAsync(CancellationToken cancellationToken = default);
    Task<Message?> GetMessageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task SaveMessageAsync(Message message, CancellationToken cancellationToken = default);
}
