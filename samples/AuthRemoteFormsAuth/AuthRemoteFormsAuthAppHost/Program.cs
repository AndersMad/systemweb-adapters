var builder = DistributedApplication.CreateBuilder(args);

var coreApp = builder.AddProject<Projects.AuthRemoteFormsAuthCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
