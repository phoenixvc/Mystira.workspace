# Planning Documentation

This directory contains strategic planning documents and roadmaps for the Mystira platform.

## Contents

### Primary Planning Documents

- [Implementation Roadmap](./implementation-roadmap.md) - Strategic implementation plan covering infrastructure, CI/CD, monitoring, security, and developer experience
- [Master Implementation Checklist](./master-implementation-checklist.md) - Detailed checklist for Cosmos DB → PostgreSQL migration and Wolverine event-driven architecture

### ADR-Specific Roadmaps

- [ADR-0014 Implementation Roadmap](./adr-0014-implementation-roadmap.md) - Polyglot persistence framework (Ardalis.Specification)
- [ADR-0015 Implementation Roadmap](./adr-0015-implementation-roadmap.md) - Wolverine event-driven architecture
- [Hybrid Data Strategy Roadmap](./hybrid-data-strategy-roadmap.md) - Cosmos DB → PostgreSQL migration phases

## Current Status (December 2025)

### Completed

- **ADR-0017**: 3-tier resource group organization implemented
- **Azure AI Foundry**: Integrated with gpt-4o and gpt-4o-mini models
- **Entra External ID**: Consumer authentication (replaces B2C)
- **Secrets Management**: All secrets auto-populated by Terraform
- **Entra ID/External ID**: Terraform modules for authentication
- **Workload Identity**: AKS pods authenticate via managed identity

### In Progress

- Cosmos DB → PostgreSQL migration
- Wolverine integration for event-driven architecture

## Related Documentation

- [Architecture Overview](../guides/architecture.md) - System architecture overview
- [Architecture Decision Records](../architecture/adr/) - ADRs documenting key decisions
- [Infrastructure Guide](../infrastructure/infrastructure.md) - Infrastructure setup and management
- [Secrets Management](../infrastructure/secrets-management.md) - Key Vault secrets strategy
- [Entra ID Best Practices](../infrastructure/entra-id-best-practices.md) - Authentication guidance
