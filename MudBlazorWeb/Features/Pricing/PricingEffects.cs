using Fluxor;

using static MudBlazorWeb.Features.Pricing.PricingAction;

namespace MudBlazorWeb.Features.Pricing;

public class PricingEffects
{
    //private readonly WebApiClient _apiClient;

    //public PricingEffects(WebApiClient apiClient)
    //{
    //    _apiClient = apiClient;
    //}

    //[EffectMethod(typeof(LoadPackagesAction))]
    //public async Task HandleLoadPackages(IDispatcher dispatcher)
    //{
    //    try
    //    {
    //        // Check API connectivity before making the call
    //        if (!await _apiClient.IsApiHealthyAsync())
    //        {
    //            dispatcher.Dispatch(new LoadPackagesFailedAction("Unable to connect to the API. Please check your network connection."));
    //            return;
    //        }

    //        var result = await _apiClient.Get<PagedApiResult<List<PackageResponse>>>(
    //            new WebApiClientInfo<object> { Method = "packages", Request = string.Empty }
    //        );

    //        if (result?.Success == true && result.Value != null)
    //        {
    //            dispatcher.Dispatch(new LoadPackagesSuccessAction(result.Value));
    //        }
    //        else
    //        {
    //            dispatcher.Dispatch(new LoadPackagesFailedAction(result?.Message ?? "Failed to load packages"));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        dispatcher.Dispatch(new LoadPackagesFailedAction($"Error loading packages: {ex.Message}"));
    //    }
    //}
}
