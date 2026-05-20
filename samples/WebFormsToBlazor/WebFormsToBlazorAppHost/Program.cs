var builder = DistributedApplication.CreateBuilder(args);

var coreApp = builder.AddProject<Projects.WebFormsToBlazorCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
