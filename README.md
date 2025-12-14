# Mystira Workspace

A unified workspace containing all Mystira platform components as integrated repositories.

## Overview

Mystira is an AI-powered interactive storytelling platform that combines blockchain technology, generative AI, and immersive narratives. This workspace consolidates all major components of the Mystira ecosystem by integrating separate repositories into a unified development environment.

## Projects

Each component is maintained as a separate repository and integrated into this workspace:

| Package                    | Description                                                      | Repository                         | Path                        |
| -------------------------- | ---------------------------------------------------------------- | ---------------------------------- | --------------------------- |
| **Mystira.Chain**          | Blockchain infrastructure, smart contracts, and Web3 integration | `phoenixvc/Mystira.Chain`          | `packages/chain/`           |
| **Mystira.App**            | Main application (being split into modular components)           | `phoenixvc/Mystira.App`            | `packages/app/`             |
| **Mystira.StoryGenerator** | AI-powered story generation engine                               | `phoenixvc/Mystira.StoryGenerator` | `packages/story-generator/` |
| **Mystira.Publisher**      | Publisher web application                                        | `phoenixvc/Mystira.Publisher`      | `packages/publisher/`       |
| **Mystira.DevHub**         | Development operations desktop application                       | `phoenixvc/Mystira.DevHub`         | `packages/devhub/`          |
| **Mystira.Infra**          | Infrastructure, DevOps, and deployment configurations            | `phoenixvc/Mystira.Infra`          | `infra/`                    |

## Repository Structure

This workspace integrates the following repositories as git submodules:

```
Mystira.workspace/
├── packages/
│   ├── chain/              # Mystira.Chain repository (git submodule)
│   │   └── [Mystira.Chain repository contents]
│   │
│   ├── app/                # Mystira.App repository (git submodule)
│   │   ├── [Mystira.App repository contents]
│   │
│   ├── devhub/            # Mystira.DevHub repository (git submodule)
│   │       └── [Mystira.DevHub repository contents]
│   │
│   ├── publisher/          # Mystira.Publisher repository (git submodule)
│   │   └── [Mystira.Publisher repository contents]
│   │
│   └── story-generator/    # Mystira.StoryGenerator repository (git submodule)
│       └── [Mystira.StoryGenerator repository contents]
│
├── infra/                  # Mystira.Infra repository (git submodule)
│   └── [Mystira.Infra repository contents]
│
├── docs/                   # Workspace documentation
├── tools/                  # Shared development tools
├── .github/                # GitHub workflows and templates
├── .husky/                 # Git hooks
├── .changeset/             # Version management
├── .vscode/                # VS Code settings
├── docker-compose.yml      # Local development services
└── SECURITY.md             # Security policy
```

> **Note**: The actual structure of each subdirectory depends on the structure of the respective repositories.

## Getting Started

> **Important**: This workspace integrates separate repositories as git submodules. Make sure to clone with `--recurse-submodules` or initialize submodules after cloning.

### Prerequisites

- Node.js >= 18.x
- pnpm >= 8.x
- Docker (for local development)
- Git (with submodule support)

### Installation

```bash
# Clone the repository with submodules
git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
cd Mystira.workspace

# OR if already cloned, initialize submodules
git submodule update --init --recursive

# OR use the setup script (Linux/Mac)
./scripts/setup-submodules.sh

# OR use the setup script (Windows PowerShell)
./scripts/setup-submodules.ps1

# Install dependencies for all integrated repositories
pnpm install

# Build all packages
pnpm build
```

### Updating Submodules

```bash
# Update all submodules to latest commits
git submodule update --remote

# Update a specific submodule
git submodule update --remote packages/chain
```

> **Note**: For detailed information on working with git submodules, see [docs/SUBMODULES.md](./docs/SUBMODULES.md)

### Development

```bash
# Start all services in development mode
pnpm dev

# Run specific package
pnpm --filter @mystira/chain dev
pnpm --filter @mystira/app dev
pnpm --filter @mystira/story-generator dev

# Run tests
pnpm test

# Lint all packages
pnpm lint
```

## Package Details

### Mystira.Chain

Handles all blockchain-related functionality including:

- Smart contract development and deployment
- NFT minting and management
- Token economics
- Web3 wallet integration

### Mystira.App

The main user-facing application, currently being modularized into:

- **Web**: Next.js-based web application
- **Mobile**: React Native mobile app
- **Shared**: Common components and utilities

### Mystira.StoryGenerator

AI-powered story generation system featuring:

- Dynamic narrative generation
- Character and world-building
- Multi-model AI orchestration
- Context-aware story continuation

### Mystira.Infra

Infrastructure and deployment configurations:

- Cloud infrastructure (Terraform)
- Container orchestration (Kubernetes)
- CI/CD pipelines
- Monitoring and observability

For Azure deployments, see [Azure Setup Guide](./infra/AZURE_SETUP.md) for configuring service principal permissions.

## Documentation

**Workspace Documentation** (`docs/`):

- [Documentation Index](./docs/README.md) - Complete documentation navigation
- [Quick Start Guide](./docs/QUICK_START.md) - Get started in 5 minutes
- [Setup Guide](./docs/SETUP.md) - Detailed setup instructions
- [Submodules Guide](./docs/SUBMODULES.md) - Working with git submodules
- [Architecture](./docs/ARCHITECTURE.md) - System architecture overview
- [Infrastructure Guide](./docs/INFRASTRUCTURE.md) - Infrastructure organization and deployment
- [Implementation Roadmap](./docs/IMPLEMENTATION_ROADMAP.md) - Strategic implementation plan
- [Environment Variables](./docs/ENVIRONMENT.md) - Environment configuration
- [Architecture Decisions](./docs/architecture/adr/) - ADRs documenting key decisions

**Project-Specific Documentation**:

- [App Documentation](./packages/app/README.md) and [App Docs](./packages/app/docs/)
- [Publisher Documentation](./packages/publisher/README.md)
- [Story-Generator Documentation](./packages/story-generator/README.md)
- [Chain Documentation](./packages/chain/README.md)
- [Infrastructure Documentation](./infra/README.md) and [Azure Setup](./infra/AZURE_SETUP.md)

## Contributing

Please read [CONTRIBUTING.md](./CONTRIBUTING.md) for details on our development process and how to submit pull requests.

For information on working with git submodules, see [docs/SUBMODULES.md](./docs/SUBMODULES.md).

## Security

Please review our [Security Policy](./SECURITY.md) for reporting vulnerabilities.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Mystira Platform                         │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────────┐ │
│  │   Web App   │  │ Mobile App  │  │      Admin Dashboard     │ │
│  └──────┬──────┘  └──────┬──────┘  └────────────┬─────────────┘ │
│         │                │                      │               │
│         └────────────────┼──────────────────────┘               │
│                          │                                      │
│                   ┌──────▼──────┐                               │
│                   │   API Layer  │                               │
│                   └──────┬──────┘                               │
│         ┌────────────────┼────────────────┐                     │
│         │                │                │                     │
│  ┌──────▼──────┐  ┌──────▼──────┐  ┌──────▼──────┐             │
│  │Story Engine │  │  Chain/Web3 │  │   Services  │             │
│  │   (AI)      │  │   Layer     │  │   (Auth,etc)│             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                    Infrastructure (Infra)                    ││
│  │   Cloud Services │ Databases │ CDN │ Monitoring │ CI/CD     ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

## License

Proprietary - All rights reserved by Phoenix VC / Mystira

## Contact

- GitHub: [@phoenixvc](https://github.com/phoenixvc)
- Website: [mystira.io](https://mystira.io)
