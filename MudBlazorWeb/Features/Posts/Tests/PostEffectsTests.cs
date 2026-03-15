using Fluxor;
using MediatR;

using MudBlazorWeb.Features.Posts.Application;
using MudBlazorWeb.Features.Posts.UI;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Posts.Tests;

public class PostEffectsTests
{
    [Fact]
    public async Task HandleLoadPosts_ShouldDispatchSuccess_WhenMediatorReturnsSuccessfulResult()
    {
        var posts = new List<GetPostsQuery.PostDto>
        {
            CreatePostDto("One"),
            CreatePostDto("Two")
        };

        object? capturedRequest = null;
        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                var result = new PagedResult<List<GetPostsQuery.PostDto>>(posts, true, 1, 2, 1, 10);
                return Task.FromResult<object?>(result);
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        await effects.HandleLoadPosts(new LoadPostsAction(1, 10), dispatcher);

        var query = Assert.IsType<GetPostsQuery.Query>(capturedRequest);
        Assert.Equal(10, query.PageSize);
        Assert.Equal(1, query.PageNumber);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<LoadPostsSuccessAction>(action);
        Assert.Equal(2, success.Posts.Count);
        Assert.Equal(2, success.TotalItems);
    }

    [Fact]
    public async Task HandleLoadPosts_ShouldDispatchFailed_WhenMediatorReturnsFailure()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(
                new PagedResult<List<GetPostsQuery.PostDto>>(
                    Value: new List<GetPostsQuery.PostDto>(),
                    Success: false,
                    TotalPages: 0,
                    TotalItems: 0,
                    Message: "Backend failure"))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        await effects.HandleLoadPosts(new LoadPostsAction(), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadPostsFailedAction>(action);
        Assert.Equal("Backend failure", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadPostDetail_ShouldDispatchFailure_WhenPostIdIsInvalid()
    {
        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(new FakeMediator());

        await effects.HandleLoadPostDetail(new LoadPostDetailAction("invalid"), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<LoadPostDetailFailedAction>(action);
        Assert.Equal("Invalid post ID.", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleLoadPostDetail_ShouldDispatchSuccess_WhenMediatorReturnsPost()
    {
        var postId = Guid.NewGuid();
        object? capturedRequest = null;

        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                var dto = CreatePostDetailDto(postId, "Detail");
                return Task.FromResult<object?>(new Result<GetPostQuery.PostDto>(dto, true));
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        await effects.HandleLoadPostDetail(new LoadPostDetailAction(postId.ToString()), dispatcher);

        var query = Assert.IsType<GetPostQuery.Query>(capturedRequest);
        Assert.Equal(postId, query.Id);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<LoadPostDetailSuccessAction>(action);
        Assert.Equal(postId.ToString(), success.Post.ID);
    }

    [Fact]
    public async Task HandleUpdatePost_ShouldDispatchDetailReload_WhenUpdateSucceeds()
    {
        var postId = Guid.NewGuid().ToString();
        object? capturedRequest = null;

        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                return Task.FromResult<object?>(new Result<string>(postId, true));
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        await effects.HandleUpdatePost(new UpdatePostAction(new UpdatePostRequest { Id = postId, Title = "Updated" }), dispatcher);

        var command = Assert.IsType<UpdatePostCommand.Command>(capturedRequest);
        Assert.Equal(postId, command.Request.Id);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var reload = Assert.IsType<LoadPostDetailAction>(action);
        Assert.Equal(postId, reload.PostId);
    }

    [Fact]
    public async Task HandleUpdatePost_ShouldDispatchFailure_WhenMediatorThrows()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => throw new InvalidOperationException("db unavailable")
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        await effects.HandleUpdatePost(new UpdatePostAction(new UpdatePostRequest { Id = Guid.NewGuid().ToString() }), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<UpdatePostFailedAction>(action);
        Assert.Equal("Error updating post: db unavailable", failed.ErrorMessage);
    }

    [Fact]
    public async Task HandleCreatePost_ShouldDispatchSuccess_WhenMediatorReturnsId()
    {
        var postId = Guid.NewGuid().ToString();
        object? capturedRequest = null;

        var mediator = new FakeMediator
        {
            SendHandler = request =>
            {
                capturedRequest = request;
                return Task.FromResult<object?>(new Result<string>(postId, true));
            }
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        var request = new CreatePostRequest
        {
            Title = "T",
            Heading = "H",
            Content = "Body",
            Author = "Author",
            Category = "General"
        };

        await effects.HandleCreatePost(new CreatePostAction(request), dispatcher);

        Assert.IsType<CreatePostCommand.Command>(capturedRequest);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var success = Assert.IsType<CreatePostSuccessAction>(action);
        Assert.Equal(postId, success.PostId);
    }

    [Fact]
    public async Task HandleCreatePost_ShouldDispatchFailure_WhenMediatorReturnsNoId()
    {
        var mediator = new FakeMediator
        {
            SendHandler = _ => Task.FromResult<object?>(new Result<string>(string.Empty, false, Message: "create failed"))
        };

        var dispatcher = new FakeDispatcher();
        var effects = new PostsEffects(mediator);

        await effects.HandleCreatePost(new CreatePostAction(new CreatePostRequest()), dispatcher);

        var action = Assert.Single(dispatcher.DispatchedActions);
        var failed = Assert.IsType<CreatePostFailedAction>(action);
        Assert.Equal("create failed", failed.ErrorMessage);
    }

    private static GetPostsQuery.PostDto CreatePostDto(string title)
    {
        return new GetPostsQuery.PostDto(
            Title: title,
            Heading: "Heading",
            Description: "Description",
            Content: "Content",
            Author: "Author",
            UserId: "user-1",
            Category: "General",
            TotalViews: 5,
            TotalLikes: 2,
            PostedOn: DateTime.UtcNow)
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private static GetPostQuery.PostDto CreatePostDetailDto(Guid id, string title)
    {
        return new GetPostQuery.PostDto(
            Title: title,
            Heading: "Heading",
            Description: "Description",
            Content: "Content",
            Author: "Author",
            UserId: "user-1",
            Category: "General",
            TotalViews: 7,
            TotalLikes: 3,
            PostedOn: DateTime.UtcNow)
        {
            Id = id,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private sealed class FakeDispatcher : IDispatcher
    {
        public List<object> DispatchedActions { get; } = new();
        public event EventHandler<ActionDispatchedEventArgs>? ActionDispatched;

        public void Dispatch(object action)
        {
            DispatchedActions.Add(action);
            ActionDispatched?.Invoke(this, new ActionDispatchedEventArgs(action));
        }
    }

    private sealed class FakeMediator : IMediator
    {
        public Func<object, Task<object?>>? SendHandler { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (SendHandler == null)
            {
                throw new InvalidOperationException("No send handler configured.");
            }

            return SendHandler(request).ContinueWith(t => (TResponse)t.Result!, cancellationToken);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            if (SendHandler == null)
            {
                throw new InvalidOperationException("No send handler configured.");
            }

            return SendHandler(request!);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (SendHandler == null)
            {
                throw new InvalidOperationException("No send handler configured.");
            }

            return SendHandler(request);
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Streaming is not used in these tests.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Streaming is not used in these tests.");
    }
}
