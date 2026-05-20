var builder = DistributedApplication.CreateBuilder(args);

var core = builder.AddProject<Projects.AppConfigCore>("core")
    .WithHttpHealthCheck("/");

builder.Build().Run();
