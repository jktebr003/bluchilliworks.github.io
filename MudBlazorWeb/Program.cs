using Asp.Versioning;

using Blazor.SubtleCrypto;

using BlazorDownloadFile;

using Carter;

using FluentValidation;

using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;

using log4net.Config;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using MudBlazor.Services;

using MudBlazorWeb.Components;
using MudBlazorWeb.Features.Authentication;
using MudBlazorWeb.Features.Pricing;
using MudBlazorWeb.Infrastructure;
using MudBlazorWeb.Infrastructure.Database;
using MudBlazorWeb.Infrastructure.Database.Postgres;
using MudBlazorWeb.Shared.Extensions.Swagger;
using MudBlazorWeb.Shared.Helpers;
using MudBlazorWeb.Shared.Services;

using MudExtensions.Services;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Extensions;

using Swashbuckle.AspNetCore.SwaggerGen;

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

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddApiVersioning(
//                    options =>
//                    {
//                        // reporting api versions will return the headers
//                        // "api-supported-versions" and "api-deprecated-versions"
//                        options.ReportApiVersions = true;

//                        options.Policies.Sunset(0.9)
//                                        .Effective(DateTimeOffset.Now.AddDays(60))
//                                        .Link("policy.html")
//                                        .Title("Versioning Policy")
//                                        .Type("text/html");
//                        options.DefaultApiVersion = new ApiVersion(1, 0);
//                        options.AssumeDefaultVersionWhenUnspecified = true;
//                    })
//                .AddMvc()
//                .AddApiExplorer(
//                    options =>
//                    {
//                        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
//                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
//                        options.GroupNameFormat = "'v'VVV";

//                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
//                        // can also be used to control the format of the API version in route templates
//                        options.SubstituteApiVersionInUrl = true;
//                    });

//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
//    {
//        Description = "api key.",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "basic"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "basic"
//                },
//                In = ParameterLocation.Header
//            },
//            new List<string>()
//        }
//    });

//    // add a custom operation filter which sets default values
//    c.OperationFilter<SwaggerDefaultValues>();

//    var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
//    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

//    // integrate xml comments
//    c.IncludeXmlComments(filePath);

//    c.ResolveConflictingActions(apiDescriptions =>
//    {
//        var descriptions = apiDescriptions as ApiDescription[] ?? apiDescriptions.ToArray();
//        var first = descriptions.First(); // build relative to the 1st method
//        var parameters = descriptions.SelectMany(d => d.ParameterDescriptions).ToList();

//        first.ParameterDescriptions.Clear();
//        // add parameters and make them optional
//        foreach (var parameter in parameters)
//            if (first.ParameterDescriptions.All(x => x.Name != parameter.Name))
//            {
//                first.ParameterDescriptions.Add(new ApiParameterDescription
//                {
//                    ModelMetadata = parameter.ModelMetadata,
//                    Name = parameter.Name,
//                    ParameterDescriptor = parameter.ParameterDescriptor,
//                    Source = parameter.Source,
//                    IsRequired = false,
//                    DefaultValue = null
//                });
//            }
//        return first;
//    });
//});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

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

// Features (features depend on DbContext)
builder.Services.AddPackageFeature();

// HTTP Client
builder.Services.AddHttpClient<WebApiClient>(client =>
{
    var baseApiServiceUrl = builder.Configuration["BaseApiServiceUrl"];
    if (string.IsNullOrEmpty(baseApiServiceUrl))
        throw new ArgumentNullException(nameof(baseApiServiceUrl), "BaseApiServiceUrl is not configured.");
    client.DefaultRequestHeaders.AcceptLanguage.Clear();
    client.DefaultRequestHeaders.Add("Authorization", builder.Configuration["AuthorizationKey"]);
    client.BaseAddress = new Uri(baseApiServiceUrl);
});

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddScoped<LocalStorageHelper>();
builder.Services.AddScoped<AuthenticationStateProvider, DatabaseAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<BusyDialogService>();

Console.WriteLine("✅ All available services registered successfully");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseSwagger();
//app.UseSwaggerUI(
//options =>
//{
//    var descriptions = app.DescribeApiVersions();

//    // build a swagger endpoint for each discovered API version
//    foreach (var description in descriptions)
//    {
//        var url = $"/swagger/{description.GroupName}/swagger.json";
//        var name = description.GroupName.ToUpperInvariant();
//        options.SwaggerEndpoint(url, name);
//    }
//});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseStaticFiles();
app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();
app.UseAntiforgery();

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


//app.Run();
try
{
    Console.WriteLine("Starting up...");
    app.Run();
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
