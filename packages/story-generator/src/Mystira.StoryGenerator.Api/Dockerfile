# Multi-stage Dockerfile for Mystira Story Generator API (.NET 10)
# Build context: monorepo root (Mystira.workspace/)

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:e362a8dbcd691522456da26a5198b8f3ca1d7641c95624fadc5e3e82678bd08a AS build
WORKDIR /src

# Copy solution and global.json for SDK version pinning
COPY global.json ./
COPY Mystira.sln ./

# Copy csproj files for restore (story-generator + all project-referenced packages)
COPY packages/story-generator/src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj packages/story-generator/src/Mystira.StoryGenerator.Api/
COPY packages/story-generator/src/Mystira.StoryGenerator.Application/Mystira.StoryGenerator.Application.csproj packages/story-generator/src/Mystira.StoryGenerator.Application/
COPY packages/story-generator/src/Mystira.StoryGenerator.Contracts/Mystira.StoryGenerator.Contracts.csproj packages/story-generator/src/Mystira.StoryGenerator.Contracts/
COPY packages/story-generator/src/Mystira.StoryGenerator.Domain/Mystira.StoryGenerator.Domain.csproj packages/story-generator/src/Mystira.StoryGenerator.Domain/
COPY packages/story-generator/src/Mystira.StoryGenerator.GraphTheory/Mystira.StoryGenerator.GraphTheory.csproj packages/story-generator/src/Mystira.StoryGenerator.GraphTheory/
COPY packages/story-generator/src/Mystira.StoryGenerator.Llm/Mystira.StoryGenerator.Llm.csproj packages/story-generator/src/Mystira.StoryGenerator.Llm/
COPY packages/story-generator/src/Mystira.StoryGenerator.Infrastructure/Mystira.StoryGenerator.Infrastructure.csproj packages/story-generator/src/Mystira.StoryGenerator.Infrastructure/
COPY packages/contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj packages/contracts/dotnet/Mystira.Contracts/
COPY packages/shared/Mystira.Shared/Mystira.Shared.csproj packages/shared/Mystira.Shared/
COPY packages/core/Mystira.Core/Mystira.Core.csproj packages/core/Mystira.Core/
COPY packages/domain/Mystira.Domain/Mystira.Domain.csproj packages/domain/Mystira.Domain/

# Restore dependencies (no NuGet auth needed — internal packages are ProjectReferences)
RUN dotnet restore "packages/story-generator/src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj"

# Copy source code
COPY packages/story-generator/src/ packages/story-generator/src/
COPY packages/contracts/dotnet/ packages/contracts/dotnet/
COPY packages/shared/ packages/shared/
COPY packages/core/ packages/core/
COPY packages/domain/ packages/domain/

# Build and publish
WORKDIR "/src/packages/story-generator/src/Mystira.StoryGenerator.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:aec87aa74ddf129da573fa69f42f229a23c953a1c6fdecedea1aa6b1fe147d76 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

COPY --from=build /app/publish .

# Set ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Mystira.StoryGenerator.Api.dll"]
