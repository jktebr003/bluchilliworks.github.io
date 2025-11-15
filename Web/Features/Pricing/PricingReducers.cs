using Fluxor;

namespace Web.Features.Pricing;

public static class PricingReducers
{
    [ReducerMethod(typeof(LoadPackagesAction))]
    public static PricingState ReduceLoadPackages(PricingState state) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static PricingState ReduceLoadPackagesSuccess(PricingState state, LoadPackagesSuccessAction action)
    {
        var filteredPackages = state.SelectedPackageType == global::Shared.Enums.PackageType.None
            ? action.Packages
            : action.Packages.Where(p => p.Type == state.SelectedPackageType).ToList();

        return state with 
        { 
            IsLoading = false, 
            AllPackages = action.Packages,
            FilteredPackages = filteredPackages,
            ErrorMessage = null 
        };
    }

    [ReducerMethod]
    public static PricingState ReduceLoadPackagesFailed(PricingState state, LoadPackagesFailedAction action) =>
        state with { IsLoading = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static PricingState ReduceFilterPackagesByType(PricingState state, FilterPackagesByTypeAction action)
    {
        var filteredPackages = action.PackageType == global::Shared.Enums.PackageType.None
            ? state.AllPackages
            : state.AllPackages.Where(p => p.Type == action.PackageType).ToList();

        return state with 
        { 
            SelectedPackageType = action.PackageType,
            FilteredPackages = filteredPackages 
        };
    }
}
