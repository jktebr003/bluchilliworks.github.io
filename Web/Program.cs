using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Extensions;

using Solutaris.InfoWARE.ProtectedBrowserStorage.Extensions;

using Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true)
    .AddJsonFile("appsettings.Build.json", optional: true);


// Root Components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Core Services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddIWProtectedBrowserStorageAsSingleton("WFCC7h70VDhZjS7AIJsGpvOGVoNNLp3aVM0OCNf8CSZQ78MphFCeNhf3XrxKLAnyO1iAWoBPJtUSIKsc");
builder.Services.AddGeolocationServices();

// or this to add only the MudBlazor.Extensions but please ensure that this is added after mud servicdes are added. That means after `AddMudServices`
builder.Services.AddMudServicesWithExtensions();

// HTTP Client
//builder.Services.AddHttpClient<WebApiClient>(client =>
//{
//    var baseApiServiceUrl = builder.Configuration["BaseApiServiceUrl"];
//    if (string.IsNullOrEmpty(baseApiServiceUrl))
//        throw new ArgumentNullException(nameof(baseApiServiceUrl), "BaseApiServiceUrl is not configured.");
//    client.DefaultRequestHeaders.AcceptLanguage.Clear();
//    client.DefaultRequestHeaders.Add("Authorization", builder.Configuration["AuthorizationKey"]);
//    client.BaseAddress = new Uri(baseApiServiceUrl);
//});

// Fluxor (State Management)
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

await builder.Build().RunAsync();
