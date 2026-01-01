# Mystira Admin API

Admin API backend for the Mystira platform. This is a pure REST/gRPC API for content management and administration.

## ✅ Migration Status

**This repository was extracted from `Mystira.App` as part of the admin tooling separation.**

**Status**: ✅ **COMPLETE** - Admin API extraction is finished and operational.

See [Migration Phases Documentation](../../docs/MIGRATION_PHASES.md) for overall migration status.

The Admin API was separated from the `Mystira.App` monorepo to enable:
- Independent deployment and versioning
- Separate development workflows
- Pure API service without UI dependencies
- Better separation of concerns between admin tools and main application
- Support for modern frontend frameworks (React/Vue/etc) via REST/gRPC

## Overview

This repository contains the Admin API extracted from the `Mystira.App` monorepo. The Admin UI has been separated into the `Mystira.Admin.UI` repository, allowing the frontend to be built with modern web technologies while this API provides a clean REST/gRPC interface.

### Key Responsibilities

- **Content Management**: Scenarios, Characters, Badges, Bundles
- **User Administration**: Read-only access to user accounts and profiles
- **Master Data**: Compass axes, archetypes, echo types, fantasy themes
- **Media Management**: Media assets, audio transcoding

### Migration Status

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1: Extraction | ✅ Complete | Repository created, NuGet packages configured |
| Phase 2: CI/CD | ✅ Complete | GitHub Actions workflows in place |
| Phase 3: PostgreSQL Setup | 🔄 In Progress | Adding dual-write infrastructure |
| Phase 4: Wolverine Events | ⏳ Pending | Event-driven architecture |
| Phase 5: Production Cutover | ⏳ Pending | Full PostgreSQL migration |

## Documentation

Comprehensive documentation is available in the `docs/` directory:

- [**Architecture Decision Records**](docs/architecture/adr/) - Key architectural decisions
- [**Migration Guides**](docs/architecture/migrations/) - Data migration and repository patterns
- [**Planning Documents**](docs/planning/) - Roadmaps and implementation checklists
- [**Operations**](docs/operations/) - Deployment, testing, and rollback procedures

See [docs/README.md](docs/README.md) for the complete documentation index.

## Dependencies

This project depends on NuGet packages published from shared Mystira repositories:

| Package | Version | Purpose |
|---------|---------|---------|
| `Mystira.Domain` | 0.5.0-alpha | Domain models and entities |
| `Mystira.Application` | 0.5.0-alpha | CQRS handlers and use cases |
| `Mystira.Contracts` | 0.5.0-alpha | API contracts and DTOs |
| `Mystira.Infrastructure.Azure` | 1.0.0 | Azure Cosmos DB, Blob Storage |
| `Mystira.Infrastructure.Data` | 1.0.0 | Repository implementations |
| `Mystira.Infrastructure.Discord` | 1.0.0 | Discord bot integration |
| `Mystira.Infrastructure.StoryProtocol` | 1.0.0 | Blockchain/IP integration |
| `Mystira.Shared` | 0.5.0-alpha | Shared services and middleware |

## Setup

### Prerequisites

- .NET 9.0 SDK
- GitHub account - for GitHub Packages access (internal Mystira packages)
- (Optional) Azure Cosmos DB account
- (Optional) Azure Storage account

### Local Development

1. **Configure GitHub Packages authentication**:
   
   Set up a Personal Access Token (PAT) with `read:packages` scope:
   
   **Create a GitHub PAT:**
   - Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate new token with `read:packages` scope
   - Copy the token
   
   **Configure the token:**
   ```bash
   # Set environment variable (required for NuGet restore)
   export GITHUB_TOKEN="your-github-pat-here"  # macOS/Linux
   $env:GITHUB_TOKEN = "your-github-pat-here"  # Windows PowerShell
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Build**:
   ```bash
   dotnet build
   ```

4. **Run** (uses in-memory database by default):
   ```bash
   dotnet run --project src/Mystira.Admin.Api
   ```

5. **Access API**:
   - Swagger UI: https://localhost:7001/
   - Health check: https://localhost:7001/health

### Configuration

Configuration is managed through `appsettings.json` and environment-specific overrides:

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:CosmosDb` | Azure Cosmos DB connection string |
| `ConnectionStrings:PostgreSQL` | PostgreSQL connection string (Phase 2+) |
| `ConnectionStrings:Redis` | Redis cache connection string |
| `DataMigration:Phase` | Current migration phase (0-3) |
| `DataMigration:Enabled` | Enable migration features |
| `AzureAd:TenantId` | Microsoft Entra ID tenant ID |
| `AzureAd:ClientId` | App registration client ID |

