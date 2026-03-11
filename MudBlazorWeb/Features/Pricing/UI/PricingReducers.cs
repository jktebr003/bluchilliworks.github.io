using Fluxor;

using static MudBlazorWeb.Features.Pricing.UI.PricingAction;

namespace MudBlazorWeb.Features.Pricing.UI;

public class PricingReducers
{
    [ReducerMethod(typeof(LoadPackagesAction))]
    public static PricingState ReduceLoadPackages(PricingState state) =>
        state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static PricingState ReduceLoadPackagesSuccess(PricingState state, LoadPackagesSuccessAction action)
    {
        var filteredPackages = state.SelectedPackageType == global::MudBlazorWeb.Shared.Enums.PackageType.None
            ? action.Packages
            : action.Packages.Where(p => p.PackageType == state.SelectedPackageType).ToList();

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
        var filteredPackages = action.PackageType == global::MudBlazorWeb.Shared.Enums.PackageType.None
            ? state.AllPackages
            : state.AllPackages.Where(p => p.PackageType == action.PackageType).ToList();

        return state with
        {
            SelectedPackageType = action.PackageType,
            FilteredPackages = filteredPackages
        };
    }
}
