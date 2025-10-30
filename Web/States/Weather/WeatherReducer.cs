using Fluxor;

namespace Web.States.Weather;

public static class Reducers
{
    [ReducerMethod(typeof(FetchWeatherForecasts))]
    public static WeatherState OnFetchWeatherForecasts(WeatherState state) => new() { IsLoading = true };

    [ReducerMethod]
    public static WeatherState ReduceDataFetchedAction(WeatherState state, DataFetchedWeatherForecasts action)
    {
        return new() { IsLoading = false, Forecasts = action.Forecasts };
    }
}
