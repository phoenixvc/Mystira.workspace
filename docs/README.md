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

- 📚 [Quick Start Guide](./guides/quick-start.md) - Get started in 5 minutes
- ⚙️ [Setup Guide](./guides/setup.md) - Detailed setup instructions
- 🔧 [Environment Variables](./guides/environment-variables.md) - Environment configuration
- 🔀 [Submodules Guide](./guides/submodules.md) - Working with git submodules

### Architecture & Infrastructure

- 🏗️ [Architecture Overview](./guides/architecture.md) - System architecture overview
- 📋 [Architecture Decision Records](./architecture/adr/) - ADRs documenting key decisions
  - [ADR-0001: Infrastructure Organization](./architecture/adr/0001-infrastructure-organization-hybrid-approach.md)
  - [ADR-0002: Documentation Location Strategy](./architecture/adr/0002-documentation-location-strategy.md)
  - [ADR-0003: Release Pipeline Strategy](./architecture/adr/0003-release-pipeline-strategy.md)
  - [ADR-0004: Branching Strategy and CI/CD](./architecture/adr/0004-branching-strategy-and-cicd.md)
  - [ADR-0005: Service Networking and Communication](./architecture/adr/0005-service-networking-and-communication.md)
  - [ADR-0006: Admin API Repository Extraction](./architecture/adr/0006-admin-api-repository-extraction.md)
  - [ADR-0007: NuGet Feed Strategy](./architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md)
  - [ADR-0008: Azure Resource Naming Conventions](./architecture/adr/0008-azure-resource-naming-conventions.md)
  - [ADR-0009: Further App Segregation Strategy](./architecture/adr/0009-further-app-segregation-strategy.md)
  - [ADR-0010: Authentication and Authorization Strategy](./architecture/adr/0010-authentication-and-authorization-strategy.md)
  - [ADR-0011: Entra ID Authentication Integration](./architecture/adr/0011-entra-id-authentication-integration.md)
  - [ADR-0012: GitHub Workflow Naming Convention](./architecture/adr/0012-github-workflow-naming-convention.md)
  - [ADR-0013: Data Management and Storage Strategy](./architecture/adr/0013-data-management-and-storage-strategy.md)
  - [ADR-0014: Polyglot Persistence Framework Selection](./architecture/adr/0014-polyglot-persistence-framework-selection.md)
  - [ADR-0015: Event-Driven Architecture Framework](./architecture/adr/0015-event-driven-architecture-framework.md)
  - [ADR-0016: Monorepo Tooling and Multi-Repository Strategy](./architecture/adr/0016-monorepo-tooling-and-multi-repository-strategy.md)

### Infrastructure Documentation

- 🌐 [Infrastructure Guide](./infrastructure/infrastructure.md) - Infrastructure organization and deployment
- 🚀 [Quick Start Deploy](./infrastructure/quick-start-deploy.md) - Quick infrastructure deployment
- 🔒 [Kubernetes Secrets Management](./infrastructure/kubernetes-secrets-management.md) - Creating and managing K8s secrets
- 📜 [SSL Certificates Guide](./infrastructure/ssl-certificates-guide.md) - SSL/TLS certificate management
- 🗃️ [Shared Resources](./infrastructure/shared-resources.md) - PostgreSQL, Redis, and Monitoring
- 🛠️ [Terraform Backend Setup](./infrastructure/terraform-backend-setup.md) - Setting up Terraform state backend
- 🏭 [ACR Strategy](./infrastructure/acr-strategy.md) - Azure Container Registry strategy
- 🧩 [Troubleshooting Kubernetes](./infrastructure/troubleshooting-kubernetes-center.md) - K8s troubleshooting guide

### CI/CD & DevOps

- 🔄 [CI/CD Documentation](./cicd/) - CI/CD pipelines and workflows
  - [CI/CD Setup](./cicd/cicd-setup.md) - CI/CD pipelines and branch protection
  - [Branch Protection](./cicd/branch-protection.md) - Branch protection rules
  - [Submodule Access](./cicd/submodule-access.md) - Troubleshooting submodule access in CI/CD
  - [Workflow Permissions](./cicd/workflow-permissions.md) - GitHub workflow permissions
  - [Publishing Flow](./cicd/publishing-flow.md) - Package publishing workflows

