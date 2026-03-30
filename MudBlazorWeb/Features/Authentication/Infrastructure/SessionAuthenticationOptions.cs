namespace MudBlazorWeb.Features.Authentication.Infrastructure;

public sealed class SessionAuthenticationOptions
{
    public const string SectionName = "Authentication:Session";

    public string CookieName { get; set; } = "__Host-MudBlazorWeb.Session";
    public int IdleTimeoutMinutes { get; set; } = 30;
    public int AbsoluteLifetimeHours { get; set; } = 8;
    public int RevalidationIntervalSeconds { get; set; } = 60;
    public int ActivityRefreshThrottleSeconds { get; set; } = 60;
}