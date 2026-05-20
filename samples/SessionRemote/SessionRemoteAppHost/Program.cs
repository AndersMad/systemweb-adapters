var builder = DistributedApplication.CreateBuilder(args);

var coreApp = builder.AddProject<Projects.SessionRemoteCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
