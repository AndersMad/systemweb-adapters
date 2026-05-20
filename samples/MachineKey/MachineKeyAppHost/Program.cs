var builder = DistributedApplication.CreateBuilder(args);

var coreApp = builder.AddProject<Projects.MachineKeyCore>("core")
    .WithHttpHealthCheck();

builder.Build().Run();
