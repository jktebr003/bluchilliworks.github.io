using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;
using MudBlazor.Extensions.Helper;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Services;

using System.Globalization;

using Web.Services;
using Web.Shared;

namespace Web.Layout;

public partial class AlternateLayout
{
    [Inject] private IIWLocalStorageService _localStorage { get; set; }
    [Inject] private IConfiguration _configuration { get; set; }
    [Inject] private WebApiClient WebApiClient { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }
    [Inject] private DeviceService DeviceService { get; set; }
    [Inject] private IJSRuntime JSRuntime { get; set; }

    #region Custom Theme

    private static readonly MudTheme MyCustomTheme = new()
    {
        PaletteLight = new()
        {
            Primary = "#007DBA",
            Secondary = "#001489",
            Tertiary = "#00A1A3",
            Info = "#007DBA",
            Warning = "#F39C11",
            Success = "#8FAD15",
            Error = "#BA0C2F",
            AppbarBackground = "#001489",
        },
        Typography = new Typography()
        {
            Default = new DefaultTypography()
            {
                FontFamily = new[] { "Gotham", "Century Gothic", "Montserrat", "Roboto", "Tahoma", "sans-serif" }
            }
        }
    };

    #endregion Custom Theme

    private bool busy = false;
    private bool networkOnline;
    private string syncMessage = string.Empty;
    private string _versionNumber = string.Empty;
    private string _environment = string.Empty;

    private Timer? syncTimer;

    protected override async Task OnInitializedAsync()
    {
        syncTimer = new Timer(async _ =>
        {
            await CheckNetworkStatus();
            if (networkOnline)
            {
            }
            else
            {
                syncMessage = "No network connection. Please try again later.";
            }
        }, null, 0, 120000); // every 120s

        DeviceService.IsMobile = await JSRuntime.InvokeAsync<bool>("deviceInfo.isMobile");

        try
        {
            var buildInfo = _configuration.GetSection("BuildInfo").Get<BuildInfo>();
            _environment = $"{_configuration["ASPNETCORE_ENVIRONMENT"]}";
            string buildId, buildNumber;

            buildId = $"{buildInfo?.BuildId}";
            buildNumber = $"{buildInfo?.BuildNumber}";

            _versionNumber = string.IsNullOrEmpty(buildId) || string.IsNullOrEmpty(buildNumber)
                ? "VERSION 1.0.0 | Invalid Build Information"
                : GenerateVersionNumber(buildId, buildNumber);
        }
        catch (Exception ex)
        {
            //LoggerService.Instance.Error("Error retrieving build information from configuration.", ex);
            _versionNumber = "VERSION 1.0.0 | Invalid Build Information";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JSRuntime.InitializeMudBlazorExtensionsAsync();
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task CheckNetworkStatus()
    {
        networkOnline = await JSRuntime.InvokeAsync<bool>("Connection.IsOnline");
        StateHasChanged();
    }

    private static string GenerateVersionNumber(string buildId, string buildNumber)
    {
        string[] buildNumberDetails = buildNumber.Split('.');
        string date = DateTime.ParseExact(buildNumberDetails[0], "yyyyMMdd", CultureInfo.InvariantCulture)
            .ToString("dd MMMM yyyy");
        string revisionNumber = $"Rev:{buildNumberDetails[1]}";
        return $"VERSION 1.0.{buildId} | {date} [{revisionNumber}]";
    }

    private string GetContainerClasses()
    {
        return DeviceService.GetFooterPositionClass();
    }
}
