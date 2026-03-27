using MudBlazorWeb.Features.Authentication.Domain;
using MudBlazorWeb.Features.Users.Application;
using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Features.Users.Infrastructure;
using MudBlazorWeb.Shared;

namespace MudBlazorWeb.Features.Users;

public static class DependencyInjection
{
	public static IServiceCollection AddUserFeature(this IServiceCollection services)
	{
		services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IJobRepository, JobRepository>();
		services.AddScoped<ICertificationRepository, CertificationRepository>();
		services.AddScoped<IQualificationRepository, QualificationRepository>();
		services.AddScoped<IAuthenticationUserStore, AuthenticationUserStore>();
		services.AddScoped<IDomainEventHandler<UserJobsUpdatedEvent>, UserJobsUpdatedEventHandler>();
		services.AddScoped<IDomainEventHandler<UserCertificationsUpdatedEvent>, UserCertificationsUpdatedEventHandler>();
		services.AddScoped<IDomainEventHandler<UserQualificationsUpdatedEvent>, UserQualificationsUpdatedEventHandler>();

		return services;
	}
}
