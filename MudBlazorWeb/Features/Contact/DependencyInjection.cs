using System;
using MudBlazorWeb.Features.Contact.Domain;
using MudBlazorWeb.Features.Contact.Infrastructure;

namespace MudBlazorWeb.Features.Contact;

public static class DependencyInjection
{
    public static IServiceCollection AddMessageFeature(this IServiceCollection services)
    {
        // Application
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Infrastructure - Only register repository (DbContext comes from shared infrastructure)
        services.AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }
}

