# ADR-0008: Azure Resource Naming Conventions

## Status

**Accepted** - 2025-12-14

## Context

Azure resources require consistent naming to:

- Enable easy identification of resources
- Support automation and scripting
- Facilitate cost tracking and organization
- Ensure compliance with Azure naming restrictions
- Support multi-environment deployments (dev, staging, prod)

Azure has specific naming constraints:

- Resource names must be globally unique for some resource types (storage accounts, Key Vaults)
- Length restrictions vary by resource type
- Some resources only allow lowercase alphanumeric characters and hyphens
- Some resources don't allow hyphens (e.g., storage accounts, ACR)

## Decision

We will adopt the following naming conventions for all Azure resources:

### General Principles

1. **Lowercase only**: All resource names use lowercase characters
2. **Hyphens for separation**: Use hyphens to separate words (where allowed)
3. **Environment suffix**: Include environment name (`dev`, `staging`, `prod`) for environment-specific resources
4. **Resource type suffix**: Include resource type abbreviation for clarity
5. **Component prefix**: Include component/service name for service-specific resources
6. **Shared resources**: Prefix with `shared-` for resources shared across services
7. **No dashes for restricted resources**: Some resources (storage accounts, ACR) cannot use dashes

### Naming Pattern

```
mystira-{component}-{env}-{resource-type}
```

Where:

- `mystira`: Project prefix (always lowercase)
- `{component}`: Service/component name (e.g., `chain`, `publisher`, `story-generator`, `shared`)
- `{env}`: Environment (`dev`, `staging`, `prod`)
- `{resource-type}`: Resource type abbreviation (e.g., `rg`, `aks`, `vnet`, `kv`)

### Resource Type Abbreviations

| Resource Type           | Abbreviation | Example                          |
| ----------------------- | ------------ | -------------------------------- |
| Resource Group          | `rg`         | `mystira-dev-rg`                 |
| Azure Kubernetes        | `aks`        | `mystira-dev-aks`                |
| Virtual Network         | `vnet`       | `mystira-dev-vnet`               |
| Subnet                  | `subnet`     | `chain-subnet` (no env prefix)   |
| Key Vault               | `kv`         | `mystira-pub-dev-kv`             |
| Storage Account         | (no dashes)  | `mystiraterraformstate`          |
| Container Registry      | (no dashes)  | `mystiraacr`                     |
| PostgreSQL              | `pg`         | `mystira-shared-pg-dev`          |
| Redis Cache             | (in name)    | `mystira-shared-redis-dev-cache` |
| Application Insights    | `ai`         | `mystira-pub-dev-ai`             |
| Log Analytics Workspace | `law`        | `mystira-dev-law`                |

### Detailed Naming Patterns

#### Resource Groups

**Pattern**: `mystira-{env}-rg`

**Examples**:

- `mystira-dev-rg`
- `mystira-staging-rg`
- `mystira-prod-rg`
- `mystira-terraform-state` (for Terraform state storage)

#### Azure Kubernetes Service (AKS)

**Pattern**: `mystira-{env}-aks`

**Examples**:

- `mystira-dev-aks`
- `mystira-staging-aks`
- `mystira-prod-aks`

**DNS Prefix**: `mystira-{env}` (same as cluster name without `-aks`)

#### Virtual Networks

**Pattern**: `mystira-{env}-vnet`

**Examples**:

- `mystira-dev-vnet`
- `mystira-staging-vnet`
- `mystira-prod-vnet`

#### Subnets

**Pattern**: `{component}-subnet`

**Examples**:

- `chain-subnet`
- `publisher-subnet`
- `aks-subnet`
- `postgresql-subnet`
- `redis-subnet`
- `story-generator-subnet`

**Note**: Subnets don't include environment prefix as they're scoped to a VNet which already includes the environment.

#### Azure Container Registry (ACR)

**Pattern**: `mystiraacr` (shared across all environments)

**Rationale**:

- ACR names cannot contain hyphens
- Must be globally unique
- Shared across all environments (uses tags for environment separation)

**Examples**:

- `mystiraacr` (shared)

#### Storage Accounts

**Pattern**: `mystira{description}` (no dashes, lowercase only)

**Examples**:

- `mystiraterraformstate` (Terraform state backend)
- `mystirastorage` (if needed for general storage)

**Note**: Storage account names must be globally unique, 3-24 characters, lowercase alphanumeric only.

#### Key Vaults

**Pattern**: `mystira-{component}-{env}-kv`

**Examples**:

- `mystira-pub-dev-kv` (Publisher dev Key Vault)
- `mystira-sg-staging-kv` (Story Generator staging Key Vault)
- `mystira-shared-dev-kv` (Shared dev Key Vault)

**Note**: Key Vault names must be globally unique, 3-24 characters, alphanumeric and hyphens only.

#### Shared Resources

**Pattern**: `mystira-shared-{resource}-{env}-{type}`

**Examples**:

- `mystira-shared-pg-dev` (PostgreSQL server)
- `mystira-shared-redis-dev-cache` (Redis cache)
- `mystira-shared-monitoring-dev` (Monitoring resources)

