using MudBlazorWeb.Features.Audits.Domain;
using MudBlazorWeb.Features.Audits.Infrastructure;

namespace MudBlazorWeb.Features.Audits;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditFeature(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<IAuditRepository, AuditRepository>();

        return services;
    }
}
