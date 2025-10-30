using static Web.Pages.Weather;

namespace Web.States.Weather;

public record FetchWeatherForecasts;

public record DataFetchedWeatherForecasts(IEnumerable<WeatherForecast> Forecasts);
