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
- [Infrastructure Guide](./INFRASTRUCTURE.md) - Infrastructure organization and deployment
- [Shared Resources Guide](./SHARED_RESOURCES.md) - Using shared PostgreSQL, Redis, and Monitoring
- [Kubernetes Secrets Management](./kubernetes-secrets-management.md) - Creating and managing Kubernetes secrets
- [Architecture Decision Records](./architecture/adr/) - ADRs documenting key decisions
  - [ADR-0005: Service Networking and Communication](./architecture/adr/0005-service-networking-and-communication.md) - Service communication patterns
  - [ADR-0006: Admin API Repository Extraction](./architecture/adr/0006-admin-api-repository-extraction.md) - Admin API extraction decision
  - [ADR-0007: NuGet Feed Strategy for Shared Libraries](./architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md) - Package management strategy
- [Implementation Roadmap](./IMPLEMENTATION_ROADMAP.md) - Strategic implementation plan
- [Setup & Implementation Status](./SETUP_STATUS.md) - Repository setup and NuGet implementation status
- [CI/CD Setup](./CI_CD_SETUP.md) - CI/CD pipelines and branch protection configuration
- [Infrastructure Phase 1](./INFRASTRUCTURE_PHASE1.md) - Infrastructure Phase 1 status and analysis

### Repository Organization

- [Repository Extraction Analysis](./REPOSITORY_EXTRACTION_ANALYSIS.md) - Analysis of all repositories and extraction recommendations
- [App Components Extraction Analysis](./APP_COMPONENTS_EXTRACTION_ANALYSIS.md) - Analysis of Admin API/Public API extraction
- [Migration Plans](./migration/) - Migration plans for repository extractions
  - [Admin API Extraction Plan](./migration/ADMIN_API_EXTRACTION_PLAN.md) - Detailed migration plan

### CI/CD & DevOps

- [Branch Protection & CI/CD Workflow](./BRANCH_PROTECTION.md) - Branch protection rules and deployment workflows

### CI/CD & DevOps

- [Branch Protection & CI/CD Workflow](./BRANCH_PROTECTION.md) - Branch protection rules and deployment workflows

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
