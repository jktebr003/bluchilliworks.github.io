using System.Reflection;

namespace MudBlazorWeb.Shared.Services;

public interface IAppVersionService
{
    string DisplayVersion { get; }
    string InformationalVersion { get; }
}

public sealed class AppVersionService : IAppVersionService
{
    public string DisplayVersion { get; }
    public string InformationalVersion { get; }

    public AppVersionService()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(AppVersionService).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        var fileVersion = assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            ?.Version;

        InformationalVersion = string.IsNullOrWhiteSpace(informational)
            ? fileVersion ?? "0.0.0"
            : informational;

        DisplayVersion = ExtractDisplayVersion(InformationalVersion, fileVersion);
    }

    private static string ExtractDisplayVersion(string informationalVersion, string? fallbackVersion)
    {
        var plusIndex = informationalVersion.IndexOf('+');
        var versionCore = plusIndex >= 0
            ? informationalVersion[..plusIndex]
            : informationalVersion;

        return string.IsNullOrWhiteSpace(versionCore)
            ? fallbackVersion ?? "0.0.0"
            : versionCore;
    }
}