#### Service-Specific Resources

**Pattern**: `mystira-{component}-{env}-{resource-type}`

**Examples**:

- `mystira-chain-dev-nsg` (Chain Network Security Group)
- `mystira-publisher-dev-ai` (Publisher Application Insights)
- `mystira-story-generator-dev-kv` (Story Generator Key Vault)

**Component Abbreviations**:

- `chain` → Chain service
- `pub` → Publisher service
- `sg` → Story Generator service
- `app` → App service (when deployed via infrastructure)

### Kubernetes Resources

Kubernetes resource names follow Kubernetes naming conventions but align with our component structure:

**Namespaces**: `mystira-{env}`

- `mystira-dev`
- `mystira-staging`
- `mystira-prod`

**Deployments/StatefulSets**: `mystira-{component}`

- `mystira-chain`
- `mystira-publisher`
- `mystira-story-generator`

**Services**: `mystira-{component}`

- `mystira-chain`
- `mystira-publisher`
- `mystira-story-generator`

**ConfigMaps/Secrets**: `mystira-{component}-{purpose}`

- `mystira-publisher-config`
- `mystira-story-generator-secrets`

### DNS and URLs

**Pattern**: `{component}.{env}.mystira.app` or `{component}.mystira.app` (prod)

**Examples**:

- `chain.dev.mystira.app` (dev)
- `publisher.staging.mystira.app` (staging)
- `chain.mystira.app` (prod)
- `api.mystira.app` (prod API)

### Terraform State Backend

**Storage Account**: `mystiraterraformstate`
**Container**: `tfstate`
**State File Keys**: `{env}/terraform.tfstate`

- `dev/terraform.tfstate`
- `staging/terraform.tfstate`
- `prod/terraform.tfstate`

## Examples

### Complete Resource Naming Examples

#### Development Environment

```
Resource Group:        mystira-dev-rg
AKS Cluster:           mystira-dev-aks
Virtual Network:       mystira-dev-vnet
Subnets:               chain-subnet, publisher-subnet, aks-subnet
ACR:                   mystiraacr (shared)
Storage Account:       mystiraterraformstate (shared)
Key Vaults:            mystira-pub-dev-kv, mystira-sg-dev-kv
PostgreSQL:            mystira-shared-pg-dev
Redis:                 mystira-shared-redis-dev-cache
Namespace:             mystira-dev
```

#### Staging Environment

```
Resource Group:        mystira-staging-rg
AKS Cluster:           mystira-staging-aks
Virtual Network:       mystira-staging-vnet
Subnets:               chain-subnet, publisher-subnet, aks-subnet
ACR:                   mystiraacr (shared)
Key Vaults:            mystira-pub-staging-kv, mystira-sg-staging-kv
PostgreSQL:            mystira-shared-pg-staging
Redis:                 mystira-shared-redis-staging-cache
Namespace:             mystira-staging
```

#### Production Environment

```
Resource Group:        mystira-prod-rg
AKS Cluster:           mystira-prod-aks
Virtual Network:       mystira-prod-vnet
Subnets:               chain-subnet, publisher-subnet, aks-subnet
ACR:                   mystiraacr (shared)
Key Vaults:            mystira-pub-prod-kv, mystira-sg-prod-kv
PostgreSQL:            mystira-shared-pg-prod
Redis:                 mystira-shared-redis-prod-cache
Namespace:             mystira-prod
```

## Implementation

### Terraform Conventions

In Terraform modules, use `locals` blocks to define naming patterns:

```hcl
locals {
  name_prefix = "mystira-${var.component}-${var.environment}"
  resource_group_name = "mystira-${var.environment}-rg"
  common_tags = {
    Environment = var.environment
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
}
```

### Consistency Checks

1. All resource names should follow these patterns
2. Review Terraform code for consistency
3. Document deviations with rationale
4. Update existing resources during migration/refactoring

## Consequences

### Positive

- ✅ Easy to identify resources by environment and component
- ✅ Supports automation and scripting
- ✅ Clear resource ownership and purpose
- ✅ Easier cost tracking and organization
- ✅ Consistent across all environments
- ✅ Aligns with Azure best practices

### Negative

- ⚠️ Some existing resources may not follow this pattern (migration needed)
- ⚠️ Storage accounts and ACR have restrictions (no dashes)
- ⚠️ Must ensure globally unique names where required

## Migration Strategy

For existing resources that don't follow this convention:

1. **Document current names**: List all existing resources and their current names
2. **Plan migration**: Identify resources that need renaming
3. **Execute during maintenance windows**: Rename resources during planned maintenance
4. **Update documentation**: Update all references to resource names
5. **Update CI/CD**: Update any scripts or workflows that reference resource names

**Note**: Some resources cannot be renamed and must be recreated. Plan migrations carefully.

## References

- [Azure naming conventions](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming)
- [Azure resource naming restrictions](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules)
- [Terraform Azure Provider documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0007: NuGet Feed Strategy](./0007-nuget-feed-strategy-for-shared-libraries.md)
