# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/BitCoin.Domain/BitCoin.Domain.csproj", "src/BitCoin.Domain/"]
COPY ["src/BitCoin.Application/BitCoin.Application.csproj", "src/BitCoin.Application/"]
COPY ["src/BitCoin.Infrastructure/BitCoin.Infrastructure.csproj", "src/BitCoin.Infrastructure/"]
COPY ["src/BitCoin.ServiceDefaults/BitCoin.ServiceDefaults.csproj", "src/BitCoin.ServiceDefaults/"]
COPY ["src/BitCoin.API/BitCoin.API.csproj", "src/BitCoin.API/"]
RUN dotnet restore "src/BitCoin.API/BitCoin.API.csproj"
COPY . .
RUN dotnet publish "src/BitCoin.API/BitCoin.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/healthz || exit 1
ENTRYPOINT ["dotnet", "BitCoin.API.dll"]
