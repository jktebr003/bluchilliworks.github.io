using Microsoft.JSInterop;

namespace Web.Services;

public class DeviceService : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<DeviceService>? _objRef;

    public bool IsMobile { get; private set; }
    public ViewportInfo? CurrentViewport { get; private set; }

    public event Action<ViewportInfo>? ViewportChanged;

    public DeviceService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _objRef ??= DotNetObjectReference.Create(this);
        try
        {
            var viewport = await _jsRuntime.InvokeAsync<ViewportInfo>("addResizeListener", _objRef, nameof(OnViewportChanged));
            SetViewport(viewport);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing DeviceService: {ex.Message}");
        }
    }

    [JSInvokable]
    public void OnViewportChanged(ViewportInfo viewportInfo)
    {
        SetViewport(viewportInfo);
        ViewportChanged?.Invoke(viewportInfo);
    }

    public async Task<ViewportInfo> GetViewportInfoAsync()
    {
        try
        {
            var viewport = await _jsRuntime.InvokeAsync<ViewportInfo>("getViewportInfo");
            SetViewport(viewport);
            return viewport;
        }
        catch
        {
            var fallback = new ViewportInfo { Width = 1024, Height = 768 };
            SetViewport(fallback);
            return fallback;
        }
    }

    public string GetFooterPositionClass()
    {
        var viewport = CurrentViewport;
        if (viewport == null) return "footer-auto";

        if (viewport.Width <= 480)
            return viewport.Height <= 600 ? "footer-fixed-bottom" : "footer-auto";

        if (viewport.Width <= 768)
            return viewport.Height <= 800 ? "footer-sticky" : "footer-auto";

        return "footer-auto";
    }

    private void SetViewport(ViewportInfo? viewport)
    {
        CurrentViewport = viewport;
        IsMobile = viewport?.Width <= 768;
    }

    public void Dispose()
    {
        _objRef?.Dispose();
        _objRef = null;
    }
}

public class ViewportInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int ScrollHeight { get; set; }
    public int ClientHeight { get; set; }
    public bool IsLandscape { get; set; }
}