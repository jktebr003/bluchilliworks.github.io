using System.Net;
using System.Net.Http.Json;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MudBlazorWeb.Features.Pricing.Application;
using MudBlazorWeb.Shared;

using Xunit;

namespace MudBlazorWeb.Features.Pricing.Tests;

public class PackageEndpointsTests
{
	private const string ApiKey = "primary-test-key";

	[Fact]
	public async Task GetPackage_ShouldReturnOk_WhenPackageExists()
	{
		var packageId = Guid.NewGuid();
		var dto = new GetPackageQuery.PackageDto("Starter", "Starter plan", "STR", "10", Shared.Enums.PackageType.Monthly, true)
		{
			Id = packageId
		};
		var expected = new Result<GetPackageQuery.PackageDto>(dto, true);

		var sender = new FakeSender(request =>
		{
			var query = Assert.IsType<GetPackageQuery.Query>(request);
			Assert.Equal(packageId, query.Id);
			return expected;
		});

		await using var host = await CreateHostAsync(sender);
		host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

		var response = await host.Client.GetAsync($"/api/packages/{packageId}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var payload = await response.Content.ReadFromJsonAsync<Result<GetPackageQuery.PackageDto>>();
		Assert.NotNull(payload);
		Assert.True(payload!.Success);
		Assert.Equal(packageId, payload.Value.Id);
		Assert.Equal("Starter", payload.Value.Name);
	}

	[Fact]
	public async Task GetPackage_ShouldReturnNotFoundPayload_WhenPackageDoesNotExist()
	{
		var emptyDto = new GetPackageQuery.PackageDto(string.Empty, string.Empty, string.Empty, string.Empty, default, false);
		var expected = new Result<GetPackageQuery.PackageDto>(emptyDto, false, "GetPackage.Null", "The package with the specified ID was not found");

		var sender = new FakeSender(request =>
		{
			Assert.IsType<GetPackageQuery.Query>(request);
			return expected;
		});

		await using var host = await CreateHostAsync(sender);
		host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

		var response = await host.Client.GetAsync($"/api/packages/{Guid.NewGuid()}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var payload = await response.Content.ReadFromJsonAsync<Result<GetPackageQuery.PackageDto>>();
		Assert.NotNull(payload);
		Assert.False(payload!.Success);
		Assert.Equal("GetPackage.Null", payload.Code);
	}

	[Fact]
	public async Task GetPackage_ShouldReturnBadRequest_WhenGuidIsInvalid()
	{
		var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called for invalid route values."));

		await using var host = await CreateHostAsync(sender);
		host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

		var response = await host.Client.GetAsync("/api/packages/not-a-guid");

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task GetPackage_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
	{
		var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

		await using var host = await CreateHostAsync(sender);

		var response = await host.Client.GetAsync($"/api/packages/{Guid.NewGuid()}");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task GetPackages_ShouldReturnOkWithPagedResult_WhenParametersAreValid()
	{
		var expectedData = new List<GetPackagesQuery.PackageDto>
		{
			new("Starter", "Starter plan", "STR", "10", Shared.Enums.PackageType.Monthly, true),
			new("Pro", "Pro plan", "PRO", "25", Shared.Enums.PackageType.TwoYearSubscription, true)
		};

		var expected = new PagedResult<List<GetPackagesQuery.PackageDto>>(expectedData, true, 1, 2, 1, 10);

		var sender = new FakeSender(request =>
		{
			var query = Assert.IsType<GetPackagesQuery.Query>(request);
			Assert.Equal(10, query.PageSize);
			Assert.Equal(1, query.PageNumber);
			return expected;
		});

		await using var host = await CreateHostAsync(sender);
		host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

		var response = await host.Client.GetAsync("/api/packages?pageSize=10&pageNumber=1");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var payload = await response.Content.ReadFromJsonAsync<PagedResult<List<GetPackagesQuery.PackageDto>>>();
		Assert.NotNull(payload);
		Assert.True(payload!.Success);
		Assert.Equal(2, payload.TotalItems);
		Assert.Equal(2, payload.Value.Count);
	}

	[Fact]
	public async Task GetPackages_ShouldReturnUnauthorized_WhenAuthorizationHeaderIsMissing()
	{
		var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called when unauthorized."));

		await using var host = await CreateHostAsync(sender);

		var response = await host.Client.GetAsync("/api/packages");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task GetPackages_ShouldForwardNullPaging_WhenParametersAreMissing()
	{
		var expected = new PagedResult<List<GetPackagesQuery.PackageDto>>(new List<GetPackagesQuery.PackageDto>(), true, 0, 0, null, null);

		var sender = new FakeSender(request =>
		{
			var query = Assert.IsType<GetPackagesQuery.Query>(request);
			Assert.Null(query.PageSize);
			Assert.Null(query.PageNumber);
			return expected;
		});

		await using var host = await CreateHostAsync(sender);
		host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

		var response = await host.Client.GetAsync("/api/packages");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task GetPackages_ShouldReturnBadRequest_WhenPagingParametersAreInvalid()
	{
		var sender = new FakeSender(_ => throw new InvalidOperationException("Sender should not be called for invalid query values."));

		await using var host = await CreateHostAsync(sender);
		host.Client.DefaultRequestHeaders.Add("Authorization", ApiKey);

		var response = await host.Client.GetAsync("/api/packages?pageSize=abc&pageNumber=1");

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

		new GetPackageQueryEndpoint().AddRoutes(app);
		new GetPackagesQueryEndpoint().AddRoutes(app);

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
