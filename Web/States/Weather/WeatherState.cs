using Fluxor;

using static Web.Pages.Weather;

namespace Web.States.Weather;

public record WeatherState
{
    public bool IsLoading { get; init; }
    public IEnumerable<WeatherForecast> Forecasts { get; init; } = new List<WeatherForecast>();
}

public class WeatherFeatureState : Feature<WeatherState>
{
    public override string GetName() => nameof(WeatherState);
    protected override WeatherState GetInitialState() => new WeatherState { IsLoading = false };
}
