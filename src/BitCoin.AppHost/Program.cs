var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject("api", "../Bitcoin.API/BitCoin.API.csproj");

builder.Build().Run();