### Authentication

The Admin API supports multiple authentication schemes:

1. **Cookie Authentication** - For browser-based admin dashboard sessions
2. **JWT Bearer** - For API clients with custom JWT tokens
3. **Microsoft Entra ID (Azure AD)** - Enterprise SSO via Microsoft.Identity.Web

#### Enabling Entra ID Authentication

To enable Microsoft Entra ID authentication, configure the following settings via User Secrets or Azure Key Vault:

```bash
# Set via User Secrets (local development)
dotnet user-secrets set "AzureAd:TenantId" "your-tenant-id-guid"
dotnet user-secrets set "AzureAd:ClientId" "your-app-client-id-guid"
```

When both `TenantId` and `ClientId` are configured, the API automatically enables Entra ID authentication with the "AzureAd" scheme.

**Exposed API Scopes** (configure in Azure App Registration):
- `Admin.Read` - Read access to admin data
- `Admin.Write` - Write access to admin data
- `Users.Manage` - User management operations
- `Content.Moderate` - Content moderation operations

## API Endpoints

### Content Management

- `GET /api/admin/scenarios` - List scenarios
- `POST /api/admin/scenarios` - Create scenario
- `PUT /api/admin/scenarios/{id}` - Update scenario
- `DELETE /api/admin/scenarios/{id}` - Delete scenario

### User Administration (Read-Only)

- `GET /api/admin/accounts` - List accounts
- `GET /api/admin/accounts/{id}` - Get account details
- `GET /api/admin/profiles` - List user profiles

### Health Checks

- `GET /health` - Full health check (all dependencies)
- `GET /health/ready` - Readiness check (database)
- `GET /health/live` - Liveness check (always 200 if app is running)

## Architecture

### Key Differences from App API

| Aspect | Public API | Admin API |
|--------|------------|-----------|
| User Data | Read/Write | **Read-Only** |
| Content Data | Read-Only | **Read/Write** |
| Migration Phase | Full dual-write | Read-only from PostgreSQL |
| Redis Caching | User sessions | Content caching |
| Security | Public auth | Admin/SuperAdmin roles |

### Data Migration Strategy

The Admin API follows a phased migration approach from Cosmos DB to PostgreSQL:

1. **Phase 0** (Current): Cosmos DB only
2. **Phase 1**: Dual-write with Cosmos read
3. **Phase 2**: Dual-write with PostgreSQL read
4. **Phase 3**: PostgreSQL only

See [docs/planning/hybrid-data-strategy-roadmap.md](docs/planning/hybrid-data-strategy-roadmap.md) for details.

## CI/CD

GitHub Actions workflows are configured for:

- **Build & Test**: On push to `main`/`dev` and PRs
- **Configuration Validation**: Verify JSON config files
- **Documentation Check**: Ensure required docs exist

### GitHub Packages

The project uses GitHub Packages for internal Mystira NuGet packages. The CI workflow automatically authenticates using the `GITHUB_TOKEN` secret that is automatically available in GitHub Actions.

**No additional configuration is needed for CI/CD** - the workflow includes:
- Automatic authentication with `GITHUB_TOKEN`
- `packages: read` permission to access packages
- Configuration of the GitHub Packages feed

For local development, see the setup instructions above.

For more details, see [NuGet Feed Configuration](docs/NUGET_FEED_CONFIGURATION.md).

## CORS Configuration

The API is configured to accept requests from:

- `Mystira.Admin.UI` frontend application
- Local development servers (localhost:7000, 7001)
- Production Azure Static Web Apps

Update CORS settings in `Program.cs` or via `CorsSettings:AllowedOrigins` configuration.

## Architecture

```
┌─────────────────┐
│  Admin UI (SPA) │  ← Mystira.Admin.UI repository
│  (React/Vue/etc)│
└────────┬────────┘
         │ REST/gRPC
         ▼
┌─────────────────┐
│  Admin API      │  ← This repository
│  (ASP.NET Core) │
└────────┬────────┘
         │ NuGet packages
         ▼
┌─────────────────┐
│  Mystira.App    │  ← Source of shared libraries
│  (Domain/Infra) │
└─────────────────┘
```

## Related Repositories

| Repository | Description |
|------------|-------------|
| [Mystira.App](https://github.com/phoenixvc/Mystira.App) | Source of shared NuGet packages |
| [Mystira.Admin.UI](https://github.com/phoenixvc/Mystira.Admin.UI) | Admin frontend application |
| [mystira.workspace](https://github.com/phoenixvc/mystira.workspace) | Mono-repo workspace |

## License

Proprietary - Phoenix VC
