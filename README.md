[![Build Status](https://dev.azure.com/rahulrajan-gitlab/Bitcoin/_apis/build/status/Bitcoin-ASP.NET%20Core-CI?branchName=master)](https://dev.azure.com/rahulrajan-gitlab/Bitcoin/_build/latest?definitionId=1&branchName=master)

# BitCoin.API
API for retrieving the latest Bit coin price.

## Usage

```cmd
dotnet run
``` 

## Authentication

All API endpoints require a valid JWT bearer token in the `Authorization` header.

`Jwt` configuration values are required for token validation:

- `Key`: Symmetric secret used to validate the token signature so tampered/forged tokens are rejected.
- `Issuer`: Ensures the token was issued by the expected authority.
- `Audience`: Ensures the token was created for this API and not another service.
