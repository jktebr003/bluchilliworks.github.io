using Microsoft.JSInterop;

namespace Web.Services;

public sealed class GeoLocationService
{
    private readonly IJSRuntime _js;

    public GeoLocationService(IJSRuntime js) => _js = js;

    public async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
    {
        try
        {
            var pos = await _js.InvokeAsync<GeoPosition>("safeGetCurrentPosition");
            return (pos.Latitude, pos.Longitude);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to get location: {ex.Message}", ex);
        }
    }

    private sealed class GeoPosition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
