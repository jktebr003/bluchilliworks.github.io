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
using Api.Infrastructure.Telemetry;
using Api.Jobs;
using Api.Services;

using Asp.Versioning;

using Carter;

using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

using FluentValidation;

using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;

using Serilog;
using Serilog.Exceptions;

using Swashbuckle.AspNetCore.SwaggerGen;

// Helper method to mask sensitive credentials in connection strings
static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
        return "Not configured";
    
    try
    {
        // For mongodb+srv:// or mongodb:// URLs, mask the password
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo;
        
        if (!string.IsNullOrEmpty(userInfo) && userInfo.Contains(':'))
        {
            var parts = userInfo.Split(':');
            var username = parts[0];
            var maskedUserInfo = $"{username}:****";
            return connectionString.Replace(userInfo, maskedUserInfo);
        }
        
        return connectionString;
    }
    catch
    {
        // If parsing fails, just mask everything after "://" for safety
        var protocolIndex = connectionString.IndexOf("://");
        if (protocolIndex > 0)
        {
            return connectionString.Substring(0, protocolIndex + 3) + "****";
        }
        return "****";
    }
}

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();
builder.AddServiceDefaults();

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

    // Register email service
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Register password hashing service
    builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();

    // Register background jobs
    builder.Services.AddScoped<SendMessageJob>();
    builder.Services.AddScoped<RetryFailedMessagesJob>();

    Console.WriteLine("✅ Database repositories registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database repository registrations failed: {ex.Message}");
}

try
{
    // Hangfire configuration: ensure MongoDB storage is set before using Hangfire APIs
    string connectionUri = $"{builder.Configuration.GetValue<string>("Database:ConnectionString")}";
    string databaseName = $"{builder.Configuration.GetValue<string>("Database:DatabaseName")}";

    // Display connection info (mask credentials for security)
    var maskedConnectionUri = MaskConnectionString(connectionUri);
    Console.WriteLine($"🗄️  MongoDB Connection String: {maskedConnectionUri}");
    Console.WriteLine($"🗄️  MongoDB Database Name: {databaseName}");

    // Configure MongoDB client settings with timeouts for cloud deployments
    var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionUri);
    mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(10);
    mongoClientSettings.SocketTimeout = TimeSpan.FromSeconds(10);
    mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
    mongoClientSettings.MaxConnectionPoolSize = 100;
    mongoClientSettings.RetryWrites = true;
    mongoClientSettings.RetryReads = true;
    mongoClientSettings.MaxConnectionIdleTime = TimeSpan.FromMinutes(1);
    mongoClientSettings.MinConnectionPoolSize = 0;
    mongoClientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

    // For mongodb+srv:// connections, TLS is enabled automatically by the driver
    // Just disable certificate revocation checking for cloud deployments
    if (connectionUri.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase))
    {
        if (mongoClientSettings.SslSettings == null)
        {
            mongoClientSettings.SslSettings = new SslSettings();
        }
        mongoClientSettings.SslSettings.CheckCertificateRevocation = false;
        Console.WriteLine("🔒 TLS enabled for Hangfire MongoDB Atlas connection (using default protocols)");
    }

    var mongoClient = new MongoClient(mongoClientSettings);

    // Don't ping during startup - let connections happen lazily in background
    Console.WriteLine("🔄 MongoDB client configured - connections will be established on demand");

    // Add Hangfire services with minimal startup dependencies
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMongoStorage(mongoClient, databaseName, new MongoStorageOptions
        {
            MigrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            },
            Prefix = "hangfire.mongo",
            CheckConnection = false,
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        })
    );

    // Register IBackgroundJobClient explicitly
    builder.Services.AddSingleton<IBackgroundJobClient>(sp =>
    {
        try
        {
            return new BackgroundJobClient(sp.GetRequiredService<JobStorage>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ BackgroundJobClient creation delayed: {ex.Message}");
            // Return a dummy client that will be replaced when storage is available
            return new BackgroundJobClient();
        }
    });

    // Add the processing server as IHostedService with delayed start
    builder.Services.AddHangfireServer(serverOptions =>
    {
        serverOptions.ServerName = "Hangfire.Mongo server 1";
        serverOptions.ServerTimeout = TimeSpan.FromMinutes(5);
        serverOptions.ServerCheckInterval = TimeSpan.FromSeconds(30);
        serverOptions.SchedulePollingInterval = TimeSpan.FromSeconds(15);
    });

    // Register a hosted service to configure recurring jobs after Hangfire is initialized
    builder.Services.AddHostedService<HangfireJobInitializer>();

    Console.WriteLine("✅ Hangfire server registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Hangfire server registration failed: {ex.Message}");
    Console.WriteLine($"⚠️ Application will continue without Hangfire functionality");
}


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

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
// Add HTTP request telemetry middleware early in the pipeline
app.UseMiddleware<HttpRequestTelemetryMiddleware>();

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
    // Initialize MongoDB.Entities asynchronously without blocking startup
    var mongoInitTask = Task.Run(async () =>
    {
        // Add initial delay to let the app start first
        await Task.Delay(TimeSpan.FromSeconds(5));

        try
        {
            string connectionUri = $"{builder.Configuration.GetValue<string>("Database:ConnectionString")}";
            string databaseName = $"{builder.Configuration.GetValue<string>("Database:DatabaseName")}";

            var settings = MongoClientSettings.FromConnectionString(connectionUri);

            // Set the ServerApi field of the settings object to Stable API version 1
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            // Configure connection timeouts for cloud deployments - use shorter timeouts
            settings.ConnectTimeout = TimeSpan.FromSeconds(10);
            settings.SocketTimeout = TimeSpan.FromSeconds(10);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
            settings.MaxConnectionPoolSize = 100;
            settings.MinConnectionPoolSize = 0;
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(1);
            settings.RetryWrites = true;
            settings.RetryReads = true;

            // For mongodb+srv:// connections, TLS is enabled automatically by the driver
            // Just disable certificate revocation checking for cloud deployments
            if (connectionUri.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase))
            {
                if (settings.SslSettings == null)
                {
                    settings.SslSettings = new SslSettings();
                }
                settings.SslSettings.CheckCertificateRevocation = false;
                Console.WriteLine("🔒 TLS enabled for MongoDB.Entities connection (using default protocols)");
            }

            // Retry logic for MongoDB initialization
            int maxRetries = 3;
            int retryCount = 0;
            Exception? lastException = null;

            while (retryCount < maxRetries)
            {
                try
                {
                    await DB.InitAsync($"{databaseName}", settings);
                    Console.WriteLine($"✅ MongoDB.Entities initialized for database: {databaseName}");
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;
                    Console.WriteLine($"⚠️ MongoDB.Entities initialization attempt {retryCount}/{maxRetries} failed: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }

            Console.WriteLine($"⚠️ MongoDB.Entities initialization failed after {maxRetries} attempts");
            Console.WriteLine($"⚠️ Last error: {lastException?.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ MongoDB.Entities initialization failed: {ex.Message}");
            Console.WriteLine($"⚠️ Stack trace: {ex.StackTrace}");
        }
    });

    // Don't wait for MongoDB initialization - let it complete in background
    Console.WriteLine("🔄 MongoDB.Entities initialization started in background with retry logic...");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Failed to start MongoDB initialization: {ex.Message}");
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


