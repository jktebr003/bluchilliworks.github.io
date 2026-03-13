using MudBlazorWeb.Features.Pricing.Application;
using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Shared.Enums;

using Xunit;

namespace MudBlazorWeb.Features.Pricing.Tests;

public class PackageHandlerTests
{
	[Fact]
	public async Task GetPackageHandler_ShouldReturnSuccess_WhenPackageExists()
	{
		var packageId = Guid.NewGuid();
		var createdOn = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
		var modifiedOn = createdOn.AddDays(3);
		var deletedOn = createdOn.AddDays(6);

		var package = new Package
		{
			Id = packageId,
			Name = "Starter",
			Description = "Starter plan",
			Code = "STR",
			Price = "10",
			Type = (int)PackageType.Monthly,
			CreatedOn = createdOn,
			CreatedBy = "system",
			ModifiedOn = modifiedOn,
			ModifiedBy = "editor",
			DeletedOn = deletedOn,
			DeletedBy = "admin",
			IsDeleted = false
		};

		var repository = new FakePackageRepository
		{
			GetPackageByIdHandler = (_, _) => Task.FromResult<Package?>(package)
		};

		var handler = new GetPackageQuery.Handler(repository);

		var result = await handler.Handle(new GetPackageQuery.Query(packageId), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Null(result.Code);
		Assert.NotNull(result.Value);
		Assert.Equal(packageId, result.Value.Id);
		Assert.Equal("Starter", result.Value.Name);
		Assert.Equal("Starter plan", result.Value.Description);
		Assert.Equal("STR", result.Value.Code);
		Assert.Equal("10", result.Value.Price);
		Assert.Equal(PackageType.Monthly, result.Value.PackageType);
		Assert.True(result.Value.ShowPackage);
		Assert.Equal(createdOn, result.Value.CreatedOn);
		Assert.Equal("system", result.Value.CreatedBy);
		Assert.Equal(modifiedOn, result.Value.ModifiedOn);
		Assert.Equal("editor", result.Value.ModifiedBy);
		Assert.Equal(deletedOn, result.Value.DeletedOn);
		Assert.Equal("admin", result.Value.DeletedBy);
		Assert.False(result.Value.IsDeleted);
	}

	[Fact]
	public async Task GetPackageHandler_ShouldReturnFailure_WhenPackageDoesNotExist()
	{
		var repository = new FakePackageRepository
		{
			GetPackageByIdHandler = (_, _) => Task.FromResult<Package?>(null)
		};

		var handler = new GetPackageQuery.Handler(repository);

		var result = await handler.Handle(new GetPackageQuery.Query(Guid.NewGuid()), CancellationToken.None);

		Assert.False(result.Success);
		Assert.Equal("GetPackage.Null", result.Code);
		Assert.Equal("The package with the specified ID was not found", result.Message);
		Assert.NotNull(result.Value);
		Assert.Equal(Guid.Empty, result.Value.Id);
		Assert.Equal(string.Empty, result.Value.Name);
		Assert.Equal(string.Empty, result.Value.Description);
		Assert.Equal(string.Empty, result.Value.Code);
		Assert.Equal(string.Empty, result.Value.Price);
		Assert.False(result.Value.ShowPackage);
	}

	[Fact]
	public async Task GetPackagesHandler_ShouldReturnPagedData_WhenPagingParametersAreValid()
	{
		var packages = CreatePackages(5);
		var repository = new FakePackageRepository
		{
			GetAllPackagesHandler = _ => Task.FromResult(packages)
		};

		var handler = new GetPackagesQuery.Handler(repository);

		var result = await handler.Handle(new GetPackagesQuery.Query(2, 2), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(3, result.TotalPages);
		Assert.Equal(5, result.TotalItems);
		Assert.Equal(2, result.PageSize);
		Assert.Equal(2, result.PageNumber);
		Assert.Equal(2, result.Value.Count);
		Assert.Equal(packages[2].Id, result.Value[0].Id);
		Assert.Equal(packages[3].Id, result.Value[1].Id);
	}

	[Fact]
	public async Task GetPackagesHandler_ShouldUseDefaults_WhenPagingParametersAreNull()
	{
		var packages = CreatePackages(3);
		var repository = new FakePackageRepository
		{
			GetAllPackagesHandler = _ => Task.FromResult(packages)
		};

		var handler = new GetPackagesQuery.Handler(repository);

		var result = await handler.Handle(new GetPackagesQuery.Query(null, null), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(1, result.TotalPages);
		Assert.Equal(3, result.TotalItems);
		Assert.Equal(3, result.PageSize);
		Assert.Equal(1, result.PageNumber);
		Assert.Equal(3, result.Value.Count);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(-10, -2)]
	public async Task GetPackagesHandler_ShouldClampInvalidPagingValues(int pageSize, int pageNumber)
	{
		var packages = CreatePackages(4);
		var repository = new FakePackageRepository
		{
			GetAllPackagesHandler = _ => Task.FromResult(packages)
		};

		var handler = new GetPackagesQuery.Handler(repository);

		var result = await handler.Handle(new GetPackagesQuery.Query(pageSize, pageNumber), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(1, result.TotalPages);
		Assert.Equal(4, result.TotalItems);
		Assert.Equal(4, result.PageSize);
		Assert.Equal(1, result.PageNumber);
		Assert.Equal(4, result.Value.Count);
	}

	[Fact]
	public async Task GetPackagesHandler_ShouldReturnAllData_WhenRequestedPageExceedsRange()
	{
		var packages = CreatePackages(3);
		var repository = new FakePackageRepository
		{
			GetAllPackagesHandler = _ => Task.FromResult(packages)
		};

		var handler = new GetPackagesQuery.Handler(repository);

		var result = await handler.Handle(new GetPackagesQuery.Query(2, 5), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(2, result.TotalPages);
		Assert.Equal(3, result.TotalItems);
		Assert.Equal(2, result.PageSize);
		Assert.Equal(5, result.PageNumber);
		Assert.Equal(3, result.Value.Count);
	}

	[Fact]
	public async Task GetPackagesHandler_ShouldHandleEmptyDataSet()
	{
		var repository = new FakePackageRepository
		{
			GetAllPackagesHandler = _ => Task.FromResult(new List<Package>())
		};

		var handler = new GetPackagesQuery.Handler(repository);

		var result = await handler.Handle(new GetPackagesQuery.Query(null, null), CancellationToken.None);

		Assert.True(result.Success);
		Assert.Equal(1, result.TotalPages);
		Assert.Equal(0, result.TotalItems);
		Assert.Equal(0, result.PageSize);
		Assert.Equal(1, result.PageNumber);
		Assert.Empty(result.Value);
	}

	[Fact]
	public async Task GetPackagesHandler_ShouldForwardCancellationTokenToRepository()
	{
		var packages = CreatePackages(1);
		var observedToken = CancellationToken.None;
		using var cancellationTokenSource = new CancellationTokenSource();

		var repository = new FakePackageRepository
		{
			GetAllPackagesHandler = token =>
			{
				observedToken = token;
				return Task.FromResult(packages);
			}
		};

		var handler = new GetPackagesQuery.Handler(repository);

		await handler.Handle(new GetPackagesQuery.Query(1, 1), cancellationTokenSource.Token);

		Assert.Equal(cancellationTokenSource.Token, observedToken);
	}

	private static List<Package> CreatePackages(int count)
	{
		var list = new List<Package>(count);

		for (int i = 1; i <= count; i++)
		{
			list.Add(new Package
			{
				Id = Guid.NewGuid(),
				Name = $"Package {i}",
				Description = $"Description {i}",
				Code = $"PKG-{i}",
				Price = (i * 10).ToString(),
				Type = (int)PackageType.Monthly,
				CreatedOn = DateTime.UtcNow,
				CreatedBy = "test"
			});
		}

		return list;
	}

	private sealed class FakePackageRepository : IPackageRepository
	{
		public Func<CancellationToken, Task<List<Package>>>? GetAllPackagesHandler { get; init; }
		public Func<Guid, CancellationToken, Task<Package?>>? GetPackageByIdHandler { get; init; }

		public Task<List<Package>> GetAllPackagesAsync(CancellationToken cancellationToken = default)
			=> GetAllPackagesHandler?.Invoke(cancellationToken)
			   ?? Task.FromResult(new List<Package>());

		public Task<Package?> GetPackageByIdAsync(Guid id, CancellationToken cancellationToken = default)
			=> GetPackageByIdHandler?.Invoke(id, cancellationToken)
			   ?? Task.FromResult<Package?>(null);

		public Task SavePackageAsync(Package package, CancellationToken cancellationToken = default)
			=> Task.CompletedTask;
	}

}
