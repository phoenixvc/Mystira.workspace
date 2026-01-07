# Use the official .NET 9.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and NuGet config
# NOTE: Build context should be the repository root, so paths are relative to /
# Dependencies are pulled from NuGet packages (Mystira.Shared, Mystira.Contracts, etc.)
COPY ["src/Mystira.Admin.Api/Mystira.Admin.Api.csproj", "Mystira.Admin.Api/"]
COPY ["NuGet.config", "."]

# Restore dependencies using BuildKit secret for GitHub Packages authentication
# The secret is mounted at /run/secrets/nuget_auth_token and used inline
RUN --mount=type=secret,id=nuget_auth_token \
    dotnet nuget update source github \
        --username "docker-build" \
        --password "$(cat /run/secrets/nuget_auth_token)" \
        --store-password-in-clear-text \
        --configfile NuGet.config && \
    dotnet restore "Mystira.Admin.Api/Mystira.Admin.Api.csproj"

# Copy all source files from the src directory
COPY src/ .
WORKDIR "/src/Mystira.Admin.Api"

# Build the application
RUN dotnet build "Mystira.Admin.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Mystira.Admin.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy appsettings files explicitly to ensure they're available at runtime
# These should be overridden by environment variables or volume mounts in production
COPY --from=build /src/Mystira.Admin.Api/appsettings.json .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Mystira.Admin.Api.dll"]