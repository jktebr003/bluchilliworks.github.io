using MudBlazorWeb.Features.Posts.Application;
using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Posts.Tests;

public class PostHandlerTests
{
    [Fact]
    public async Task GetPostHandler_ShouldReturnSuccess_WhenPostExists()
    {
        var postId = Guid.NewGuid();
        var createdOn = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var post = CreatePost(postId, "First", "Heading", likes: ["u1", "u2"]);
        post.CreatedOn = createdOn;
        post.CreatedBy = "system";

        var repository = new FakePostRepository
        {
            GetPostByIdHandler = (_, _) => Task.FromResult<Post?>(post)
        };

        var handler = new GetPostQuery.Handler(repository);

        var result = await handler.Handle(new GetPostQuery.Query(postId), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Null(result.Code);
        Assert.NotNull(result.Value);
        Assert.Equal(postId, result.Value.Id);
        Assert.Equal("First", result.Value.Title);
        Assert.Equal("Heading", result.Value.Heading);
        Assert.Equal(2, result.Value.TotalLikes);
        Assert.Equal(createdOn, result.Value.CreatedOn);
        Assert.Equal("system", result.Value.CreatedBy);
    }

    [Fact]
    public async Task GetPostHandler_ShouldReturnFailure_WhenPostDoesNotExist()
    {
        var repository = new FakePostRepository
        {
            GetPostByIdHandler = (_, _) => Task.FromResult<Post?>(null)
        };

        var handler = new GetPostQuery.Handler(repository);

        var result = await handler.Handle(new GetPostQuery.Query(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("GetPost.Null", result.Code);
        Assert.Equal("The post with the specified ID was not found", result.Message);
        Assert.NotNull(result.Value);
        Assert.Equal(Guid.Empty, result.Value.Id);
        Assert.Equal(string.Empty, result.Value.Title);
    }

    [Fact]
    public async Task GetPostsHandler_ShouldReturnPagedData_WhenPagingParametersAreValid()
    {
        var posts = new List<Post>
        {
            CreatePost(Guid.NewGuid(), "Post 1", "H1"),
            CreatePost(Guid.NewGuid(), "Post 2", "H2"),
            CreatePost(Guid.NewGuid(), "Post 3", "H3"),
            CreatePost(Guid.NewGuid(), "Post 4", "H4")
        };

        var repository = new FakePostRepository
        {
            GetAllPostsHandler = _ => Task.FromResult(posts)
        };

        var handler = new GetPostsQuery.Handler(repository);

        var result = await handler.Handle(new GetPostsQuery.Query(2, 2), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(4, result.TotalItems);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(posts[2].Id, result.Value[0].Id);
        Assert.Equal(posts[3].Id, result.Value[1].Id);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-10, -3)]
    public async Task GetPostsHandler_ShouldClampInvalidPagingValues(int pageSize, int pageNumber)
    {
        var posts = new List<Post>
        {
            CreatePost(Guid.NewGuid(), "Post 1", "H1"),
            CreatePost(Guid.NewGuid(), "Post 2", "H2"),
            CreatePost(Guid.NewGuid(), "Post 3", "H3")
        };

        var repository = new FakePostRepository
        {
            GetAllPostsHandler = _ => Task.FromResult(posts)
        };

        var handler = new GetPostsQuery.Handler(repository);

        var result = await handler.Handle(new GetPostsQuery.Query(pageSize, pageNumber), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(3, result.Value.Count);
    }

    [Fact]
    public async Task FilterPostsHandler_ShouldReturnMappedPosts_AndForwardCancellationToken()
    {
        var userId = "user-1";
        var post = CreatePost(Guid.NewGuid(), "Filtered", "H", userId: userId, likes: ["a"]);
        CancellationToken observedToken = default;
        using var cts = new CancellationTokenSource();

        var repository = new FakePostRepository
        {
            FilterPostsByUserIdHandler = (id, token) =>
            {
                observedToken = token;
                Assert.Equal(userId, id);
                return Task.FromResult(new List<Post> { post });
            }
        };

        var handler = new FilterPostsQuery.Handler(repository);

        var result = await handler.Handle(new FilterPostsQuery.Query(userId), cts.Token);

        Assert.True(result.Success);
        Assert.Single(result.Value);
        Assert.Equal(post.Id, result.Value[0].Id);
        Assert.Equal(cts.Token, observedToken);
    }

    [Fact]
    public async Task CreatePostHandler_ShouldReturnValidationFailure_WhenRequiredFieldsAreMissing()
    {
        var repository = new FakePostRepository();
        var handler = new CreatePostCommand.Handler(repository);

        var request = new CreatePostRequest
        {
            Title = "",
            Heading = "H",
            Content = "Body",
            Author = "Author",
            Category = "Category"
        };

        var result = await handler.Handle(new CreatePostCommand.Command(request), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("CreatePost.Validation", result.Code);
        Assert.Equal(string.Empty, result.Value);
        Assert.Equal("Please provide title, heading, content, author, and category.", result.Message);
        Assert.Null(repository.SavedPost);
    }

    [Fact]
    public async Task CreatePostHandler_ShouldPersistPost_WhenRequestIsValid()
    {
        var repository = new FakePostRepository();
        var handler = new CreatePostCommand.Handler(repository);

        var request = new CreatePostRequest
        {
            Title = "T",
            Heading = "H",
            Description = "D",
            Content = "Body",
            Author = "Author",
            UserId = "user-1",
            Category = "General",
            CreatedBy = "tester"
        };

        var result = await handler.Handle(new CreatePostCommand.Command(request), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEmpty(result.Value);
        Assert.NotNull(repository.SavedPost);
        Assert.Equal("T", repository.SavedPost!.Title);
        Assert.Equal("tester", repository.SavedPost.CreatedBy);
        Assert.Equal("user-1", repository.SavedPost.UserId);
        Assert.Empty(repository.SavedPost.Likes);
    }

    [Fact]
    public async Task UpdatePostHandler_ShouldReturnValidationFailure_WhenIdIsInvalid()
    {
        var repository = new FakePostRepository();
        var handler = new UpdatePostCommand.Handler(repository);

        var request = new UpdatePostRequest
        {
            Id = "not-a-guid",
            Title = "Updated"
        };

        var result = await handler.Handle(new UpdatePostCommand.Command(request), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("UpdatePost.Validation", result.Code);
        Assert.Equal("Invalid post ID.", result.Message);
    }

    [Fact]
    public async Task UpdatePostHandler_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        var repository = new FakePostRepository
        {
            GetPostByIdHandler = (_, _) => Task.FromResult<Post?>(null)
        };

        var handler = new UpdatePostCommand.Handler(repository);

        var request = new UpdatePostRequest
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Updated"
        };

        var result = await handler.Handle(new UpdatePostCommand.Command(request), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("UpdatePost.NotFound", result.Code);
        Assert.Equal("The post with the specified ID was not found.", result.Message);
    }

    [Fact]
    public async Task UpdatePostHandler_ShouldUpdateExistingPost_WhenRequestIsValid()
    {
        var postId = Guid.NewGuid();
        var existing = CreatePost(postId, "Old", "Old Heading", likes: ["u1"]);

        var repository = new FakePostRepository
        {
            GetPostByIdHandler = (_, _) => Task.FromResult<Post?>(existing)
        };

        var handler = new UpdatePostCommand.Handler(repository);

        var request = new UpdatePostRequest
        {
            Id = postId.ToString(),
            Title = "New",
            Heading = "New Heading",
            Description = "New Description",
            Content = "New Content",
            Author = "New Author",
            Category = "Updated",
            TotalViews = 50,
            Likes = ["u2", "u3"],
            ModifiedBy = "editor",
            ModifiedOn = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = await handler.Handle(new UpdatePostCommand.Command(request), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(postId.ToString(), result.Value);
        Assert.NotNull(repository.UpdatedPost);
        Assert.Equal("New", repository.UpdatedPost!.Title);
        Assert.Equal("New Heading", repository.UpdatedPost.Heading);
        Assert.Equal(50, repository.UpdatedPost.TotalViews);
        Assert.Equal(2, repository.UpdatedPost.Likes.Count);
        Assert.Equal("editor", repository.UpdatedPost.ModifiedBy);
    }

    private static Post CreatePost(Guid id, string title, string heading, string? userId = "user", List<string>? likes = null)
    {
        return new Post
        {
            Id = id,
            Title = title,
            Heading = heading,
            Description = "description",
            Content = "content",
            Author = "author",
            UserId = userId ?? string.Empty,
            Category = "category",
            TotalViews = 10,
            Likes = likes ?? [],
            PostedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "test"
        };
    }

    private sealed class FakePostRepository : IPostRepository
    {
        public Func<string, CancellationToken, Task<List<Post>>>? FilterPostsByUserIdHandler { get; init; }
        public Func<CancellationToken, Task<List<Post>>>? GetAllPostsHandler { get; init; }
        public Func<Guid, CancellationToken, Task<Post?>>? GetPostByIdHandler { get; init; }

        public Post? SavedPost { get; private set; }
        public Post? UpdatedPost { get; private set; }

        public Task<List<Post>> FilterPostsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
            => FilterPostsByUserIdHandler?.Invoke(userId, cancellationToken)
               ?? Task.FromResult(new List<Post>());

        public Task<List<Post>> GetAllPostsAsync(CancellationToken cancellationToken = default)
            => GetAllPostsHandler?.Invoke(cancellationToken)
               ?? Task.FromResult(new List<Post>());

        public Task<Post?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => GetPostByIdHandler?.Invoke(id, cancellationToken)
               ?? Task.FromResult<Post?>(null);

        public Task SavePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            SavedPost = post;
            return Task.CompletedTask;
        }

        public Task UpdatePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            UpdatedPost = post;
            return Task.CompletedTask;
        }
    }
}
