# syntax=docker/dockerfile:1
# Multi-stage build for Purchases.API (.NET 10).
# Build context MUST be the backend/ root so the Shared.Contracts project reference resolves:
#   docker build -f Purchases.API/Dockerfile -t purchases-api .

# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (layer cache): copy only the project files the app depends on.
COPY Shared.Contracts/Shared.Contracts.csproj Shared.Contracts/
COPY Purchases.API/Purchases.API.csproj Purchases.API/
RUN dotnet restore Purchases.API/Purchases.API.csproj

# Copy the sources and publish a framework-dependent, trimmed-of-apphost build.
COPY Shared.Contracts/ Shared.Contracts/
COPY Purchases.API/ Purchases.API/
RUN dotnet publish Purchases.API/Purchases.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# App Runner routes to this port; overrides the localhost "Urls" from appsettings.json.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

# Run as the non-root user shipped in the .NET base image (APP_UID=1654).
USER $APP_UID

ENTRYPOINT ["dotnet", "Purchases.API.dll"]
