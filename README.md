# Mystira Workspace

A unified monorepo workspace containing all Mystira platform components.

## Overview

Mystira is an AI-powered interactive storytelling platform that combines blockchain technology, generative AI, and immersive narratives. This workspace consolidates all components of the Mystira ecosystem into a unified development environment.

## Quick Start

```bash
# Clone the repository
git clone https://github.com/phoenixvc/Mystira.workspace.git
cd Mystira.workspace

# Install dependencies
pnpm install

# Build all packages
pnpm build

# Start development servers
pnpm dev
```

For detailed setup instructions, see [Quick Start Guide](./docs/guides/quick-start.md).

## Components

All components live in the `packages/` directory:

| Component                  | Path                        | Description                             | Tech Stack        |
| -------------------------- | --------------------------- | --------------------------------------- | ----------------- |
| **Mystira.Chain**          | `packages/chain/`           | Blockchain integration & Story Protocol | Python, gRPC      |
| **Mystira.App**            | `packages/app/`             | Main storytelling application           | C#, .NET          |
| **Mystira.StoryGenerator** | `packages/story-generator/` | AI-powered story generation engine      | C#, .NET          |
| **Mystira.Publisher**      | `packages/publisher/`       | Content publishing service              | TypeScript, Node  |
| **Mystira.DevHub**         | `packages/devhub/`          | Developer portal and tools              | TypeScript        |
| **Mystira.Admin.Api**      | `packages/admin-api/`       | Admin backend API                       | C#, ASP.NET Core  |
| **Mystira.Admin.UI**       | `packages/admin-ui/`        | Admin dashboard frontend                | TypeScript, React |
| **Infrastructure**         | `infra/`                    | Terraform, Kubernetes, CI/CD            | HCL, YAML         |

## Repository Structure

```
Mystira.workspace/
├── packages/               # Application packages
│   ├── admin-api/         # Admin backend (C# API)
│   ├── admin-ui/          # Admin frontend (React)
│   ├── app/               # Main application (C#)
│   ├── chain/             # Blockchain service (Python)
│   ├── devhub/            # Developer portal (TypeScript)
│   ├── publisher/         # Publishing service (TypeScript)
│   └── story-generator/   # AI story engine (C#)
│
├── infra/                 # Infrastructure as Code
│   ├── terraform/         # Terraform modules & environments
│   ├── kubernetes/        # Kubernetes manifests
│   ├── docker/            # Dockerfiles for all services
│   └── scripts/           # Deployment scripts
│
├── .github/workflows/     # CI/CD pipelines
│   ├── *-ci.yml          # Component CI workflows
│   ├── infra-*.yml       # Infrastructure workflows
│   └── deployment-*.yml  # Deployment workflows
│
├── docs/                  # Workspace documentation
│   ├── architecture/      # Architecture docs & ADRs
│   ├── cicd/             # CI/CD documentation
│   ├── infrastructure/    # Infrastructure guides
│   └── planning/         # Planning & roadmaps
│
└── scripts/              # Utility scripts
    ├── sync-repo-metadata.sh  # Sync GitHub repo metadata
    └── repo-metadata.json     # Repository metadata config
```

## Development

### Prerequisites

