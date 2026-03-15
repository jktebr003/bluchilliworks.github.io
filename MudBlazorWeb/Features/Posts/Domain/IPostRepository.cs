using System;

namespace MudBlazorWeb.Features.Posts.Domain;

public interface IPostRepository
{
    Task<List<Post>> FilterPostsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Post>> GetAllPostsAsync(CancellationToken cancellationToken = default);
    Task<Post?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SavePostAsync(Post post, CancellationToken cancellationToken = default);
    Task UpdatePostAsync(Post post, CancellationToken cancellationToken = default);
}
