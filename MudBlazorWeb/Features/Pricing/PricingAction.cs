using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Pricing;

public class PricingAction
{
    public record LoadPackagesAction;
    public record LoadPackagesSuccessAction(List<PackageResponse> Packages);
    public record LoadPackagesFailedAction(string ErrorMessage);
    public record FilterPackagesByTypeAction(PackageType PackageType);
    public record SelectPackageAction(PackageResponse Package);
}
