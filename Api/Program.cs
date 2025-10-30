//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

using Api.Extensions.Swagger;
using Api.Features.Messages;
using Api.Features.Packages;
using Api.Features.Posts;
using Api.Features.Users;
using Api.Features.UserSessions;
using Api.Infrastructure.Database.MongoDb.Repositories;

using Asp.Versioning;

using Carter;

using FluentValidation;

using Hangfire;
using Hangfire.Mongo;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using MongoDB.Driver;
using MongoDB.Entities;

using Serilog;
using Serilog.Exceptions;

using Swashbuckle.AspNetCore.SwaggerGen;

using System;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", false, true)
                     .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
                     .AddCommandLine(args)
                     .AddEnvironmentVariables();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddCors(o =>
       o.AddDefaultPolicy(b =>
           b.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
       )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                        options.Policies.Sunset(0.9)
                                        .Effective(DateTimeOffset.Now.AddDays(60))
                                        .Link("policy.html")
                                        .Title("Versioning Policy")
                                        .Type("text/html");
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                        options.AssumeDefaultVersionWhenUnspecified = true;
                    })
                .AddMvc()
                .AddApiExplorer(
                    options =>
                    {
                        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
                        options.GroupNameFormat = "'v'VVV";

                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                        // can also be used to control the format of the API version in route templates
                        options.SubstituteApiVersionInUrl = true;
                    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Description = "api key.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "basic"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                },
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    // add a custom operation filter which sets default values
    c.OperationFilter<SwaggerDefaultValues>();

    var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

    // integrate xml comments
    c.IncludeXmlComments(filePath);
    Console.WriteLine($"✅ XML documentation loaded: {filePath}");

    c.ResolveConflictingActions(apiDescriptions =>
    {
        var descriptions = apiDescriptions as ApiDescription[] ?? apiDescriptions.ToArray();
        var first = descriptions.First(); // build relative to the 1st method
        var parameters = descriptions.SelectMany(d => d.ParameterDescriptions).ToList();

        first.ParameterDescriptions.Clear();
        // add parameters and make them optional
        foreach (var parameter in parameters)
            if (first.ParameterDescriptions.All(x => x.Name != parameter.Name))
            {
                first.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    ModelMetadata = parameter.ModelMetadata,
                    Name = parameter.Name,
                    ParameterDescriptor = parameter.ParameterDescriptor,
                    Source = parameter.Source,
                    IsRequired = false,
                    DefaultValue = null
                });
            }
        return first;
    });
});

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();

builder.Services.AddValidatorsFromAssembly(assembly);

try
{
    //builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
    //builder.Services.AddScoped(typeof(IBaseRepository<AppRole>), typeof(BaseRepository<AppRole>));

    builder.Services.AddScoped<IMongoDbRepository, MongoDbRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
    builder.Services.AddScoped<IPostRepository, PostRepository>();
    builder.Services.AddScoped<IPackageRepository, PackageRepository>();
    builder.Services.AddScoped<IMessageRepository, MessageRepository>();

    Console.WriteLine("✅ Database repositories registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database repository registrations failed: {ex.Message}");
}

//try
//{
//    builder.Services.AddHangfire(x =>
//    {
//        x.UseSimpleAssemblyNameTypeSerializer()
//            .UseRecommendedSerializerSettings()
//            .UseMongoStorage(builder.Configuration.GetValue<string>("Database:ConnectionString") ?? throw new InvalidOperationException("Hangfire MongoDB connection string is not configured"), new MongoStorageOptions
//            {
//                Prefix = "hangfire.mongo."
//            });
//    });
//    builder.Services.AddHangfireServer();

//    Console.WriteLine("✅ Hangfire server registered successfully");
//}
//catch (Exception ex)
//{
//    Console.WriteLine($"⚠️ Hangfire server registration failed: {ex.Message}");
//}


// configure serilog
Log.Logger = new LoggerConfiguration()
                  .ReadFrom.Configuration(builder.Configuration)
                  .Enrich.FromLogContext()
                  .Enrich.WithExceptionDetails()
                  //.WriteTo.File(new JsonFormatter(), "logs/theteacherslounge.web-.json", rollingInterval: RollingInterval.Hour)
                  .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Hour)
                  .CreateLogger();


Console.WriteLine("✅ All available services registered successfully");



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

#region Original Swagger code
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(
//    options =>
//    {
//        var descriptions = app.DescribeApiVersions();

//        // build a swagger endpoint for each discovered API version
//        foreach (var description in descriptions)
//        {
//            var url = $"/swagger/{description.GroupName}/swagger.json";
//            var name = description.GroupName.ToUpperInvariant();
//            options.SwaggerEndpoint(url, name);
///
//        }
//    });

//    app.ApplyMigrations();
//}
#endregion

app.UseSwagger();
app.UseSwaggerUI(
options =>
{
    var descriptions = app.DescribeApiVersions();

    // build a swagger endpoint for each discovered API version
    foreach (var description in descriptions)
    {
        var url = $"/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        options.SwaggerEndpoint(url, name);
    }
});

Console.WriteLine("✅ Swagger configured");

app.MapCarter();

app.UseHttpsRedirection();
app.UseCors();
//app.UseAuthentication();

//app.UseAuthorization();
//app.UseHangfireDashboard();

try
{
    Task.Run(async () =>
    {
        //const string connectionUri = "mongodb+srv://ebrahimjakoet:jktebr003@cluster0.byl81xl.mongodb.net/?retryWrites=true&w=majority";
        //const string connectionUri = "mongodb://127.0.0.1:27017";
        string connectionUri = $"{builder.Configuration.GetValue<string>("Database:ConnectionString")}";
        string databaseName = $"{builder.Configuration.GetValue<string>("Database:DatabaseName")}";
        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        // Set the ServerApi field of the settings object to Stable API version 1
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);

        await DB.InitAsync($"{databaseName}", settings); //initialize db connection
        //await DB.InitAsync("ttl"); //initialize db connection

        //Console.WriteLine($"🗄️ Database connection: {(canConnect ? "✅ Success" : "❌ Failed")}");
    })
    .GetAwaiter()
    .GetResult();
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database test failed: {ex.Message}");
}

Console.WriteLine("\n🚀 API Service - All Issues Resolved!");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Failed to run app: {ex.Message}");
}
 

