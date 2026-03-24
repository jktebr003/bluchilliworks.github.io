using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace MudBlazorWeb.Infrastructure.Telemetry;

/// <summary>
/// Captures client-side route navigation events for MudBlazorWeb.
/// </summary>
public sealed class NavigationTelemetryService : IDisposable
{
    private readonly NavigationManager _navigationManager;
    private string? _lastRelativeUri;
    private DateTimeOffset? _lastNavigationAt;
    private bool _disposed;

    public NavigationTelemetryService(
        NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    public void TrackInitialPage()
    {
        var currentRelativeUri = NormalizeRelativeUri(_navigationManager.Uri);
        var navigationId = CreateNavigationId();

        Console.WriteLine($"🏁 [{navigationId}] (start) -> {currentRelativeUri} - Initial page tracked");

        _lastRelativeUri = currentRelativeUri;
        _lastNavigationAt = DateTimeOffset.UtcNow;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        var stopwatch = Stopwatch.StartNew();
        var navigationId = CreateNavigationId();
        var destinationRelativeUri = NormalizeRelativeUri(args.Location);
        var sourceRelativeUri = _lastRelativeUri ?? "(unknown)";
        var navigationMode = args.IsNavigationIntercepted ? "intercepted" : "programmatic";

        Console.WriteLine($"🧭 [{navigationId}] {sourceRelativeUri} -> {destinationRelativeUri} - Navigation started ({navigationMode})");

        var now = DateTimeOffset.UtcNow;
        var elapsedSincePreviousMs = _lastNavigationAt is null ? 0 : (long)(now - _lastNavigationAt.Value).TotalMilliseconds;

        stopwatch.Stop();

        Console.WriteLine($"✅ [{navigationId}] {sourceRelativeUri} -> {destinationRelativeUri} - Navigation completed in {stopwatch.ElapsedMilliseconds}ms (Mode={navigationMode}, ElapsedMsSincePrevious={elapsedSincePreviousMs})");

        _lastRelativeUri = destinationRelativeUri;
        _lastNavigationAt = now;
    }

    private string NormalizeRelativeUri(string absoluteUri)
    {
        var relative = _navigationManager.ToBaseRelativePath(absoluteUri);
        if (string.IsNullOrWhiteSpace(relative))
        {
            return "/";
        }

        var pathOnly = relative.Split('?', '#')[0];
        return string.IsNullOrWhiteSpace(pathOnly) ? "/" : $"/{pathOnly.TrimStart('/')}";
    }

    private static string CreateNavigationId()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _navigationManager.LocationChanged -= OnLocationChanged;
        _disposed = true;
    }
}