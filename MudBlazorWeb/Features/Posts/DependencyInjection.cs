using MudBlazorWeb.Features.Posts.Domain;
using MudBlazorWeb.Features.Posts.Infrastructure;

namespace MudBlazorWeb.Features.Posts;

public static class DependencyInjection
{
	public static IServiceCollection AddPostFeature(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

		services.AddScoped<IPostRepository, PostRepository>();

		return services;
	}
}
