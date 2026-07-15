[![Build Status](https://dev.azure.com/rahulrajan-gitlab/Bitcoin/_apis/build/status/Bitcoin-ASP.NET%20Core-CI?branchName=master)](https://dev.azure.com/rahulrajan-gitlab/Bitcoin/_build/latest?definitionId=1&branchName=master)

# BitCoin.API
API for retrieving the latest Bit coin price.

## Usage

Open this repository in a Dev Container (VS Code command: "Dev Containers: Reopen in Container").

Requires Docker (or another container runtime) since the AppHost provisions a Redis container for the
distributed cache.

```cmd
dotnet restore BitCoin.API.slnx
dotnet run --project src/BitCoin.AppHost
``` 

This opens the [Aspire dashboard](https://aka.ms/dotnet/aspire/dashboard) with traces, metrics, and
structured logs for both the API and its Redis cache. See [BitCoin.API.http](src/Bitcoin.API/BitCoin.API.http)
for ready-to-run requests to exercise it.

You can still run the API directly when needed, but you'll need a Redis instance reachable via the
`ConnectionStrings:cache` configuration value (e.g. `ConnectionStrings__cache=localhost:6379`):

```cmd
dotnet run --project src/Bitcoin.API/BitCoin.API.csproj
```

## Authentication

All API endpoints require a valid JWT bearer token in the `Authorization` header.

`Jwt` configuration values are required for token validation:

- `Key`: Symmetric secret used to validate the token signature so tampered/forged tokens are rejected.
- `Issuer`: Ensures the token was issued by the expected authority.
- `Audience`: Ensures the token was created for this API and not another service.
