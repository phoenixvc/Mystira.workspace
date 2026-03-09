# Mystira Application Suite

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Blazor PWA](https://img.shields.io/badge/Client-Blazor%20PWA-5C2D91?logo=blazor&logoColor=white)
![Azure Cosmos DB](https://img.shields.io/badge/Azure-Cosmos%20DB-0089D6?logo=microsoftazure&logoColor=white)
![CI](https://img.shields.io/badge/CI-GitHub%20Actions-2088FF?logo=githubactions&logoColor=white)
![Repo Type](https://img.shields.io/badge/Repo-Monorepo-6f42c1?logo=github&logoColor=white)

Mystira is a narrative-driven gaming platform for choice-based storytelling with moral compass tracking. Players engage with branching-narrative scenarios, make moral choices tracked across multiple axes, earn age-appropriate achievements, and build their character profiles. The platform is built on .NET 9 with a Blazor WebAssembly PWA frontend, backed by Azure Cosmos DB and distributed across multiple integration channels including Discord, Teams, and WhatsApp.

## Table of Contents

- [Deployments](#deployments)
- [Repository Structure](#repository-structure)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Running with Docker](#running-with-docker)
- [Testing](#testing)
- [Architecture](#architecture)
- [Documentation](#documentation)
- [Contributing](#contributing)

## Deployments

| Environment | Service     | URL                                                                                                            |
| ----------- | ----------- | -------------------------------------------------------------------------------------------------------------- |
| Production  | PWA         | [mystira.app](https://mystira.app)                                                                             |
| Production  | PWA (Azure) | [blue-water-0eab7991e.3.azurestaticapps.net](https://blue-water-0eab7991e.3.azurestaticapps.net)               |
| Production  | API         | [prod-wus-app-mystira-api.azurewebsites.net](https://prod-wus-app-mystira-api.azurewebsites.net)               |
| Development | PWA         | [mango-water-04fdb1c03.3.azurestaticapps.net](https://mango-water-04fdb1c03.3.azurestaticapps.net)             |
| Development | API         | [dev-san-app-mystira-api.azurewebsites.net/swagger](https://dev-san-app-mystira-api.azurewebsites.net/swagger) |

## Repository Structure

```
src/
  Mystira.App.Domain/                  # Core domain models and business logic
  Mystira.App.Application/             # CQRS commands, queries, and Wolverine handlers
  Mystira.App.Api/                     # ASP.NET Core REST API
  Mystira.App.PWA/                     # Blazor WebAssembly PWA frontend
  Mystira.App.Infrastructure.Data/     # EF Core repositories (Cosmos DB, PostgreSQL)
  Mystira.App.Infrastructure.Azure/    # Azure Blob Storage, email, health checks
  Mystira.App.Infrastructure.Discord/  # Discord bot integration
  Mystira.App.Infrastructure.Teams/    # Microsoft Teams integration
  Mystira.App.Infrastructure.WhatsApp/ # WhatsApp integration
  Mystira.App.Infrastructure.Payments/ # Payment and royalty processing

tests/
  Mystira.App.Api.Tests/               # API controller tests
  Mystira.App.Application.Tests/       # CQRS handler and caching integration tests
  Mystira.App.Domain.Tests/            # Domain model tests
  Mystira.App.Infrastructure.Data.Tests/
  Mystira.App.Infrastructure.Discord.Tests/
  Mystira.App.Infrastructure.Payments.Tests/
  Mystira.App.Infrastructure.Teams.Tests/
  Mystira.App.Infrastructure.WhatsApp.Tests/
  Mystira.App.PWA.Tests/               # Frontend tests

docs/
  architecture/                        # ADRs, CQRS migration guide, caching strategy
  domain/                              # Badge system, domain model documentation
  setup/                               # Environment setup and secrets management guides
```

## Technology Stack

| Category              | Technologies                                                                 |
| --------------------- | ---------------------------------------------------------------------------- |
| Runtime               | .NET 9 (C# 13), SDK 9.0.310                                                  |
| API Framework         | ASP.NET Core 9.0, Swagger/OpenAPI                                            |
| Messaging & CQRS      | [Wolverine](https://wolverine.netlify.app/) v5.13.0 (event-driven messaging) |
| Frontend              | Blazor WebAssembly PWA with offline support, IndexedDB, service workers      |
| Primary Database      | Azure Cosmos DB (EF Core provider)                                           |
| Secondary Database    | PostgreSQL (Npgsql EF Core provider)                                         |
| Object Storage        | Azure Blob Storage                                                           |
| Validation            | FluentValidation                                                             |
| Object Mapping        | Riok.Mapperly (compile-time)                                                 |
| Resilience            | Polly v8                                                                     |
| Logging               | Serilog + Application Insights                                               |
| Query Patterns        | Ardalis Specification pattern, in-memory query caching                       |
| Testing               | xUnit, Moq, FluentAssertions, AutoFixture, Coverlet                          |
| CI/CD                 | GitHub Actions (tests, deployments, security scanning, SLA monitoring)       |
| Dependency Management | Renovate (automated updates)                                                 |

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (`dotnet --list-sdks` should show 9.x)
- [Node.js 18+](https://nodejs.org/) for PWA build tooling and service worker bundling
- Azure resources (Cosmos DB, Blob Storage) or the [Azure Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator)

### Clone and Build

```bash
git clone https://github.com/phoenixvc/Mystira.App.git
cd Mystira.App
dotnet build Mystira.App.sln
```

### Set Up Pre-commit Hooks

The repository uses Husky.Net to run `dotnet format` before each commit:

```bash
dotnet tool restore
dotnet husky install
```

### Configure Secrets

The application requires connection strings and credentials for Azure services. See the setup guides for details:

- [Secrets Management Guide](docs/setup/secrets-management.md) -- local development with User Secrets
- [Quick Secrets Reference](docs/setup/quick-secrets-reference.md) -- cheat sheet
- [GitHub Secrets Configuration](docs/setup/github-secrets-variables.md) -- CI/CD pipeline secrets
- [Database Setup](docs/setup/database-setup.md) -- Cosmos DB and database configuration
- [Email Setup](docs/setup/email-setup.md) -- Azure Communication Services

### Run the Application

```bash
# API
dotnet run --project src/Mystira.App.Api/Mystira.App.Api.csproj

# Blazor PWA
dotnet run --project src/Mystira.App.PWA/Mystira.App.PWA.csproj
```

## Running with Docker

A `docker-compose.yml` is provided for containerized local development:

```bash
# Copy the environment template and fill in your values
cp .env.example .env

# Start the API service
docker compose up -d
```

The API will be available at `http://localhost:5000` with a health check at `/health`. See `.env.example` for all configurable environment variables.

## Testing

```bash
# Run all tests
dotnet test Mystira.App.sln

# Run CQRS integration tests only
dotnet test tests/Mystira.App.Application.Tests/

# Code formatting (also enforced automatically via pre-commit hook)
dotnet format Mystira.App.sln
```

**Test projects cover all layers:**

| Test Project                    | Scope                                      |
| ------------------------------- | ------------------------------------------ |
| `Api.Tests`                     | Controller and endpoint tests              |
| `Application.Tests`             | CQRS handlers, caching, Wolverine pipeline |
| `Domain.Tests`                  | Domain model validation                    |
| `Infrastructure.Data.Tests`     | Repository and EF Core                     |
| `Infrastructure.Discord.Tests`  | Discord bot integration                    |
| `Infrastructure.Payments.Tests` | Payment processing                         |
| `Infrastructure.Teams.Tests`    | Teams integration                          |
| `Infrastructure.WhatsApp.Tests` | WhatsApp integration                       |
| `PWA.Tests`                     | Frontend component tests                   |

CI runs tests, security scanning, and smoke tests automatically via GitHub Actions. See `.github/workflows/` for the full pipeline configuration.

## Architecture

The backend follows **hexagonal architecture** (ports and adapters) with **CQRS** for command/query separation, implemented through **Wolverine** for event-driven messaging.

```
Presentation (API)  -->  Application (Commands/Queries/Handlers)  -->  Domain (Models)
                                                                         ^
Infrastructure (EF Core, Azure Services, Integrations)  ─────────────────┘
```

Key design decisions:

- Zero Application-to-Infrastructure dependencies
- All domain entities use CQRS with dedicated command and query handlers
- In-memory query caching with configurable TTL via the `ICacheableQuery` interface
- Specification pattern for reusable, composable query logic
- Multi-channel integration (Discord, Teams, WhatsApp) via separate infrastructure projects

For detailed architecture documentation, see:

- [Architecture Decision Records](docs/architecture/adr/) (ADR-0001 through ADR-0015)
- [CQRS Migration Guide](docs/architecture/cqrs-migration-guide.md)
- [Caching Strategy](docs/architecture/caching-strategy.md)
- [Architectural Rules](docs/architecture/architectural-rules.md)
- [Chat Bot Integration](docs/architecture/chat-bot-integration.md)
- [Database Architecture Evaluation](docs/architecture/DATABASE_ARCHITECTURE_EVALUATION.md)

## Documentation

| Topic                  | Link                                                                                           |
| ---------------------- | ---------------------------------------------------------------------------------------------- |
| Setup guides           | [docs/setup/](docs/setup/)                                                                     |
| Architecture decisions | [docs/architecture/adr/](docs/architecture/adr/)                                               |
| Badge system           | [docs/domain/badge-system-v2.md](docs/domain/badge-system-v2.md)                               |
| Chat bot setup         | [docs/setup/multi-platform-chat-bot-setup.md](docs/setup/multi-platform-chat-bot-setup.md)     |
| Integration test guide | [tests/Mystira.App.Application.Tests/README.md](tests/Mystira.App.Application.Tests/README.md) |
| PR template            | [.github/PULL_REQUEST_TEMPLATE.md](.github/PULL_REQUEST_TEMPLATE.md)                           |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full contribution guide, including branching strategy, commit conventions, and PR requirements.

Quick overview:

1. Fork the repository and create a feature branch off `main`
2. Follow [Conventional Commits](https://www.conventionalcommits.org/) for commit messages
3. Keep the target framework at `net9.0`
4. Add or update tests for any changes
5. Run `dotnet test Mystira.App.sln` and `dotnet format Mystira.App.sln` before submitting
6. Open a PR describing the motivation, scope, and testing performed
