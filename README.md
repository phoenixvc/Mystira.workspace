# Mystira Story Generator

Mystira Story Generator is a lightweight companion solution that explores AI-assisted storytelling for Mystira experiences. The solution contains:

- **Mystira.StoryGenerator.Api** – ASP.NET Core minimal API that will host story generation endpoints.
- **Mystira.StoryGenerator.Web** – Blazor WebAssembly front-end that consumes the API.
- **Mystira.StoryGenerator.Contracts** – Shared contracts for request/response DTOs and configuration models.

## Getting Started

### Prerequisites

- .NET 8 SDK or later

### Restore & Build

Use the provided build script or run the following commands from the repository root:

```bash
dotnet restore Mystira.StoryGenerator.sln
dotnet build Mystira.StoryGenerator.sln
```

### Configuration

The API project reads AI configuration from `appsettings.json` under the `Ai` section. Replace the placeholder API key and model configuration with your provider details. CORS allowed origins can be configured in the `Cors:AllowedOrigins` array.

The WebAssembly project reads the API base URL from `wwwroot/appsettings.json`. Update `Api:BaseUrl` to point to the running API instance.

### Running the Projects

1. Start the API:

   ```bash
   dotnet run --project src/Mystira.StoryGenerator.Api
   ```

2. Start the Blazor WebAssembly project:

   ```bash
   dotnet run --project src/Mystira.StoryGenerator.Web
   ```

The web application will use the configured HttpClient base address to communicate with the API.

## Endpoints

- `GET /ping` – Simple health ping endpoint returning `{ "status": "ok" }`.
- `GET /health` – ASP.NET Core health check endpoint.
- `POST /stories/preview` – Placeholder endpoint that echoes the request using shared contracts.

## Scripts

- `build.sh` – restores and builds the solution for bash environments.
- `build.ps1` – restores and builds the solution for PowerShell environments.

## Next Steps

The current implementation provides the foundation for future AI integrations, including story generation pipelines, authentication, and richer front-end experiences.
