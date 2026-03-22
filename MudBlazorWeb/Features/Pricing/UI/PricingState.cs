using Fluxor;
using MudBlazorWeb.Features.Pricing.Application;
using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Pricing.UI;

public record PricingState
{
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public List<GetPackagesQuery.PackageDto> AllPackages { get; init; } = new();
    public List<GetPackagesQuery.PackageDto> FilteredPackages { get; init; } = new();
    public PackageType SelectedPackageType { get; init; } = PackageType.None;
    public GetPackagesQuery.PackageDto? SelectedPackage { get; init; }
}

public class PricingFeatureState : Feature<PricingState>
{
    public override string GetName() => nameof(PricingState);
    protected override PricingState GetInitialState() => new PricingState { IsLoading = false };
}
