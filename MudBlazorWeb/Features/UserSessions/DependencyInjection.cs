using MudBlazorWeb.Features.UserSessions.Domain;
using MudBlazorWeb.Features.UserSessions.Infrastructure;

namespace MudBlazorWeb.Features.UserSessions;

public static class DependencyInjection
{
	public static IServiceCollection AddUserSessionFeature(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

		services.AddScoped<IUserSessionRepository, UserSessionRepository>();

		return services;
	}
}
