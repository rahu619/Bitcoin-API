# Bitcoin-API Copilot Guardrails

## Architecture
- Layered: `BitCoin.Domain` (no dependencies) -> `BitCoin.Application` (ports + use cases, depends on Domain only) -> `BitCoin.Infrastructure` (implements Application's ports, depends on Application + Domain) -> `BitCoin.API` (hosting/composition root, depends on Application + Infrastructure).
- Keep dependencies flowing inward only. Domain types never reference Application/Infrastructure/Api. Application never references Infrastructure or Api.
- Wire-format DTOs for external APIs (e.g. the CoinGecko response shape) stay `internal` to `BitCoin.Infrastructure` and are mapped to Domain types before crossing a port boundary.

## Scope and safety
- Keep changes minimal and limited to the user request.
- Do not commit secrets, tokens, or credentials.
- Prefer existing project conventions over introducing new patterns.

## Required engineering loop
1. Inspect impacted files before changing code.
2. Run targeted verification first; run broader verification when done.
3. Summarize behavior changes and any known limitations.

## Repository-specific checks
- Use `dotnet test BitCoin.API.slnx` for test validation (covers `BitCoin.Application.Tests`, `BitCoin.Infrastructure.Tests`, and `BitCoin.API.Tests`).
- Use `dotnet format BitCoin.API.slnx --verify-no-changes` to check style before committing.
