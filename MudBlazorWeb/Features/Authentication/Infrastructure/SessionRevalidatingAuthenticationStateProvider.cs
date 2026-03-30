using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Options;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal sealed class SessionRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SessionAuthenticationOptions _options;

    public SessionRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<SessionAuthenticationOptions> options)
        : base(loggerFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(_options.RevalidationIntervalSeconds);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        if (authenticationState.User.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var userSessionManager = scope.ServiceProvider.GetRequiredService<IUserSessionManager>();
        var validation = await userSessionManager.ValidatePrincipalAsync(authenticationState.User, markActivity: false, cancellationToken);

        return validation.IsAuthenticated;
    }
}