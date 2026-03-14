using Fluxor;
using MudBlazorWeb.Features.Audits.Application;

namespace MudBlazorWeb.Features.Audits.UI;

public record AuditsState
{
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public List<GetAuditsQuery.AuditDto> Audits { get; init; } = new();
}

public class AuditsFeatureState : Feature<AuditsState>
{
    public override string GetName() => nameof(AuditsState);
    protected override AuditsState GetInitialState() => new AuditsState { IsLoading = false };
}
