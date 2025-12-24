# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Mystira.StoryGenerator.sln ./
COPY src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj ./src/Mystira.StoryGenerator.Api/
COPY src/Mystira.StoryGenerator.Application/Mystira.StoryGenerator.Application.csproj ./src/Mystira.StoryGenerator.Application/
COPY src/Mystira.StoryGenerator.Contracts/Mystira.StoryGenerator.Contracts.csproj ./src/Mystira.StoryGenerator.Contracts/
COPY src/Mystira.StoryGenerator.Domain/Mystira.StoryGenerator.Domain.csproj ./src/Mystira.StoryGenerator.Domain/
COPY src/Mystira.StoryGenerator.GraphTheory/Mystira.StoryGenerator.GraphTheory.csproj ./src/Mystira.StoryGenerator.GraphTheory/
COPY src/Mystira.StoryGenerator.Llm/Mystira.StoryGenerator.Llm.csproj ./src/Mystira.StoryGenerator.Llm/

# Restore dependencies
ARG NUGET_AUTH_TOKEN
RUN dotnet restore src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj

# Copy source code
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/Mystira.StoryGenerator.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

COPY --from=build /app/publish .

# Set ownership and switch to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Mystira.StoryGenerator.Api.dll"]
