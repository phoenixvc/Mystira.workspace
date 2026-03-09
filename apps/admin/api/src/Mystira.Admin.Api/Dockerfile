# Multi-stage Dockerfile for Mystira Admin API (.NET 10)
# Build context: monorepo root (Mystira.workspace/)
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:aec87aa74ddf129da573fa69f42f229a23c953a1c6fdecedea1aa6b1fe147d76 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e362a8dbcd691522456da26a5198b8f3ca1d7641c95624fadc5e3e82678bd08a AS build
WORKDIR /src

# Copy solution and global.json for SDK version pinning
COPY global.json ./
COPY Mystira.sln ./

# Copy csproj files for restore (admin-api + all project-referenced packages)
COPY apps/admin/api/src/Mystira.Admin.Api/Mystira.Admin.Api.csproj apps/admin/api/src/Mystira.Admin.Api/
COPY packages/domain/Mystira.Domain/Mystira.Domain.csproj packages/domain/Mystira.Domain/
COPY packages/application/Mystira.Application/Mystira.Application.csproj packages/application/Mystira.Application/
COPY packages/contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj packages/contracts/dotnet/Mystira.Contracts/
COPY packages/infrastructure/Mystira.Infrastructure.Azure/Mystira.Infrastructure.Azure.csproj packages/infrastructure/Mystira.Infrastructure.Azure/
COPY packages/infrastructure/Mystira.Infrastructure.Data/Mystira.Infrastructure.Data.csproj packages/infrastructure/Mystira.Infrastructure.Data/
COPY packages/infrastructure/Mystira.Infrastructure.Discord/Mystira.Infrastructure.Discord.csproj packages/infrastructure/Mystira.Infrastructure.Discord/
COPY packages/infrastructure/Mystira.Infrastructure.StoryProtocol/Mystira.Infrastructure.StoryProtocol.csproj packages/infrastructure/Mystira.Infrastructure.StoryProtocol/
COPY packages/shared/Mystira.Shared/Mystira.Shared.csproj packages/shared/Mystira.Shared/
COPY packages/core/Mystira.Core/Mystira.Core.csproj packages/core/Mystira.Core/

# Restore dependencies (no NuGet auth needed — internal packages are ProjectReferences)
RUN dotnet restore "apps/admin/api/src/Mystira.Admin.Api/Mystira.Admin.Api.csproj"

# Copy all source files
COPY apps/admin/api/src/ apps/admin/api/src/
COPY packages/domain/ packages/domain/
COPY packages/application/ packages/application/
COPY packages/contracts/dotnet/ packages/contracts/dotnet/
COPY packages/infrastructure/Mystira.Infrastructure.Azure/ packages/infrastructure/Mystira.Infrastructure.Azure/
COPY packages/infrastructure/Mystira.Infrastructure.Data/ packages/infrastructure/Mystira.Infrastructure.Data/
COPY packages/infrastructure/Mystira.Infrastructure.Discord/ packages/infrastructure/Mystira.Infrastructure.Discord/
COPY packages/infrastructure/Mystira.Infrastructure.StoryProtocol/ packages/infrastructure/Mystira.Infrastructure.StoryProtocol/
COPY packages/shared/ packages/shared/
COPY packages/core/ packages/core/

WORKDIR "/src/apps/admin/api/src/Mystira.Admin.Api"

# Build the application
RUN dotnet build "Mystira.Admin.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Mystira.Admin.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Mystira.Admin.Api.dll"]
