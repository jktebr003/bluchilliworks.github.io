using static WebServer.Components.Pages.Weather;

namespace WebServer.States.Weather;

public record FetchWeatherForecasts;

public record DataFetchedWeatherForecasts(IEnumerable<WeatherForecast> Forecasts);
