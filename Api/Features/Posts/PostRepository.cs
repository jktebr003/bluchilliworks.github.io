using Api.Infrastructure.Database.MongoDb.Entities;
using Api.Infrastructure.Database.MongoDb.Repositories;
using MongoDB.Entities;

namespace Api.Features.Posts;

public class PostRepository : IPostRepository
{
    private readonly IMongoDbRepository _mongoDbRepository;

    public PostRepository(IMongoDbRepository mongoDbRepository)
        => _mongoDbRepository = mongoDbRepository;

    public Task<List<Post>> FilterPostsByUserIdAsync(string userId)
        => DB.Find<Post>()
             .Match(b => b.UserId == userId)
             .ExecuteAsync();

    public Task<List<Post>> GetAllPostsAsync()
        => _mongoDbRepository.GetAll<Post>();

    public Task<Post?> GetPostByIdAsync(string id)
        => _mongoDbRepository.Get<Post>(id);

    public Task SavePostAsync(Post post)
        => _mongoDbRepository.Save(post);
}
