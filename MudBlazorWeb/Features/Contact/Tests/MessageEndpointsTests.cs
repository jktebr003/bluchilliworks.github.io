using System.Net;
using System.Net.Http.Json;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MudBlazorWeb.Features.Contact.Application;
using MudBlazorWeb.Shared;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

using Xunit;

namespace MudBlazorWeb.Features.Contact.Tests;

public class MessageEndpointsTests
{
    private const string ApiKey = "primary-test-key";

    // ======================================================================
    // POST /api/messages – CreateMessageCommandEndpoint
    // ======================================================================

    [Fact]
    public async Task CreateMessage_ShouldReturnOk_WhenRequestIsValid()
    {
        var messageId = Guid.NewGuid().ToString();
        var expected = new Result<string>(messageId, true);

        var sender = new FakeSender(request =>
        {
            var command = Assert.IsType<CreateMessageCommand.Command>(request);
            Assert.Equal("test@example.com", command.Request.EmailAddress);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var request = new CreateMessageRequest
        {
            From = "Test User",
            EmailAddress = "test@example.com",
            Subject = "Hello",
            Body = "Test message",
            SentOn = "2026-01-01T00:00:00Z",
            CreatedBy = "test"
        };

        var response = await host.Client.PostAsync("/api/messages", JsonContent.Create(request));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(messageId, payload.Value);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnOkWithFailurePayload_WhenValidationFails()
    {
        var expected = new Result<string>(
            string.Empty, false, "CreateMessage.Validation", "Please provide email, subject, and message body.");

        var sender = new FakeSender(_ => expected);

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var request = new CreateMessageRequest
        {
            EmailAddress = string.Empty,
            Subject = string.Empty,
            Body = string.Empty
        };

        var response = await host.Client.PostAsync("/api/messages", JsonContent.Create(request));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<string>>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
        Assert.Equal("CreateMessage.Validation", payload.Code);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        var sender = new FakeSender(
            _ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

        await using var host = await CreateHostAsync(sender);

        var request = new CreateMessageRequest
        {
            From = "User",
            EmailAddress = "a@b.com",
            Subject = "S",
            Body = "B"
        };

        var response = await host.Client.PostAsync("/api/messages", JsonContent.Create(request));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnUnauthorized_WhenWrongApiKeyIsProvided()
    {
        var sender = new FakeSender(
            _ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", "wrong-key");

        var request = new CreateMessageRequest
        {
            From = "User",
            EmailAddress = "a@b.com",
            Subject = "S",
            Body = "B"
        };

        var response = await host.Client.PostAsync("/api/messages", JsonContent.Create(request));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ======================================================================
    // GET /api/messages/{id} – GetMessageQueryEndpoint
    // ======================================================================

    [Fact]
    public async Task GetMessage_ShouldReturnOk_WhenMessageExists()
    {
        var messageId = Guid.NewGuid();
        var dto = new GetMessageQuery.MessageDto(
            "Test User", "test@example.com", "Hello", "Body",
            null, MessageStatus.Sent, 0, 3, null, null)
        {
            Id = messageId,
            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = "system"
        };

        var expected = new Result<GetMessageQuery.MessageDto>(dto, true);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetMessageQuery.Query>(request);
            Assert.Equal(messageId, query.Id);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync($"/api/messages/{messageId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<GetMessageQuery.MessageDto>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(messageId, payload.Value.Id);
        Assert.Equal("test@example.com", payload.Value.EmailAddress);
    }

    [Fact]
    public async Task GetMessage_ShouldReturnNotFoundPayload_WhenMessageDoesNotExist()
    {
        var emptyDto = new GetMessageQuery.MessageDto(
            string.Empty, string.Empty, string.Empty, null, null, default, 0, 0, null, null);
        var expected = new Result<GetMessageQuery.MessageDto>(
            emptyDto, false, "GetMessage.Null", "The message with the specified ID was not found");

        var sender = new FakeSender(request =>
        {
            Assert.IsType<GetMessageQuery.Query>(request);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Result<GetMessageQuery.MessageDto>>();
        Assert.NotNull(payload);
        Assert.False(payload!.Success);
        Assert.Equal("GetMessage.Null", payload.Code);
    }

    [Fact]
    public async Task GetMessage_ShouldReturnBadRequest_WhenGuidIsInvalid()
    {
        var sender = new FakeSender(
            _ => throw new InvalidOperationException("Sender should not be called for invalid route values."));

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/messages/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMessage_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        var sender = new FakeSender(
            _ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

        await using var host = await CreateHostAsync(sender);

        var response = await host.Client.GetAsync($"/api/messages/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ======================================================================
    // GET /api/messages – GetMessagesQueryEndpoint
    // ======================================================================

    [Fact]
    public async Task GetMessages_ShouldReturnOkWithPagedResult_WhenParametersAreValid()
    {
        var items = new List<GetMessagesQuery.MessageDto>
        {
            new("Alice", "a@a.com", "Subject A", "Body A", null, MessageStatus.Pending, 0, 3, null, null),
            new("Bob", "b@b.com", "Subject B", "Body B", null, MessageStatus.Sent, 1, 3, null, null)
        };

        var expected = new PagedResult<List<GetMessagesQuery.MessageDto>>(items, true, 1, 2, 1, 10);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetMessagesQuery.Query>(request);
            Assert.Equal(10, query.PageSize);
            Assert.Equal(1, query.PageNumber);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/messages?pageSize=10&pageNumber=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content
            .ReadFromJsonAsync<PagedResult<List<GetMessagesQuery.MessageDto>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(2, payload.TotalItems);
        Assert.Equal(2, payload.Value.Count);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        var sender = new FakeSender(
            _ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

        await using var host = await CreateHostAsync(sender);

        var response = await host.Client.GetAsync("/api/messages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_ShouldForwardNullPaging_WhenParametersAreMissing()
    {
        var expected = new PagedResult<List<GetMessagesQuery.MessageDto>>(
            new List<GetMessagesQuery.MessageDto>(), true, 0, 0, null, null);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetMessagesQuery.Query>(request);
            Assert.Null(query.PageSize);
            Assert.Null(query.PageNumber);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/messages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnBadRequest_WhenPagingParametersAreInvalid()
    {
        var sender = new FakeSender(
            _ => throw new InvalidOperationException("Sender should not be called for invalid query values."));

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/messages?pageSize=abc&pageNumber=1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ======================================================================
    // Helpers
    // ======================================================================

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

        new CreateMessageCommandEndpoint().AddRoutes(app);
        new GetMessageQueryEndpoint().AddRoutes(app);
        new GetMessagesQueryEndpoint().AddRoutes(app);

        await app.StartAsync();

        return new TestHostContext
        {
            App = app,
            Client = app.GetTestClient()
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
