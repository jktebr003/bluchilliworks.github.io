using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Features.Pricing.Infrastructure;

namespace MudBlazorWeb.Features.Pricing;

public static class DependencyInjection
{
    public static IServiceCollection AddPackageFeature(this IServiceCollection services)
    {
        // Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Infrastructure - Only register repository (DbContext comes from shared infrastructure)
        services.AddScoped<IPackageRepository, PackageRepository>();

        return services;
    }
}
