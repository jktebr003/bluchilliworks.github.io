var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Api>("api");

builder.AddProject<Projects.Web>("web")
       .WithReference(api); // Link to the backend API

builder.Build().Run();
