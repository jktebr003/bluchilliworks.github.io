using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace MudBlazorWeb.Infrastructure.Telemetry;

/// <summary>
/// Captures client-side route navigation events for MudBlazorWeb.
/// </summary>
public sealed class NavigationTelemetryService : IDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<NavigationTelemetryService> _logger;
    private string? _lastRelativeUri;
    private DateTimeOffset? _lastNavigationAt;
    private bool _disposed;

    public NavigationTelemetryService(
        NavigationManager navigationManager,
        ILogger<NavigationTelemetryService> logger)
    {
        _navigationManager = navigationManager;
        _logger = logger;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    public void TrackInitialPage()
    {
        var currentRelativeUri = NormalizeRelativeUri(_navigationManager.Uri);
        LogNavigation("(start)", currentRelativeUri, wasIntercepted: true, isInitial: true);
        _lastRelativeUri = currentRelativeUri;
        _lastNavigationAt = DateTimeOffset.UtcNow;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        var destinationRelativeUri = NormalizeRelativeUri(args.Location);
        var sourceRelativeUri = _lastRelativeUri ?? "(unknown)";

        LogNavigation(sourceRelativeUri, destinationRelativeUri, args.IsNavigationIntercepted, isInitial: false);

        _lastRelativeUri = destinationRelativeUri;
        _lastNavigationAt = DateTimeOffset.UtcNow;
    }

    private void LogNavigation(string from, string to, bool wasIntercepted, bool isInitial)
    {
        var now = DateTimeOffset.UtcNow;
        var elapsedMs = _lastNavigationAt is null ? 0 : (long)(now - _lastNavigationAt.Value).TotalMilliseconds;

        _logger.LogInformation(
            "FrontendNavigation Initial={IsInitial} Intercepted={WasIntercepted} From={From} To={To} ElapsedMsSincePrevious={ElapsedMs}",
            isInitial,
            wasIntercepted,
            from,
            to,
            elapsedMs);
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