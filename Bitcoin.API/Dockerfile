# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["BitCoin.API.csproj", "./"]
RUN dotnet restore "BitCoin.API.csproj" -a $TARGETARCH
COPY . .
RUN dotnet publish "BitCoin.API.csproj" -c Release -a $TARGETARCH -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./BitCoin.API"]
