using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;

namespace Api.Features.Messages;

public class MessageRepository : IMessageRepository
{
    private readonly IMongoDbRepository _mongoDbRepository;

    public MessageRepository(IMongoDbRepository mongoDbRepository)
        => _mongoDbRepository = mongoDbRepository;

    public Task<List<Message>> GetAllMessagesAsync()
        => _mongoDbRepository.GetAll<Message>();

    public Task<Message?> GetMessageByIdAsync(string id)
        => _mongoDbRepository.Get<Message>(id);

    public Task SaveMessageAsync(Message message)
        => _mongoDbRepository.Save(message);
}
