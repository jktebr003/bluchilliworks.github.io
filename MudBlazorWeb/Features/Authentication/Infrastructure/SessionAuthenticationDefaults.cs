namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal static class SessionAuthenticationDefaults
{
    public const string AuthenticationScheme = "Session";
    public const string SessionIdClaimType = "mudblazorweb:session-id";
    public const string UserStateVersionClaimType = "mudblazorweb:user-state-version";
    public const string UsernameClaimType = "mudblazorweb:username";
}