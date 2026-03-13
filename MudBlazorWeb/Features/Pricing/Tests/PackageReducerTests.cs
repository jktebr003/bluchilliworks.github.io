using MudBlazorWeb.Features.Pricing.Application;
using MudBlazorWeb.Features.Pricing.UI;
using MudBlazorWeb.Shared.Enums;

using static MudBlazorWeb.Features.Pricing.UI.PricingAction;

using Xunit;

namespace MudBlazorWeb.Features.Pricing.Tests;

public class PackageReducerTests
{
	[Fact]
	public void ReduceLoadPackages_ShouldSetLoadingTrue_AndClearError()
	{
		var state = new PricingState
		{
			IsLoading = false,
			ErrorMessage = "old error",
			AllPackages = CreatePackages(PackageType.Monthly),
			FilteredPackages = CreatePackages(PackageType.Monthly),
			SelectedPackageType = PackageType.Monthly
		};

		var result = PricingReducers.ReduceLoadPackages(state);

		Assert.NotSame(state, result);
		Assert.True(result.IsLoading);
		Assert.Null(result.ErrorMessage);
		Assert.Equal(state.AllPackages, result.AllPackages);
		Assert.Equal(state.FilteredPackages, result.FilteredPackages);
		Assert.Equal(PackageType.Monthly, result.SelectedPackageType);
	}

	[Fact]
	public void ReduceLoadPackagesSuccess_ShouldPopulateAllAndFiltered_WhenNoTypeSelected()
	{
		var packages = CreatePackages(PackageType.Monthly, PackageType.TwoYearSubscription);
		var state = new PricingState
		{
			IsLoading = true,
			ErrorMessage = "old error",
			SelectedPackageType = PackageType.None
		};

		var action = new LoadPackagesSuccessAction(packages);

		var result = PricingReducers.ReduceLoadPackagesSuccess(state, action);

		Assert.False(result.IsLoading);
		Assert.Null(result.ErrorMessage);
		Assert.Same(packages, result.AllPackages);
		Assert.Same(packages, result.FilteredPackages);
		Assert.Equal(PackageType.None, result.SelectedPackageType);
	}

	[Fact]
	public void ReduceLoadPackagesSuccess_ShouldFilterBySelectedType_WhenTypeAlreadySelected()
	{
		var monthly = CreatePackage("Monthly", PackageType.Monthly);
		var yearly = CreatePackage("TwoYear", PackageType.TwoYearSubscription);
		var packages = new List<GetPackagesQuery.PackageDto> { monthly, yearly };

		var state = new PricingState
		{
			IsLoading = true,
			SelectedPackageType = PackageType.Monthly
		};

		var action = new LoadPackagesSuccessAction(packages);

		var result = PricingReducers.ReduceLoadPackagesSuccess(state, action);

		Assert.False(result.IsLoading);
		Assert.Null(result.ErrorMessage);
		Assert.Equal(2, result.AllPackages.Count);
		Assert.Single(result.FilteredPackages);
		Assert.Equal("Monthly", result.FilteredPackages[0].Name);
		Assert.Equal(PackageType.Monthly, result.SelectedPackageType);
	}

	[Fact]
	public void ReduceLoadPackagesSuccess_ShouldHandleEmptyPackageList()
	{
		var state = new PricingState
		{
			IsLoading = true,
			SelectedPackageType = PackageType.Monthly
		};

		var action = new LoadPackagesSuccessAction(new List<GetPackagesQuery.PackageDto>());

		var result = PricingReducers.ReduceLoadPackagesSuccess(state, action);

		Assert.False(result.IsLoading);
		Assert.Null(result.ErrorMessage);
		Assert.Empty(result.AllPackages);
		Assert.Empty(result.FilteredPackages);
	}

