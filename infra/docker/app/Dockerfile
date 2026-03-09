# Multi-stage Dockerfile for Mystira App API (.NET 10)
# Build context: monorepo root (Mystira.workspace/)
#
# Usage:
#   docker build -f infra/docker/app/Dockerfile -t mystira-app-api .
#
# See ADR-0024 for monorepo migration details.

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e362a8dbcd691522456da26a5198b8f3ca1d7641c95624fadc5e3e82678bd08a AS build
WORKDIR /src

# Copy global.json for SDK version pinning
COPY global.json ./

# Copy csproj files for restore (app + all project-referenced packages)
COPY apps/app/src/Mystira.App.Api/Mystira.App.Api.csproj apps/app/src/Mystira.App.Api/
COPY apps/app/src/Mystira.App.Domain/Mystira.App.Domain.csproj apps/app/src/Mystira.App.Domain/
COPY apps/app/src/Mystira.App.Application/Mystira.App.Application.csproj apps/app/src/Mystira.App.Application/
COPY apps/app/src/Mystira.App.Infrastructure.Data/Mystira.App.Infrastructure.Data.csproj apps/app/src/Mystira.App.Infrastructure.Data/
COPY apps/app/src/Mystira.App.Infrastructure.Azure/Mystira.App.Infrastructure.Azure.csproj apps/app/src/Mystira.App.Infrastructure.Azure/
COPY apps/app/src/Mystira.App.Infrastructure.Discord/Mystira.App.Infrastructure.Discord.csproj apps/app/src/Mystira.App.Infrastructure.Discord/
COPY apps/app/src/Mystira.App.Infrastructure.Teams/Mystira.App.Infrastructure.Teams.csproj apps/app/src/Mystira.App.Infrastructure.Teams/
COPY apps/app/src/Mystira.App.Infrastructure.WhatsApp/Mystira.App.Infrastructure.WhatsApp.csproj apps/app/src/Mystira.App.Infrastructure.WhatsApp/
COPY apps/app/src/Mystira.App.Infrastructure.Payments/Mystira.App.Infrastructure.Payments.csproj apps/app/src/Mystira.App.Infrastructure.Payments/
COPY apps/app/src/Mystira.App.Infrastructure.Chain/Mystira.App.Infrastructure.Chain.csproj apps/app/src/Mystira.App.Infrastructure.Chain/
COPY packages/contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj packages/contracts/dotnet/Mystira.Contracts/
COPY packages/shared/Mystira.Shared/Mystira.Shared.csproj packages/shared/Mystira.Shared/
COPY packages/shared/Mystira.Shared.Generators/Mystira.Shared.Generators.csproj packages/shared/Mystira.Shared.Generators/
COPY packages/domain/Mystira.Domain/Mystira.Domain.csproj packages/domain/Mystira.Domain/
COPY packages/application/Mystira.Application/Mystira.Application.csproj packages/application/Mystira.Application/

# Restore dependencies (no NuGet auth needed — internal packages are ProjectReferences)
RUN dotnet restore "apps/app/src/Mystira.App.Api/Mystira.App.Api.csproj"

# Copy all source files
COPY apps/app/src/ apps/app/src/
COPY packages/contracts/dotnet/ packages/contracts/dotnet/
COPY packages/shared/ packages/shared/
COPY packages/domain/ packages/domain/
COPY packages/application/ packages/application/

WORKDIR "/src/apps/app/src/Mystira.App.Api"

# Build the application
RUN dotnet build "Mystira.App.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Mystira.App.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:aec87aa74ddf129da573fa69f42f229a23c953a1c6fdecedea1aa6b1fe147d76 AS final
WORKDIR /app

# Install curl for healthcheck (aspnet base image doesn't include it)
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports
# 8080: Main HTTP API endpoint
# 8081: Health check endpoint
EXPOSE 8080
EXPOSE 8081

# Configure environment
ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_HTTP_PORTS="8080"
ENV ASPNETCORE_HEALTHCHECK_PORTS="8081"
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8081/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Mystira.App.Api.dll"]
