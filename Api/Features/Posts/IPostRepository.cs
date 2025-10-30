using Api.Infrastructure.Database.MongoDb.Entities;

namespace Api.Features.Posts;

public interface IPostRepository
{
    public Task<List<Post>> FilterPostsByUserIdAsync(string userId);
    Task<List<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(string id);
    //Task<string> CreatePostAsync(Post post);
    //Task<bool> UpdatePostAsync(Post post);
    //Task<bool> DeletePostAsync(string id);
    public Task SavePostAsync(Post post);
}
