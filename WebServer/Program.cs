using Blazor.SubtleCrypto;
using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Extensions;
using MudBlazor.Services;
using Solutaris.InfoWARE.ProtectedBrowserStorage.Extensions;
using WebServer.Components;
using WebServer.Features.Authentication;
using WebServer.Services;
using WebServer.Shared;
using WebServer.Shared.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<BusyDialogService>();

builder.Services.AddIWProtectedBrowserStorage("WFCC7h70VDhZjS7AIJsGpvOGVoNNLp3aVM0OCNf8CSZQ78MphFCeNhf3XrxKLAnyO1iAWoBPJtUSIKsc"); //optionally pass an encryption key (as string)
builder.Services.AddTransient<LocalStorageHelper>();

builder.Services.AddHttpClient<WebApiClient>(client =>
{
    // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
    // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
    client.DefaultRequestHeaders.AcceptLanguage.Clear();
    //client.DefaultRequestHeaders.Add("Authorization", "4D2C6F5B-45CB-45B6-B5BA-7DC77EB22934");
    client.DefaultRequestHeaders.Add("Authorization", builder.Configuration["AuthorizationKey"]);
    //client.BaseAddress = new("https+http://apiservice");
    client.BaseAddress = new(builder.Configuration["BaseApiServiceUrl"]);

});

// Fluxor (State Management)
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
    options.UseReduxDevTools();
});

builder.Services.AddSubtleCrypto(opt =>
    opt.Key = "WFCC7h70VDhZjS7AIJsGpvOGVoNNLp3aVM0OCNf8CSZQ78MphFCeNhf3XrxKLAnyO1iAWoBPJtUSIKsc"
);

builder.Services.AddScoped<AuthenticationStateProvider, DatabaseAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<GeoLocationService>();

builder.Services.AddSingleton<BusyDialogService>();

builder.Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
builder.Services.AddMudExtensions();


var app = builder.Build();

app.MapDefaultEndpoints();

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

app.Run();
