var builder = DistributedApplication.CreateBuilder(args);

var coreApp = builder.AddProject<Projects.ModulesCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
