var builder = DistributedApplication.CreateBuilder(args);

// Add MongoDB container
var mongodb = builder.AddMongoDB("mongodb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var mongoDatabase = mongodb.AddDatabase("ttl");

// Add API project with MongoDB reference
var api = builder.AddProject<Projects.Api>("api")
    .WithReference(mongoDatabase)
    .WithEnvironment("Database__ConnectionString", mongoDatabase)
    .WithEnvironment("Database__DatabaseName", "ttl");

// Add Web project with API reference
builder.AddProject<Projects.Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
