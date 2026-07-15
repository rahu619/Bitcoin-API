[![CI](https://github.com/rahu619/Bitcoin-API/actions/workflows/ci.yml/badge.svg)](https://github.com/rahu619/Bitcoin-API/actions/workflows/ci.yml)

# BitCoin.API
API for retrieving the latest Bit coin price.

## Architecture

Layered, dependencies flow inward only:

```
BitCoin.Domain            no dependencies (entities/value objects)
    ^
BitCoin.Application        ports (interfaces) + use cases, depends on Domain only
    ^
BitCoin.Infrastructure     implements Application's ports (Redis, CoinGecko HTTP client), depends on Application + Domain
    ^
BitCoin.API                controllers, hosting, DI composition root, depends on Application + Infrastructure
```

`BitCoin.AppHost` (.NET Aspire orchestrator) and `BitCoin.ServiceDefaults` (shared OpenTelemetry/health-check/resilience
extensions) sit alongside this as standard Aspire template projects, not architectural layers.

## Usage

Open this repository in a Dev Container (VS Code command: "Dev Containers: Reopen in Container").

Requires Docker (or another container runtime) since the AppHost provisions a Redis container for the
distributed cache.

```cmd
dotnet restore BitCoin.API.slnx
dotnet run --project src/BitCoin.AppHost
``` 

This opens the [Aspire dashboard](https://aka.ms/dotnet/aspire/dashboard) with traces, metrics, and
structured logs for both the API and its Redis cache. See [BitCoin.API.http](src/BitCoin.API/BitCoin.API.http)
for ready-to-run requests to exercise it.

You can still run the API directly when needed, but you'll need a Redis instance reachable via the
`ConnectionStrings:cache` configuration value (e.g. `ConnectionStrings__cache=localhost:6379`):

```cmd
dotnet run --project src/BitCoin.API/BitCoin.API.csproj
```

## Authentication

All API endpoints require a valid JWT bearer token in the `Authorization` header.

`Jwt` configuration values are required for token validation:

- `Key`: Symmetric secret used to validate the token signature so tampered/forged tokens are rejected.
- `Issuer`: Ensures the token was issued by the expected authority.
- `Audience`: Ensures the token was created for this API and not another service.

`scripts/generate-dev-jwt.sh` mints a local dev token signed with the placeholder `Jwt:Key` value in
`appsettings.json` (override via `JWT_KEY`/`JWT_ISSUER`/`JWT_AUDIENCE` env vars to match a real configured key).

### Secrets

`appsettings.json`'s `Jwt:Key` is a placeholder (`__USE_ENVIRONMENT_JWT_SECRET_32_CHARS__`), never a real value.

- **Local development**: set the real key with .NET user-secrets, scoped to the `BitCoin.API` project's
  `UserSecretsId`: `dotnet user-secrets set "Jwt:Key" "<value>" --project src/BitCoin.API`.
- **Other environments**: supply it via environment variable (`Jwt__Key`) or a managed secret store
  (e.g. Azure Key Vault). Never commit a real value to `appsettings.json` or `appsettings.*.json`.

## CORS

Allowed origins are configured, not hardcoded, via `Cors:AllowedOrigins` in `appsettings.json` (an array of
origin strings). Override per environment with `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.
