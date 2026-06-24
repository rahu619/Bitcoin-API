---
name: validation-piv
description: Validate Bitcoin-API changes using targeted tests, safety checks, and result summary.
user-invocable: true
---

# Validation (PIV)

Use this skill before finalizing changes.

## Steps
1. Run targeted tests for modified areas first.
2. Run `dotnet test Bitcoin.API.Test/BitCoin.API.Test.csproj`.
3. Confirm no secrets were introduced in changed files.
4. Report pass/fail outcomes and any residual risk.
