using System.Net;
using System.Net.Http.Json;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MudBlazorWeb.Features.Audits.Application;
using MudBlazorWeb.Shared;

using Xunit;

namespace MudBlazorWeb.Features.Audits.Tests;

public class AuditEndpointsTests
{
    private const string ApiKey = "primary-test-key";

    [Fact]
    public async Task GetAudits_ShouldReturnOkWithPagedResult_WhenParametersAreValid()
    {
        var items = new List<GetAuditsQuery.AuditDto>
        {
            new(Guid.NewGuid(), "Create", "alice@example.com", "Packages", "{\"Id\":\"1\"}", null, null, "[\"Name\"]", "system", new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)),
            new(Guid.NewGuid(), "Update", "bob@example.com", "Messages", "{\"Id\":\"2\"}", null, null, "[\"Status\"]", "system", new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc))
        };

        var expected = new PagedResult<List<GetAuditsQuery.AuditDto>>(items, true, 1, 2, 1, 10);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetAuditsQuery.Query>(request);
            Assert.Equal(10, query.PageSize);
            Assert.Equal(1, query.PageNumber);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/audits?pageSize=10&pageNumber=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PagedResult<List<GetAuditsQuery.AuditDto>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(2, payload.TotalItems);
        Assert.Equal(2, payload.Value.Count);
    }

    [Fact]
    public async Task GetAudits_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

        await using var host = await CreateHostAsync(sender);

        var response = await host.Client.GetAsync("/api/audits");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAudits_ShouldForwardNullPaging_WhenParametersAreMissing()
    {
        var expected = new PagedResult<List<GetAuditsQuery.AuditDto>>(new List<GetAuditsQuery.AuditDto>(), true, 0, 0, null, null);

        var sender = new FakeSender(request =>
        {
            var query = Assert.IsType<GetAuditsQuery.Query>(request);
            Assert.Null(query.PageSize);
            Assert.Null(query.PageNumber);
            return expected;
        });

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/audits");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAudits_ShouldReturnBadRequest_WhenPagingParametersAreInvalid()
    {
        var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called for invalid query values."));

        await using var host = await CreateHostAsync(sender);
        host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

        var response = await host.Client.GetAsync("/api/audits?pageSize=abc&pageNumber=1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        new GetAuditsQueryEndpoint().AddRoutes(app);

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