using Fluxor;
using static Web.Pages.Weather;
using System.Net.Http.Json;

namespace Web.States.Weather;

public class WeatherEffects(HttpClient http)
{
    [EffectMethod(typeof(FetchWeatherForecasts))]
    public async Task HandleFetchDataAction(IDispatcher dispatcher)
    {
        var forecasts = await http.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json")
                        ?? [];
        dispatcher.Dispatch(new DataFetchedWeatherForecasts(forecasts));
    }
}
