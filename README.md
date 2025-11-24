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

The WebAssembly project reads the API base URL from `wwwroot/appsettings.json`. Update `Api:BaseUrl` to point to the running API instance. If you have a Syncfusion license key, set it under `Syncfusion:LicenseKey` in the same file so the sidebar components run without a trial watermark.

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

## Project Structure

```
Mystira.StoryGenerator/
├── src/                    # Source code
│   ├── Mystira.StoryGenerator.Api/        # ASP.NET Core API
│   ├── Mystira.StoryGenerator.Web/        # Blazor WebAssembly frontend
│   ├── Mystira.StoryGenerator.Contracts/  # Shared contracts
│   └── Mystira.StoryGenerator.Console/    # Console application
├── tests/                  # Unit and integration tests
│   └── Mystira.StoryGenerator.Api.Tests/
├── docs/                   # Documentation
│   ├── AI_PROVIDER_INTEGRATION.md
│   ├── IMPLEMENTATION_SUMMARY.md
│   ├── SCHEMA_VALIDATION_IMPLEMENTATION.md
│   ├── YAML_IMPORT_FEATURE.md
│   └── test-yaml-import.md
├── test-data/             # Test files and scripts
│   ├── test-story.yaml
│   ├── test-story-invalid.yaml
│   └── test-import-feature.sh
├── build.sh               # Build script for bash
└── build.ps1              # Build script for PowerShell
```

## Documentation

Detailed implementation documentation is available in the `docs/` directory:

- **[AI Provider Integration](docs/AI_PROVIDER_INTEGRATION.md)** – Integration with Azure AI Foundry and Google Gemini
- **[Implementation Summary](docs/IMPLEMENTATION_SUMMARY.md)** – Overview of key features
- **[Schema Validation](docs/SCHEMA_VALIDATION_IMPLEMENTATION.md)** – Story YAML validation details
- **[YAML Import Feature](docs/YAML_IMPORT_FEATURE.md)** – Import stories from YAML files

## Scripts

- `build.sh` – restores and builds the solution for bash environments.
- `build.ps1` – restores and builds the solution for PowerShell environments.
- `test-data/test-import-feature.sh` – test script for YAML import functionality.

## Next Steps

The current implementation provides the foundation for future AI integrations, including story generation pipelines, authentication, and richer front-end experiences.