- **Node.js** >= 18.x with **pnpm** >= 8.x (TypeScript components)
- **.NET SDK** 10.0+ (C# components)
- **Python** 3.11+ (Chain component)
- **Docker** (local development)
- **Azure AD Tenant** (for Entra authentication)
- **Email Service** (SendGrid/SMTP for Magic Link authentication)

> **Note**: For .NET development, you'll need to configure GitHub Packages authentication. See [Setup Guide](./docs/guides/setup.md#nuget-packages-github-packages) for NuGet configuration including Package Source Mapping.

### Authentication Setup

The Mystira platform uses unified dual-path authentication (Entra + Magic Link). For complete setup instructions:

📖 **[Entra-Magic Auth Setup Guide](./docs/ENTRA_MAGIC_AUTH_SETUP.md)**
🔖 **[Quick Reference](./docs/ENTRA_MAGIC_AUTH_QUICK_REFERENCE.md)**

Key requirements:

- Azure AD app registration for Entra SSO
- Email service configuration for Magic Link
- JWT token configuration across all applications

### Common Tasks

```bash
# Install all dependencies
pnpm install

# Run linting
pnpm lint

# Run tests
pnpm test

# Build all packages
pnpm build

# Build specific package
pnpm --filter @mystira/publisher build

# Start development mode
pnpm dev

# Start specific service
pnpm --filter mystira-publisher dev
```

### Debugging

The workspace includes VS Code debug configurations for TypeScript/Node.js services. Template environment files are provided:

- `.env.admin-api` - Admin API environment variables
- `.env.publisher` - Publisher service environment variables
- `.env.story-generator` - Story Generator service environment variables

Before debugging, update these files with your local configuration values. The debug configurations will automatically load these environment files.

## CI/CD Pipeline

The workspace uses a distributed CI/CD model:

### Component CI

Each component has a CI workflow triggered by path-based filters:

| Component       | Workflow                 | Runtime     |
| --------------- | ------------------------ | ----------- |
| Admin API       | `admin-api-ci.yml`       | .NET 10.0   |
| Admin UI        | `admin-ui-ci.yml`        | Node.js 24  |
| App             | `app-ci.yml`             | .NET 10.0   |
| Chain           | `chain-ci.yml`           | Python 3.11 |
| DevHub          | `devhub-ci.yml`          | Node.js 24  |
| Publisher       | `publisher-ci.yml`       | Node.js 24  |
| Story Generator | `story-generator-ci.yml` | .NET 10.0   |

### Integration, Staging & Production

This workspace handles integration, staging, and production deployments:

### Infrastructure Workflows

- **Infrastructure: Deploy** - Automated infrastructure deployment
- **Infrastructure: Validate** - Infrastructure validation

### Deployment Workflows

- **Deployment: Staging** - Auto-deploy to staging on main branch
- **Deployment: Production** - Manual production deployment

### Workspace Workflows

- **Workspace: CI** - Workspace-level CI
- **Workspace: Release** - NPM package releases via Changesets

### Utilities

- **Utilities: Link Checker** - Check markdown links in documentation

All workflows run on push to `dev`/`main` and on pull requests.

## Infrastructure

The infrastructure is defined using:

- **Terraform** - Cloud resources (Azure AKS, networking, databases)
- **Kubernetes** - Container orchestration with Kustomize overlays
- **Docker** - Containerized microservices
- **Azure Front Door** - CDN, WAF, and DDoS protection

### Environments

| Environment | URL                     | Deployment          | Branch |
| ----------- | ----------------------- | ------------------- | ------ |
| Development | `*.dev.mystira.app`     | Manual              | `dev`  |
| Staging     | `*.staging.mystira.app` | Auto (on main push) | `main` |
| Production  | `*.mystira.app`         | Manual (protected)  | `main` |

### Quick Deploy

```bash
# Deploy to dev environment
gh workflow run "Infrastructure: Deploy" \
  --field environment=dev \
  --field components=all

# Deploy to staging (automatic on main branch push)
git push origin main

# Deploy to production (requires manual approval)
gh workflow run "Deployment: Production" \
  --field confirm="DEPLOY TO PRODUCTION"
```

See [Infrastructure Documentation](./infra/README.md) for detailed guides.

## Documentation

### Getting Started

- [Quick Start Guide](./docs/guides/quick-start.md) - Get running in 5 minutes
- [Setup Guide](./docs/guides/setup.md) - Detailed setup instructions

### Development

- [Architecture Overview](./docs/guides/architecture.md) - System architecture
- [Architecture Decision Records](./docs/architecture/adr/) - Key design decisions
- [Commit Conventions](./docs/guides/commit-conventions.md) - Commit message guidelines
- [Contributing Guide](./CONTRIBUTING.md) - How to contribute

### Infrastructure & DevOps

- [Infrastructure Guide](./docs/infrastructure/infrastructure.md) - Infrastructure overview
- [CI/CD Documentation](./docs/cicd/) - CI/CD setup and workflows
- [Azure Setup Guide](./infra/azure-setup.md) - Azure configuration
- [Kubernetes Secrets](./docs/infrastructure/kubernetes-secrets-management.md)

### Planning & Migration

- [Implementation Roadmap](./docs/planning/implementation-roadmap.md)
- [Repository Analysis](./docs/analysis/)
- [Migration Plans](./docs/migration/)

## Utilities

### Repository Metadata Sync

Sync repository descriptions, topics, and settings across all Mystira repos:

```bash
# Preview changes
./scripts/sync-repo-metadata.sh --dry-run

# Apply changes
./scripts/sync-repo-metadata.sh
```

Configuration: [`scripts/repo-metadata.json`](./scripts/repo-metadata.json)

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              Mystira Platform                                    │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│                            ┌─────────────────────┐                              │
│                            │   Azure Front Door  │                              │
│                            │   (CDN + WAF + TLS) │                              │
│                            └──────────┬──────────┘                              │
│                                       │                                         │
│  ┌────────────────────────────────────┼────────────────────────────────────┐    │
│  │                                    │                                    │    │
│  ▼                                    ▼                                    ▼    │
│ ┌───────────────────────────────┐  ┌─────────────┐  ┌───────────────────────┐  │
│ │      Mystira.App (Client)     │  │ Admin UI    │  │     DevHub            │  │
│ │  ┌───────────────────────────┐│  │  (React)    │  │  (Tauri Desktop)      │  │
│ │  │  Blazor WebAssembly PWA   ││  │             │  │  Developer Portal     │  │
│ │  │  • Web • Mobile • Offline ││  │ admin.      │  │                       │  │
│ │  │  • IndexedDB • Haptics    ││  │ mystira.app │  │                       │  │
│ │  └────────────┬──────────────┘│  └──────┬──────┘  └───────────┬───────────┘  │
│ │               │               │         │                     │              │
│ └───────────────┼───────────────┘         │                     │              │
│                 │                         │                     │              │
│                 └─────────────────────────┴─────────────────────┘              │
│                                    │                                            │
│                                    ▼                                            │
│  ┌──────────────────────────────────────────────────────────────────────────┐  │
│  │                         Kubernetes (AKS)                                  │  │
│  │  ┌────────────┐  ┌────────────┐  ┌─────────────┐  ┌───────────┐          │  │
│  │  │ Publisher  │  │ Admin API  │  │   Story     │  │   Chain   │          │  │
│  │  │(TypeScript)│  │(C# .NET 10) │  │  Generator  │  │  (Python) │          │  │
│  │  │            │  │            │  │ (C# + AI)   │  │           │          │  │
│  │  │ • Content  │  │ • Entra ID │  │ • Claude    │  │ • Story   │          │  │
│  │  │ • Publish  │  │ • CRUD     │  │ • GPT-4     │  │   Protocol│          │  │
│  │  │ • NFT Mint │  │ • Analytics│  │ • Context   │  │ • Web3    │          │  │
│  │  └─────┬──────┘  └─────┬──────┘  └──────┬──────┘  └─────┬─────┘          │  │
│  │        │               │                │               │                │  │
│  │        └───────────────┴────────────────┴───────────────┘                │  │
│  │                                │                                         │  │
│  └────────────────────────────────┼─────────────────────────────────────────┘  │
│                                   │                                            │
│  ┌────────────────────────────────┴─────────────────────────────────────────┐  │
│  │                        Shared Infrastructure                              │  │
│  │  ┌────────────┐  ┌─────────┐  ┌───────────┐  ┌─────────────────────────┐ │  │
│  │  │ PostgreSQL │  │  Redis  │  │ Key Vault │  │    Azure Monitor        │ │  │
│  │  │ (Flexible) │  │ (Cache) │  │ (Secrets) │  │ (App Insights + Logs)   │ │  │
│  │  └────────────┘  └─────────┘  └───────────┘  └─────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────────────────────┘  │
│                                                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────┐  │
│  │                          Security & Auth                                  │  │
│  │  ┌─────────────────┐  ┌───────────────────┐  ┌─────────────────────────┐ │  │
│  │  │ Microsoft Entra │  │ Entra External ID │  │   Managed Identity      │ │  │
│  │  │  ID (Admins)    │  │ (Consumer Users)  │  │  (Service-to-Service)   │ │  │
│  │  │                 │  │  + Magic Link    │  │                         │ │  │
│  │  │  • OAuth 2.0    │  │  • OAuth 2.0      │  │  • Azure AD Auth       │ │  │
│  │  │  • JWT Tokens   │  │  • Magic Links    │  │  • Service Principals  │ │  │
│  │  │  • SSO          │  │  • JWT Tokens     │  │                         │ │  │
│  │  │                 │  │  • Email Service  │  │                         │ │  │
│  │  └─────────┬───────┘  └─────────┬─────────┘  └─────────────────────────┘ │  │
│  │            │                    │                                            │  │
│  │            └──────────┬─────────┘                                            │  │
│  │                       │ JWT Token Flow                                       │  │
│  │                       ▼                                                      │  │
│  │              ┌─────────────────┐                                            │  │
│  │              │  Identity API   │                                            │  │
│  │              │  (Token Issuer  │                                            │  │
│  │              │   & Validator)   │                                            │  │
│  │              │                 │                                            │  │
│  │              │ • JWT issuance  │                                            │  │
│  │              │ • Token validation│                                           │  │
│  │              │ • Magic link auth│                                           │  │
│  │              │ • Entra ID integration│                                      │  │
│  │              │ • Centralized auth authority│                               │  │
│  │              └─────────────────┘                                            │  │
│  └──────────────────────────────────────────────────────────────────────────┘  │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                           Package & Container Registries                         │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ┌─────────────────────────┐  ┌─────────────────────────┐  ┌─────────────────┐ │
│  │    Docker Images        │  │     NuGet Packages      │  │   NPM Packages  │ │
│  │  (Azure Container Reg.) │  │   (GitHub Packages)     │  │   (npmjs.org)   │ │
│  │                         │  │                         │  │                 │ │
│  │  myssharedacr.azurecr.io│  │  nuget.pkg.github.com/  │  │  @mystira/*     │ │
│  │                         │  │  phoenixvc/index.json   │  │                 │ │
│  │  • publisher:latest     │  │                         │  │  • publisher    │ │
│  │  • chain:latest         │  │  • Mystira.App.Domain   │  │  • shared-utils │ │
│  │  • story-generator      │  │  • Mystira.App.Shared   │  │  • ui-components│ │
│  │  • admin-api:latest     │  │  • Mystira.App.Contracts│  │                 │ │
│  │  • admin-ui:latest      │  │  • Mystira.StoryGen.*   │  │                 │ │
│  └─────────────────────────┘  └─────────────────────────┘  └─────────────────┘ │
│                                                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────┐  │
│  │                      Infrastructure as Code                               │  │
│  │   Terraform (Azure) │ Kubernetes (Kustomize) │ GitHub Actions (CI/CD)    │  │
│  └──────────────────────────────────────────────────────────────────────────┘  │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Mystira.App

The main client application ([Mystira.App](https://github.com/phoenixvc/Mystira.App)) is built with:

- **Blazor WebAssembly** - C# frontend running in the browser via WebAssembly
- **Progressive Web App (PWA)** - Offline-capable with service workers
- **IndexedDB** - Client-side data persistence and sync
- **Haptics & Audio** - Native device feature integration via JS interop

**Backend**: ASP.NET Core on .NET 10 with Cosmos DB and hexagonal architecture (CQRS + MediatR)

### Package Ecosystem

| Registry                     | Packages                                                                                        | Purpose                                       |
| ---------------------------- | ----------------------------------------------------------------------------------------------- | --------------------------------------------- |
| **Azure Container Registry** | `publisher`, `chain`, `story-generator`, `admin-api`, `admin-ui`                                | Docker images for Kubernetes deployment       |
| **GitHub Packages (NuGet)**  | `Mystira.App.Domain`, `Mystira.App.Shared`, `Mystira.App.Contracts`, `Mystira.StoryGenerator.*` | Shared .NET libraries for service development |
| **npmjs.org**                | `@mystira/publisher`, `@mystira/shared-utils`                                                   | TypeScript packages via Changesets            |

### Service Endpoints

| Environment | Service         | URL                           |
| ----------- | --------------- | ----------------------------- |
| Development | Web App         | dev.mystira.app               |
|             | Admin UI        | dev.admin.mystira.app         |
|             | Admin API       | dev.admin-api.mystira.app     |
|             | Identity API    | dev.identity.mystira.app      |
|             | Publisher       | dev.publisher.mystira.app     |
|             | Story Generator | dev.story-api.mystira.app     |
|             | Story Web       | dev.story.mystira.app         |
|             | Chain           | dev.chain.mystira.app         |
| Staging     | Web App         | staging.mystira.app           |
|             | Admin UI        | staging.admin.mystira.app     |
|             | Admin API       | staging.admin-api.mystira.app |
|             | Identity API    | staging.identity.mystira.app  |
|             | Publisher       | staging.publisher.mystira.app |
|             | Story Generator | staging.story-api.mystira.app |
|             | Story Web       | staging.story.mystira.app     |
|             | Chain           | staging.chain.mystira.app     |
| Production  | Web App         | mystira.app                   |
|             | Admin UI        | admin.mystira.app             |
|             | Admin API       | admin-api.mystira.app         |
|             | Identity API    | identity.mystira.app          |
|             | Publisher       | publisher.mystira.app         |
|             | Story Generator | story-api.mystira.app         |
|             | Story Web       | story.mystira.app             |
|             | Chain           | chain.mystira.app             |

## Security

- Review our [Security Policy](./SECURITY.md) for reporting vulnerabilities
- All secrets managed via Azure Key Vault and Kubernetes secrets
- Branch protection enforced on `main` and `dev` branches
- Required CI checks before merging

## Contributing

We welcome contributions! Please read:

1. [Contributing Guide](./CONTRIBUTING.md) - Development process
2. [Commit Conventions](./docs/guides/commit-conventions.md) - Commit message format

## License

Proprietary - All rights reserved by Phoenix VC / Mystira

## Contact

- **GitHub**: [@phoenixvc](https://github.com/phoenixvc)
- **Website**: [mystira.app](https://mystira.app)
- **Issues**: [github.com/phoenixvc/Mystira.workspace/issues](https://github.com/phoenixvc/Mystira.workspace/issues)
