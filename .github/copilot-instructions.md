# Bitcoin-API Copilot Guardrails

## Scope and safety
- Keep changes minimal and limited to the user request.
- Do not commit secrets, tokens, or credentials.
- Prefer existing project conventions over introducing new patterns.

## Required engineering loop
1. Inspect impacted files before changing code.
2. Run targeted verification first; run broader verification when done.
3. Summarize behavior changes and any known limitations.

## Repository-specific checks
- Use `dotnet test /home/runner/work/Bitcoin-API/Bitcoin-API/Bitcoin.API.Test/BitCoin.API.Test.csproj` for test validation.
- If solution-level test fails due path mismatch, use project-level test command above.