	[Fact]
	public void ReduceLoadPackagesFailed_ShouldSetError_AndStopLoading()
	{
		var allPackages = CreatePackages(PackageType.Monthly, PackageType.TwoYearSubscription);
		var filteredPackages = CreatePackages(PackageType.Monthly);

		var state = new PricingState
		{
			IsLoading = true,
			AllPackages = allPackages,
			FilteredPackages = filteredPackages,
			SelectedPackageType = PackageType.Monthly
		};

		var action = new LoadPackagesFailedAction("Network timeout");

		var result = PricingReducers.ReduceLoadPackagesFailed(state, action);

		Assert.False(result.IsLoading);
		Assert.Equal("Network timeout", result.ErrorMessage);
		Assert.Same(allPackages, result.AllPackages);
		Assert.Same(filteredPackages, result.FilteredPackages);
		Assert.Equal(PackageType.Monthly, result.SelectedPackageType);
	}

	[Fact]
	public void ReduceFilterPackagesByType_ShouldReturnAll_WhenTypeIsNone()
	{
		var allPackages = CreatePackages(PackageType.Monthly, PackageType.TwoYearSubscription);
		var state = new PricingState
		{
			AllPackages = allPackages,
			FilteredPackages = new List<GetPackagesQuery.PackageDto>(),
			SelectedPackageType = PackageType.Monthly
		};

		var action = new FilterPackagesByTypeAction(PackageType.None);

		var result = PricingReducers.ReduceFilterPackagesByType(state, action);

		Assert.Equal(PackageType.None, result.SelectedPackageType);
		Assert.Same(allPackages, result.FilteredPackages);
	}

	[Fact]
	public void ReduceFilterPackagesByType_ShouldFilterByRequestedType()
	{
		var monthly = CreatePackage("M", PackageType.Monthly);
		var twoYearOne = CreatePackage("Y1", PackageType.TwoYearSubscription);
		var twoYearTwo = CreatePackage("Y2", PackageType.TwoYearSubscription);

		var state = new PricingState
		{
			AllPackages = new List<GetPackagesQuery.PackageDto> { monthly, twoYearOne, twoYearTwo },
			SelectedPackageType = PackageType.None
		};

		var action = new FilterPackagesByTypeAction(PackageType.TwoYearSubscription);

		var result = PricingReducers.ReduceFilterPackagesByType(state, action);

		Assert.Equal(PackageType.TwoYearSubscription, result.SelectedPackageType);
		Assert.Equal(2, result.FilteredPackages.Count);
		Assert.All(result.FilteredPackages, p => Assert.Equal(PackageType.TwoYearSubscription, p.PackageType));
	}

	[Fact]
	public void ReduceFilterPackagesByType_ShouldReturnEmpty_WhenNoMatchExists()
	{
		var state = new PricingState
		{
			AllPackages = CreatePackages(PackageType.Monthly),
			SelectedPackageType = PackageType.None
		};

		var action = new FilterPackagesByTypeAction(PackageType.TwoYearSubscription);

		var result = PricingReducers.ReduceFilterPackagesByType(state, action);

		Assert.Equal(PackageType.TwoYearSubscription, result.SelectedPackageType);
		Assert.Empty(result.FilteredPackages);
	}

	private static List<GetPackagesQuery.PackageDto> CreatePackages(params PackageType[] types)
	{
		var packages = new List<GetPackagesQuery.PackageDto>(types.Length);
		for (int i = 0; i < types.Length; i++)
		{
			packages.Add(CreatePackage($"Package {i + 1}", types[i]));
		}

		return packages;
	}

	private static GetPackagesQuery.PackageDto CreatePackage(string name, PackageType packageType)
	{
		return new GetPackagesQuery.PackageDto(
			Name: name,
			Description: $"{name} description",
			Code: name.ToUpperInvariant().Replace(" ", string.Empty),
			Price: "10",
			PackageType: packageType,
			ShowPackage: true)
		{
			Id = Guid.NewGuid(),
			CreatedOn = DateTime.UtcNow,
			CreatedBy = "test"
		};
	}

}
