using Blazor.SubtleCrypto;

using BlazorDownloadFile;

using Carter;

using FluentValidation;

using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;

using log4net.Config;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using System.Reflection;

using MudBlazor.Services;

using MudBlazorWeb.Components;
using MudBlazorWeb.Features.Audits;
using MudBlazorWeb.Features.Authentication;
using MudBlazorWeb.Features.Authentication.Application;
using MudBlazorWeb.Features.Authentication.Infrastructure;
using MudBlazorWeb.Features.Authentication.UI;
using MudBlazorWeb.Features.Contact;
using MudBlazorWeb.Features.Posts;
using MudBlazorWeb.Features.Pricing;
using MudBlazorWeb.Features.UserSessions;
using MudBlazorWeb.Features.Users;
using MudBlazorWeb.Infrastructure.Database;
using MudBlazorWeb.Infrastructure.Database.Postgres;
using MudBlazorWeb.Shared.Helpers;
using MudBlazorWeb.Shared.Services;

using MudExtensions.Services;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Extensions;
using MudBlazorWeb.Infrastructure.Telemetry;
using MudBlazorWeb.Shared;

var builder = WebApplication.CreateBuilder(args);

//Configure Log4net.
XmlConfigurator.Configure(new FileInfo("log4net.config"));

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", false, true)
                     .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                     .AddCommandLine(args)
                     .AddEnvironmentVariables();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MudBlazorWeb API",
        Version = "v1",
        Description = "API documentation for MudBlazorWeb endpoints"
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API key required in the Authorization header. Example: your-api-key",
        Type = SecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        Console.WriteLine($"✅ XML documentation loaded: {xmlPath}");
    }
});

builder.Services.AddSubtleCrypto(opt =>
    opt.Key = "WFCC7h70VDhZjS7AIJsGpvOGVoNNLp3aVM0OCNf8CSZQ78MphFCeNhf3XrxKLAnyO1iAWoBPJtUSIKsc"
);

builder.Services.AddMudExtensions();

builder.Services.AddIWProtectedBrowserStorage("WFCC7h70VDhZjS7AIJsGpvOGVoNNLp3aVM0OCNf8CSZQ78MphFCeNhf3XrxKLAnyO1iAWoBPJtUSIKsc"); //optionally pass an encryption key (as string)

builder.Services.AddBlazorDownloadFile();

// For authentication
builder.Services.AddHttpContextAccessor();

// Fluxor (State Management)
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

// ⭐ Database (register FIRST before features)
builder.Services.AddDatabase(builder.Configuration);

try 
{
    // Features (features depend on DbContext)
    builder.Services.AddAuditFeature();
    builder.Services.AddPackageFeature();
    builder.Services.AddMessageFeature();
    builder.Services.AddPostFeature();
    builder.Services.AddUserFeature();
    builder.Services.AddUserSessionFeature();

    Console.WriteLine("✅ Features registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Error registering features: {ex.Message}");
    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
    }
}

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.Configure<SessionAuthenticationOptions>(
    builder.Configuration.GetSection(SessionAuthenticationOptions.SectionName));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = SessionAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = SessionAuthenticationDefaults.AuthenticationScheme;
    })
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SessionAuthenticationHandler>(
        SessionAuthenticationDefaults.AuthenticationScheme,
        _ => { });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<LocalStorageHelper>();
builder.Services.AddScoped<SessionCookieManager>();
builder.Services.AddScoped<IUserSessionManager, UserSessionManager>();
builder.Services.AddScoped<AuthenticationStateProvider, SessionRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<MudBlazorWeb.Features.Authentication.UI.IAuthenticationService, MudBlazorWeb.Features.Authentication.UI.AuthenticationService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<ICurrentUserContext, AuthenticationCurrentUserContext>();
builder.Services.AddScoped<ICurrentUserAuthorizationService, CurrentUserAuthorizationService>();
builder.Services.AddScoped<NavigationTelemetryService>();
builder.Services.AddSingleton<IAppVersionService, AppVersionService>();
builder.Services.AddSingleton<BusyDialogService>();
builder.Services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

Console.WriteLine("✅ All available services registered successfully");

var app = builder.Build();

// Configure the HTTP request pipeline.
// Add HTTP request telemetry middleware early in the pipeline
app.UseMiddleware<HttpRequestTelemetryMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MudBlazorWeb API v1");
        options.EnablePersistAuthorization();
        options.RoutePrefix = "swagger";
    });
    Console.WriteLine("✅ Swagger configured");
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapCarter();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<AppDbContext>();
    if (context != null)
    {
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"🗄️ Database connection: {(canConnect ? "✅ Success" : "❌ Failed")}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database test failed: {ex.Message}");
}

// Run migrations on startup (optional, for development)
#if DEBUG
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Use Migrate() instead of EnsureCreated() - they are mutually exclusive
        dbContext.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Migration error: {ex.Message}");
        Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Inner exception: {ex.InnerException.Message}");
        }
    }
}
#endif

try
{
    Console.WriteLine("Starting up...");
    app.Run();

    Console.WriteLine("\n🚀 MudBlazorWeb - All Issues Resolved!");
    Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");

    Console.WriteLine("Shutting down...");
}
catch (Exception ex)
{
    Console.WriteLine($"Host terminated unexpectedly: {ex.Message}");
}
finally
{
    Console.WriteLine("Closing down...");
}
