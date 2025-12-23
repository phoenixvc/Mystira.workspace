# Infrastructure Documentation

This directory contains documentation for infrastructure, deployment, and shared resources.

## Contents

- [Infrastructure Guide](./infrastructure.md) - Infrastructure organization and deployment
- [Infrastructure Phase 1](./infrastructure-phase1.md) - Infrastructure Phase 1 status and analysis
- [Shared Resources](./shared-resources.md) - Using shared PostgreSQL, Redis, Service Bus, and Monitoring
- [Secrets Management](./secrets-management.md) - Key Vault secrets (auto-populated vs manual)
- [Kubernetes Secrets Management](./kubernetes-secrets-management.md) - Creating and managing Kubernetes secrets

## Resource Group Organization

Per [ADR-0017](../architecture/adr/0017-resource-group-organization-strategy.md), Mystira uses a 3-tier resource group strategy:

| Tier | Resource Groups | Purpose |
|------|-----------------|---------|
| **Tier 1: Core** | `mys-{env}-core-rg-san` | Shared infrastructure (VNet, AKS, PostgreSQL, Redis, Service Bus) |
| **Tier 2: Services** | `mys-{env}-{service}-rg-san` | Service-specific resources (chain, publisher, story, admin, app) |
| **Tier 3: Shared** | `mys-shared-{purpose}-rg-*` | Cross-environment (ACR, Communications, Terraform state) |

## Related Documentation

- [ADR-0001: Infrastructure Organization](../architecture/adr/0001-infrastructure-organization-hybrid-approach.md) - Infrastructure architecture decision
- [ADR-0017: Resource Group Organization Strategy](../architecture/adr/0017-resource-group-organization-strategy.md) - 3-tier RG structure
- [Infrastructure Repository](../infra/) - Infrastructure as code (Terraform, Bicep, Kubernetes)
