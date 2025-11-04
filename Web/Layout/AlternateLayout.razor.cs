using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;
using MudBlazor.Extensions.Helper;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Services;

using System.Globalization;

using Web.Features.Authentication;
using Web.Services;
using Web.Shared;

namespace Web.Layout;

public partial class AlternateLayout
{
    [Inject] private IIWLocalStorageService? _localStorage { get; set; }
    [Inject] private IConfiguration? _configuration { get; set; }
    [Inject] private WebApiClient? WebApiClient { get; set; }
    [Inject] private NavigationManager? Navigation { get; set; }
    [Inject] private DeviceService? DeviceService { get; set; }
    [Inject] IAuthenticationService AuthService { get; set; }
    [Inject] private IJSRuntime? JSRuntime { get; set; }

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
        }, null, 0, 5000); // every 5s

        if (DeviceService is not null)
            await DeviceService.InitializeAsync();

        try
        {
            var buildInfo = _configuration?.GetSection("BuildInfo").Get<BuildInfo>();
            _environment = $"{_configuration?["ASPNETCORE_ENVIRONMENT"]}";
            string buildId, buildNumber;

            buildId = $"{buildInfo?.BuildId}";
            buildNumber = $"{buildInfo?.BuildNumber}";

            _versionNumber = string.IsNullOrEmpty(buildId) || string.IsNullOrEmpty(buildNumber)
                ? "VERSION 1.0.0 | Invalid Build Information"
                : GenerateVersionNumber(buildId, buildNumber);
        }
        catch
        {
            _versionNumber = "VERSION 1.0.0 | Invalid Build Information";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    protected async Task LogoutAsync()
    {
        await AuthService.LogoutAsync();
    }

    private async Task CheckNetworkStatus()
    {
        if (JSRuntime is not null)
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
        return DeviceService?.GetFooterPositionClass() ?? "footer-auto";
    }
}
