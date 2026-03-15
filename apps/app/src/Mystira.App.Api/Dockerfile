# Multi-stage Dockerfile for Mystira App API (.NET 10)
# Build context: monorepo root (Mystira.workspace/)
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:a04d1c1d2d26119049494057d80ea6cda25bbd8aef7c444a1fc1ef874fd3955b AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:478b9038d187e5b5c29bfa8173ded5d29e864b5ad06102a12106380ee01e2e49 AS build
WORKDIR /src

# Copy solution and global.json for SDK version pinning
COPY global.json ./
COPY Mystira.sln ./

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

# Publish the application
FROM build AS publish
RUN dotnet publish "Mystira.App.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Mystira.App.Api.dll"]