**Current Workflows:**
- **Components**: Admin API, Admin UI, App, Chain, Devhub, Publisher, Story Generator
- **Infrastructure**: Deploy, Validate
- **Deployment**: Production, Staging
- **Workspace**: CI, Release
- **Utilities**: Check Submodules

All workflows follow the "Category: Name" naming convention for clarity.

### Repository Organization & Migration

- 🔍 [Repository Extraction Analysis](./analysis/repository-extraction-analysis.md) - Analysis of all repositories and extraction recommendations
- 📊 [App Components Extraction](./analysis/app-components-extraction.md) - Analysis of Admin API/Public API extraction
- 🚚 [Migration Plans](./migration/) - Migration plans for repository extractions
  - [Admin API Extraction Plan](./migration/admin-api-extraction-plan.md) - Detailed migration plan

### Planning & Roadmaps

- 🗺️ [Implementation Roadmap](./planning/implementation-roadmap.md) - Strategic implementation plan
- 📍 [Migration Phases](./migration/phases.md) - Migration phases and status

### Development

- 📝 [Commit Conventions](./guides/commit-conventions.md) - Commit message guidelines
- 🏢 [Setup Status](./setup/setup-status.md) - Repository setup and NuGet implementation status

## Project Documentation

Each project maintains its own documentation within their respective repositories:

| Project             | Location                                                         |
| ------------------- | ---------------------------------------------------------------- |
| **App**             | `packages/app/docs/` and `packages/app/README.md`                |
| **Publisher**       | `packages/publisher/docs/` and `packages/publisher/README.md`    |
| **Story-Generator** | `packages/story-generator/docs/` and `packages/story-generator/README.md` |
| **Chain**           | `packages/chain/README.md`                                       |
| **Admin UI**        | `packages/admin-ui/README.md`                                    |
| **Admin API**       | `packages/admin-api/README.md`                                   |
| **Devhub**          | `packages/devhub/README.md`                                      |
| **Infrastructure**  | `infra/README.md` and `infra/` subdirectories                    |

## Contributing Documentation

When adding new documentation:

1. **Workspace-level?** → Add to `docs/` with appropriate category
   - Cross-project concerns
   - Standards and conventions
   - Architecture decisions

2. **Project-specific?** → Add to `{project}/docs/` or `{project}/README.md`
   - Implementation details
   - Project-specific guides
   - API documentation

3. **Component-specific?** → Add `README.md` near the component code
   - Usage instructions
   - Configuration options
   - Examples

4. **Architecture decision?** → Create ADR in `docs/architecture/adr/`
   - Use template: `docs/architecture/adr/0000-template.md`
   - Number sequentially
   - Follow ADR format

See [ADR-0002](./architecture/adr/0002-documentation-location-strategy.md) for detailed guidance.

## Documentation Standards

### Markdown Guidelines

- Use GitHub-flavored Markdown
- Include table of contents for long documents
- Use relative links for internal documentation
- Add code syntax highlighting with language identifiers

### Structure

```markdown
# Title

Brief description of the document.

## Overview

High-level overview of the topic.

## Sections

### Subsection

Content organized by topics.

## Examples

Practical examples when applicable.

## See Also

- Related documentation links
```

### Code Examples

Always include:
- Working code examples
- Expected output
- Prerequisites or dependencies
- Language identifier for syntax highlighting

## Quick Links

### Frequently Accessed

- [Main README](../README.md) - Workspace overview
- [Contributing Guide](../CONTRIBUTING.md) - How to contribute
- [Security Policy](../SECURITY.md) - Security and vulnerability reporting
- [Infrastructure README](../infra/README.md) - Infrastructure documentation

### Tools & Utilities

- [Repository Metadata Sync](../scripts/sync-repo-metadata.sh) - Sync GitHub repo metadata
- [Scripts Documentation](../scripts/README.md) - Available utility scripts

## Need Help?

- **Issues**: [github.com/phoenixvc/Mystira.workspace/issues](https://github.com/phoenixvc/Mystira.workspace/issues)
- **Discussions**: [github.com/phoenixvc/Mystira.workspace/discussions](https://github.com/phoenixvc/Mystira.workspace/discussions)
- **Documentation**: Start with [Quick Start Guide](./guides/quick-start.md)

## License

Proprietary - All rights reserved by Phoenix VC / Mystira
