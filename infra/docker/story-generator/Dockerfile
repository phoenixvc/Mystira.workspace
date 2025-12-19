# Multi-stage Dockerfile for Mystira Story Generator (.NET 8)

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Mystira.StoryGenerator.Web/Mystira.StoryGenerator.Web.csproj", "Mystira.StoryGenerator.Web/"]

# Restore dependencies
RUN dotnet restore "Mystira.StoryGenerator.Web/Mystira.StoryGenerator.Web.csproj"

# Copy source code
COPY src/Mystira.StoryGenerator.Web/ Mystira.StoryGenerator.Web/

# Build application
WORKDIR "/src/Mystira.StoryGenerator.Web"
RUN dotnet build "Mystira.StoryGenerator.Web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Mystira.StoryGenerator.Web.csproj" \
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

# Expose port
EXPOSE 5000

# Configure environment
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Mystira.StoryGenerator.Web.dll"]
