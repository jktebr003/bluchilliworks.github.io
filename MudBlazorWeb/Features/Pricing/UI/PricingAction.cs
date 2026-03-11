using MudBlazorWeb.Features.Pricing.Application;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Pricing.UI;

public class PricingAction
{
    public record LoadPackagesAction;
    public record LoadPackagesSuccessAction(List<GetPackagesQuery.PackageDto> Packages);
    public record LoadPackagesFailedAction(string ErrorMessage);
    public record FilterPackagesByTypeAction(PackageType PackageType);
    public record SelectPackageAction(GetPackagesQuery.PackageDto Package);
}
