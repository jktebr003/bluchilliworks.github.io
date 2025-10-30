using Api.Infrastructure.Database.MongoDb.Entities;

namespace Api.Features.Messages;

public interface IMessageRepository
{
    //public Task<List<Package>> FilterPackagesByUserIdAsync(string userId);
    Task<List<Message>> GetAllMessagesAsync();
    Task<Message?> GetMessageByIdAsync(string id);
    //Task<string> CreatePostAsync(Post post);
    //Task<bool> UpdatePostAsync(Post post);
    //Task<bool> DeletePostAsync(string id);
    public Task SaveMessageAsync(Message message);
}
