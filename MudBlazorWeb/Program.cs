using Blazor.SubtleCrypto;

using BlazorDownloadFile;

using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;

using log4net.Config;

using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor.Services;

using MudBlazorWeb.Components;
using MudBlazorWeb.Features.Authentication;
using MudBlazorWeb.Infrastructure;
using MudBlazorWeb.Shared.Helpers;
using MudBlazorWeb.Shared.Services;

using MudExtensions.Services;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Extensions;

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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseStaticFiles();
app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();
app.UseAntiforgery();


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
