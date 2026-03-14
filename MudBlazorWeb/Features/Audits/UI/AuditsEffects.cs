using Fluxor;
using MediatR;
using MudBlazorWeb.Features.Audits.Application;

using static MudBlazorWeb.Features.Audits.UI.AuditsAction;

namespace MudBlazorWeb.Features.Audits.UI;

public class AuditsEffects
{
    private readonly IMediator _mediator;

    public AuditsEffects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [EffectMethod(typeof(LoadAuditsAction))]
    public async Task HandleLoadAudits(IDispatcher dispatcher)
    {
        try
        {
            var result = await _mediator.Send(new GetAuditsQuery.Query(null, null));

            if (result is { Success: true, Value: not null })
            {
                dispatcher.Dispatch(new LoadAuditsSuccessAction(result.Value));
            }
            else
            {
                dispatcher.Dispatch(new LoadAuditsFailedAction(result?.Message ?? "Failed to load audits"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadAuditsFailedAction($"Error loading audits: {ex.Message}"));
        }
    }
}
