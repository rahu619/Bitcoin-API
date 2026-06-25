# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/Bitcoin.API/BitCoin.API.csproj", "src/Bitcoin.API/"]
RUN dotnet restore "src/Bitcoin.API/BitCoin.API.csproj"
COPY . .
RUN dotnet publish "src/Bitcoin.API/BitCoin.API.csproj" -c Release -o /app/publish -p:PublishAot=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BitCoin.API.dll"]
