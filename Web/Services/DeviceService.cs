using Microsoft.JSInterop;

namespace Web.Services;

public class DeviceService
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<DeviceService>? _objRef;

    public bool IsMobile { get; set; }
    public ViewportInfo? CurrentViewport { get; private set; }

    public event Action<ViewportInfo>? ViewportChanged;

    public DeviceService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        _objRef = DotNetObjectReference.Create(this);
        try
        {
            CurrentViewport = await _jsRuntime.InvokeAsync<ViewportInfo>("addResizeListener", _objRef, nameof(OnViewportChanged));

            // Update IsMobile based on screen width
            IsMobile = CurrentViewport?.Width <= 768;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing DeviceService: {ex.Message}");
        }
    }

    [JSInvokable]
    public void OnViewportChanged(ViewportInfo viewportInfo)
    {
        CurrentViewport = viewportInfo;
        IsMobile = viewportInfo.Width <= 768;
        ViewportChanged?.Invoke(viewportInfo);
    }

    public async Task<ViewportInfo> GetViewportInfoAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<ViewportInfo>("getViewportInfo");
        }
        catch
        {
            return new ViewportInfo { Width = 1024, Height = 768 }; // Fallback
        }
    }

    public string GetFooterPositionClass()
    {
        if (CurrentViewport == null) return "footer-auto";

        // Small screens (mobile phones)
        if (CurrentViewport.Width <= 480)
        {
            return CurrentViewport.Height <= 600 ? "footer-fixed-bottom" : "footer-auto";
        }

        // Medium screens (tablets)
        if (CurrentViewport.Width <= 768)
        {
            return CurrentViewport.Height <= 800 ? "footer-sticky" : "footer-auto";
        }

        // Large screens (desktops)
        return "footer-auto";
    }

    public void Dispose()
    {
        _objRef?.Dispose();
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