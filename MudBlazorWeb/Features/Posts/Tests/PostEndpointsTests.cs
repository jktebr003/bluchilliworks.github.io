using System.Net;
using System.Net.Http.Json;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MudBlazorWeb.Features.Posts.Application;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Posts.Tests;

public class PostEndpointsTests
{
    private const string ApiKey = "primary-test-key";

    [Fact]
    public async Task GetPost_ShouldReturnOk_WhenPostExists()
    {
        var postId = Guid.NewGuid();
        var dto = CreatePostDetailDto(postId, "Post A");
        var expected = new Result<GetPostQuery.PostDto>(dto, true);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetPostQuery.Query>(request);
            Assert.Equal(postId, query.Id);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync($"/api/posts/{postId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<GetPostQuery.PostDto>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(postId, payload.Value.Id);
    }

    [Fact]
    public async Task GetPost_ShouldReturnBadRequest_WhenGuidIsInvalid()
    {
        var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called."));

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/posts/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPosts_ShouldReturnOkWithPagedResult_WhenParametersAreValid()
    {
        var expectedPosts = new List<GetPostsQuery.PostDto>
        {
            CreatePostListDto("One"),
            CreatePostListDto("Two")
        };
        var expected = new PagedResult<List<GetPostsQuery.PostDto>>(expectedPosts, true, 1, 2, 1, 10);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetPostsQuery.Query>(request);
            Assert.Equal(10, query.PageSize);
            Assert.Equal(1, query.PageNumber);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/posts?pageSize=10&pageNumber=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PagedResult<List<GetPostsQuery.PostDto>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(2, payload.TotalItems);
        Assert.Equal(2, payload.Value.Count);
    }

    [Fact]
    public async Task FilterPosts_ShouldReturnOk_WhenUserIdIsProvided()
    {
        var expectedPosts = new List<FilterPostsQuery.PostDto>
        {
            CreateFilterPostDto("Filtered")
        };
        var expected = new Result<List<FilterPostsQuery.PostDto>>(expectedPosts, true);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<FilterPostsQuery.Query>(request);
            Assert.Equal("user-1", query.UserId);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/posts/filter/user-1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<List<FilterPostsQuery.PostDto>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Single(payload.Value);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnOk_WhenRequestIsValid()
    {
        var newId = Guid.NewGuid().ToString();
        var sender = new FakeSender(request =>
        {
            var command = Assert.IsType<CreatePostCommand.Command>(request);
            Assert.Equal("New Post", command.Request.Title);
            return new Result<string>(newId, true);
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var request = new CreatePostRequest
        {
            Title = "New Post",
            Heading = "Heading",
            Content = "Body",
            Author = "Author",
            Category = "General"
        };

        var response = await host.Client.PostAsJsonAsync("/api/posts", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(newId, payload.Value);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnOk_WhenRequestIsValid()
    {
        var updatedId = Guid.NewGuid().ToString();
        var sender = new FakeSender(request =>
        {
            var command = Assert.IsType<UpdatePostCommand.Command>(request);
            Assert.Equal(updatedId, command.Request.Id);
            return new Result<string>(updatedId, true);
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var request = new UpdatePostRequest
        {
            Id = updatedId,
            Title = "Updated Title"
        };

        var response = await host.Client.PutAsJsonAsync("/api/posts", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(updatedId, payload.Value);
    }

    [Fact]
    public async Task PostsEndpoints_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

        await using var host = await CreateHostAsync(sender);

        var response = await host.Client.GetAsync($"/api/posts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<TestHostContext> CreateHostAsync(ISender sender)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });

        builder.WebHost.UseTestServer();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SecretKeys:ApiKey"] = ApiKey,
            ["SecretKeys:ApiKeySecondary"] = string.Empty,
            ["SecretKeys:UseSecondaryKey"] = "false"
        });

        builder.Services.AddSingleton(sender);

        var app = builder.Build();

        new GetPostQueryEndpoint().AddRoutes(app);
        new GetPostsQueryEndpoint().AddRoutes(app);
        new FilterPostsQueryEndpoint().AddRoutes(app);
        new PostCommandsEndpoint().AddRoutes(app);

        await app.StartAsync();

        return new TestHostContext
        {
            App = app,
            Client = app.GetTestClient()
        };
    }

    private static GetPostsQuery.PostDto CreatePostListDto(string title)
    {
        return new GetPostsQuery.PostDto(
            title,
            "Heading",
            "Description",
            "Content",
            "Author",
            "user-1",
            "General",
            10,
            2,
            DateTime.UtcNow)
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private static GetPostQuery.PostDto CreatePostDetailDto(Guid id, string title)
    {
        return new GetPostQuery.PostDto(
            title,
            "Heading",
            "Description",
            "Content",
            "Author",
            "user-1",
            "General",
            10,
            2,
            DateTime.UtcNow)
        {
            Id = id,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private static FilterPostsQuery.PostDto CreateFilterPostDto(string title)
    {
        return new FilterPostsQuery.PostDto(
            title,
            "Heading",
            "Description",
            "Content",
            "Author",
            "user-1",
            "General",
            10,
            2,
            DateTime.UtcNow)
        {
            Id = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "test"
        };
    }

    private sealed class TestHostContext : IAsyncDisposable
    {
        public required WebApplication App { get; init; }
        public required HttpClient Client { get; init; }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.StopAsync();
            await App.DisposeAsync();
        }
    }

    private sealed class FakeSender(Func<object, object?> sendHandler) : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var response = sendHandler(request);
            return Task.FromResult((TResponse)response!);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            sendHandler(request!);
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => Task.FromResult(sendHandler(request));

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Streaming requests are not used in these tests.");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Streaming requests are not used in these tests.");

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
