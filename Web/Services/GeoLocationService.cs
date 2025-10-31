using Microsoft.JSInterop;

namespace Web.Services;

public class GeoLocationService
{
    private readonly IJSRuntime _js;

    public GeoLocationService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
    {
        try
        {
            var pos = await _js.InvokeAsync<GeoPosition>("safeGetCurrentPosition");
            return (pos.latitude, pos.longitude);
        }
        catch (JSException ex)
        {
            // Handle errors (e.g., permission denied, unavailable)
            throw new InvalidOperationException($"Failed to get location: {ex.Message}", ex);
        }
    }

    private class GeoPosition
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
