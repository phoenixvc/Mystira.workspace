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
- [Implementation Roadmap](./IMPLEMENTATION_ROADMAP.md) - Strategic implementation plan
- [Phase 1 Status](./PHASE1_STATUS.md) - Phase 1 completion status and analysis

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
