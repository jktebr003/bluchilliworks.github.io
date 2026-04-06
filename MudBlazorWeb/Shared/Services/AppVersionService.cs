using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace MudBlazorWeb.Shared.Services;

public interface IAppVersionService
{
    string DisplayVersion { get; }
    string InformationalVersion { get; }
}

public sealed class AppVersionService : IAppVersionService
{
    private const string AppVersionSettingsKey = "AppVersion";

    public string DisplayVersion { get; }
    public string InformationalVersion { get; }

    public AppVersionService(IConfiguration configuration)
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(AppVersionService).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        var fileVersion = assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()
            ?.Version;
        var appSettingsVersion = configuration[AppVersionSettingsKey];

        InformationalVersion = string.IsNullOrWhiteSpace(informational)
            ? appSettingsVersion ?? fileVersion ?? "0.0.0"
            : informational;

        DisplayVersion = ExtractDisplayVersion(InformationalVersion, fileVersion ?? appSettingsVersion);
    }

    private static string ExtractDisplayVersion(string informationalVersion, string? fallbackVersion)
    {
        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            var plusIndex = informationalVersion.IndexOf('+');
            var versionCore = plusIndex >= 0
                ? informationalVersion[..plusIndex]
                : informationalVersion;

            if (!string.IsNullOrWhiteSpace(versionCore))
            {
                return versionCore;
            }
        }

        return string.IsNullOrWhiteSpace(fallbackVersion)
            ? "0.0.0"
            : fallbackVersion!;
    }
}
