# Terraform Infrastructure

This directory contains Terraform configurations for Mystira's Azure infrastructure.

## Resource Group Organization

Per [ADR-0017](../../docs/architecture/adr/0017-resource-group-organization-strategy.md), resources are organized into a 3-tier structure:

### Tier 1: Core Resource Group (`mys-{env}-core-rg-san`)
Shared infrastructure deployed per environment:
- Virtual Network and Subnets
- AKS Cluster
- PostgreSQL Flexible Server (shared)
- Redis Cache (shared)
- Service Bus Namespace (shared)
- Log Analytics Workspace

### Tier 2: Service Resource Groups (`mys-{env}-{service}-rg-san`)
Service-specific resources:
- **chain-rg**: Chain service (Identity, Key Vault, App Insights)
- **publisher-rg**: Publisher service (Identity, Key Vault, App Insights)
- **story-rg**: Story Generator (Identity, Key Vault, App Insights)
- **admin-rg**: Admin API (Identity, Key Vault, App Insights)
- **app-rg**: App (Static Web App, App Service)

### Tier 3: Cross-Environment Shared
Resources shared across all environments:
- `mys-shared-acr-rg-san`: Container Registry
- `mys-shared-comms-rg-glob`: Communication Services, Email
- `mys-shared-terraform-rg-san`: Terraform state storage

## Directory Structure

```
terraform/
├── environments/          # Environment-specific configurations
│   ├── dev/              # Development environment
│   │   └── main.tf       # Dev resources and module calls
│   ├── staging/          # Staging environment
│   │   └── main.tf       # Staging resources
│   └── prod/             # Production environment
│       └── main.tf       # Prod resources (premium SKUs)
└── modules/              # Reusable Terraform modules
    ├── chain/            # Chain service infrastructure
    ├── publisher/        # Publisher service infrastructure
    ├── story-generator/  # Story Generator infrastructure
    ├── admin-api/        # Admin API infrastructure
    ├── dns/              # DNS zone management
    ├── entra-id/         # Entra ID (Azure AD) authentication
    ├── azure-ad-b2c/     # Azure AD B2C consumer auth
    └── shared/           # Shared infrastructure modules
        ├── postgresql/   # Shared PostgreSQL (core-rg)
        ├── redis/        # Shared Redis (core-rg)
        ├── servicebus/   # Shared Service Bus (core-rg)
        ├── monitoring/   # Log Analytics, App Insights (core-rg)
        ├── container-registry/  # Shared ACR (shared-acr-rg)
        ├── communications/      # ACS/Email (shared-comms-rg)
        └── identity/     # Cross-RG RBAC and workload identity
```

## Quick Start

### Prerequisites
- Terraform >= 1.5.0
- Azure CLI
- Appropriate Azure subscription access

### Deploy Development Environment

```bash
cd environments/dev
terraform init
terraform plan
terraform apply
```

### Environment Variables

Terraform uses Azure CLI authentication by default. For CI/CD:

```bash
export ARM_CLIENT_ID="<service-principal-id>"
export ARM_CLIENT_SECRET="<service-principal-secret>"
export ARM_SUBSCRIPTION_ID="<subscription-id>"
export ARM_TENANT_ID="<tenant-id>"
```

## Shared Resource Access

Services access shared resources via the identity module, which configures RBAC:

```hcl
module "identity" {
  source = "../../modules/shared/identity"

  service_identities = {
    "publisher" = {
      principal_id               = module.publisher.identity_principal_id
      enable_key_vault_access    = true
      key_vault_id               = module.publisher.key_vault_id
      enable_servicebus_sender   = true
      enable_servicebus_receiver = true
      servicebus_namespace_id    = module.shared_servicebus.namespace_id
    }
  }
}
```

## Related Documentation

- [ADR-0017: Resource Group Organization Strategy](../../docs/architecture/adr/0017-resource-group-organization-strategy.md)
- [Infrastructure Guide](../../docs/infrastructure/infrastructure.md)
- [Shared Resources](../../docs/infrastructure/shared-resources.md)
