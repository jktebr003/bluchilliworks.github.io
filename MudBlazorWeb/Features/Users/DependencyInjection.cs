using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Features.Users.Infrastructure;

namespace MudBlazorWeb.Features.Users;

public static class DependencyInjection
{
	public static IServiceCollection AddUserFeature(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IAuthenticationUserStore, AuthenticationUserStore>();

		return services;
	}
}
