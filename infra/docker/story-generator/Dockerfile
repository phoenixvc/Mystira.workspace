# Multi-stage Dockerfile for Mystira Story Generator API (.NET 8)
#
# IMPORTANT: This builds the API project (Mystira.StoryGenerator.Api), NOT the
# Blazor WASM Web project (Mystira.StoryGenerator.Web).
#
# Deployment pattern (mirrors Mystira.App):
#   - API (this Dockerfile) → Kubernetes via `story-generator-deploy`
#   - Web (Blazor WASM)     → Static Web App (separate deployment if needed)
#
# See ADR-0019 for full documentation on the deployment architecture.

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj", "Mystira.StoryGenerator.Api/"]

# Restore dependencies
RUN dotnet restore "Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj"

# Copy source code
COPY src/Mystira.StoryGenerator.Api/ Mystira.StoryGenerator.Api/

# Build application
WORKDIR "/src/Mystira.StoryGenerator.Api"
RUN dotnet build "Mystira.StoryGenerator.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Mystira.StoryGenerator.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose ports (matching Kubernetes deployment configuration)
# 8080: Main HTTP API endpoint
# 8081: Health check endpoint
EXPOSE 8080
EXPOSE 8081

# Configure environment
ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_HTTP_PORTS="8080"
ENV ASPNETCORE_HEALTHCHECK_PORTS="8081"
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check (uses dedicated health port)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8081/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Mystira.StoryGenerator.Api.dll"]
