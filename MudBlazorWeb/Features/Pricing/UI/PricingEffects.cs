using Fluxor;
using MediatR;
using MudBlazorWeb.Features.Pricing.Application;
using static MudBlazorWeb.Features.Pricing.UI.PricingAction;

namespace MudBlazorWeb.Features.Pricing.UI;

public class PricingEffects
{
    private readonly IMediator _mediator;

    public PricingEffects(IMediator mediator)
    {
        _mediator = mediator;
    }

    [EffectMethod(typeof(LoadPackagesAction))]
    public async Task HandleLoadPackages(IDispatcher dispatcher)
    {
       try
       {
           var result = await _mediator.Send(new GetPackagesQuery.Query(null, null));

           if (result is { Success: true, Value: not null })
           {
               dispatcher.Dispatch(new LoadPackagesSuccessAction(result.Value));
           }
           else
           {
               dispatcher.Dispatch(new LoadPackagesFailedAction(result?.Message ?? "Failed to load packages"));
           }
       }
       catch (Exception ex)
       {
           dispatcher.Dispatch(new LoadPackagesFailedAction($"Error loading packages: {ex.Message}"));
       }
    }
}
