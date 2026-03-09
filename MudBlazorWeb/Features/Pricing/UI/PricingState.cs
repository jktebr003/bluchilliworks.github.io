using Fluxor;

using MudBlazorWeb.Shared.Enums;
using MudBlazorWeb.Shared.Models;

namespace MudBlazorWeb.Features.Pricing.UI;

public record PricingState
{
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public List<PackageResponse> AllPackages { get; init; } = new();
    public List<PackageResponse> FilteredPackages { get; init; } = new();
    public PackageType SelectedPackageType { get; init; } = PackageType.None;
}

public class PricingFeatureState : Feature<PricingState>
{
    public override string GetName() => nameof(PricingState);
    protected override PricingState GetInitialState() => new PricingState { IsLoading = false };
}
