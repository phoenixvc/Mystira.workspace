# Mystira Workspace Documentation

Welcome to the Mystira workspace documentation. This directory contains workspace-level documentation that applies across multiple projects.

## Documentation Organization

This workspace uses a **hybrid documentation strategy** with clear boundaries:

- **Workspace-level** (`docs/`): Cross-project documentation, coordination, and standards
- **Project-level** (`{project}/docs/`): Project-specific documentation
- **Component-level** (`{component}/README.md`): Component-specific documentation

For details, see [ADR-0002: Documentation Location Strategy](./architecture/adr/0002-documentation-location-strategy.md).

## Quick Navigation

### Getting Started

- [Quick Start Guide](./QUICK_START.md) - Get started in 5 minutes
- [Setup Guide](./SETUP.md) - Detailed setup instructions
- [Environment Variables](./ENVIRONMENT.md) - Environment configuration

### Architecture & Infrastructure

- [Architecture Overview](./ARCHITECTURE.md) - System architecture overview
- [Infrastructure Documentation](./infrastructure/) - Infrastructure guides and resources
  - [Infrastructure Guide](./infrastructure/infrastructure.md) - Infrastructure organization and deployment
  - [Infrastructure Phase 1](./infrastructure/infrastructure-phase1.md) - Infrastructure Phase 1 status and analysis
  - [Shared Resources](./infrastructure/shared-resources.md) - Using shared PostgreSQL, Redis, and Monitoring
  - [Kubernetes Secrets Management](./infrastructure/kubernetes-secrets-management.md) - Creating and managing Kubernetes secrets
- [Architecture Decision Records](./architecture/adr/) - ADRs documenting key decisions
  - [ADR-0005: Service Networking and Communication](./architecture/adr/0005-service-networking-and-communication.md) - Service communication patterns
  - [ADR-0006: Admin API Repository Extraction](./architecture/adr/0006-admin-api-repository-extraction.md) - Admin API extraction decision
  - [ADR-0007: NuGet Feed Strategy for Shared Libraries](./architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md) - Package management strategy
- [Planning Documentation](./planning/) - Strategic planning documents
  - [Implementation Roadmap](./planning/implementation-roadmap.md) - Strategic implementation plan
- [Setup Documentation](./setup/) - Setup and configuration
  - [Setup Status](./setup/setup-status.md) - Repository setup and NuGet implementation status

### Repository Organization & Migration

- [Analysis Documents](./analysis/) - Repository structure analysis
  - [Repository Extraction Analysis](./analysis/repository-extraction-analysis.md) - Analysis of all repositories and extraction recommendations
  - [App Components Extraction](./analysis/app-components-extraction.md) - Analysis of Admin API/Public API extraction
- [Migration Plans](./migration/) - Migration plans for repository extractions
  - [Admin API Extraction Plan](./migration/admin-api-extraction-plan.md) - Detailed migration plan

### CI/CD & DevOps

- [CI/CD Documentation](./cicd/) - CI/CD pipelines and workflows
  - [CI/CD Setup](./cicd/cicd-setup.md) - CI/CD pipelines and branch protection configuration
  - [Branch Protection](./cicd/branch-protection.md) - Branch protection rules for `dev` and `main`
  - [Submodule Access](./cicd/submodule-access.md) - Troubleshooting guide for submodule access in CI/CD
  - [Workflow Permissions](./cicd/workflow-permissions.md) - GitHub workflow permissions explained

### Development

- [Submodules Guide](./SUBMODULES.md) - Working with git submodules
- [Commit Conventions](./COMMITS.md) - Commit message guidelines

## Project Documentation

Each project maintains its own documentation:

- **App**: `packages/app/docs/` and `packages/app/README.md`
- **Publisher**: `packages/publisher/docs/` and `packages/publisher/README.md`
- **Story-Generator**: `packages/story-generator/docs/` and `packages/story-generator/README.md`
- **Chain**: `packages/chain/README.md`
- **Infrastructure**: `infra/README.md`

## Contributing Documentation

When adding new documentation:

1. **Workspace-level?** → Add to `docs/` with appropriate category
2. **Project-specific?** → Add to `{project}/docs/` or `{project}/README.md`
3. **Component-specific?** → Add `README.md` near the component code
4. **Architecture decision?** → Create ADR in `docs/architecture/adr/`

See [ADR-0002](./architecture/adr/0002-documentation-location-strategy.md) for detailed guidance.
