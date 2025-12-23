# ADR-0017: Resource Group Organization Strategy

## Status

**Accepted** - 2025-12-23

## Context

Azure resource groups serve multiple purposes:

1. **Logical grouping** - Organize related resources for management
2. **Lifecycle management** - Resources that are created/destroyed together
3. **Access control (RBAC)** - Security boundaries for role assignments
4. **Cost tracking** - Enable billing and chargeback by group
5. **Deployment scope** - ARM/Bicep/Terraform deployment target
6. **Blast radius** - Limit impact of accidental deletions or misconfigurations

### Current State

All Mystira resources are currently deployed to a single "core" resource group per environment:

```text
mys-dev-core-rg-san          # All dev resources
mys-staging-core-rg-san      # All staging resources
mys-prod-core-rg-san         # All prod resources
mys-shared-terraform-rg-san  # Terraform state (shared)
```

This includes:

| Category | Resources |
|----------|-----------|
| **Networking** | VNet, 7 Subnets, NSGs |
| **Compute** | AKS Cluster (3 node pools) |
| **Container Registry** | ACR (shared) |
| **Databases** | PostgreSQL Flexible Server, Redis Cache |
| **Monitoring** | Log Analytics, Application Insights (x5), Action Groups |
| **Chain Service** | User Identity, Key Vault, NSG, Storage |
| **Publisher Service** | User Identity, Key Vault, NSG, Storage, Service Bus |
| **Story Generator** | User Identity, Key Vault, Storage |
| **Admin API** | User Identity, Key Vault, NSG |
| **Identity** | Entra ID Apps, Service Principals, Federated Credentials |

### Problems with Current Approach

1. **No isolation** - All resources share the same RBAC scope; granting access to one service grants access to all
2. **Blast radius** - Accidental deletion of RG destroys entire environment
3. **Cost opacity** - Cannot easily track costs per service
4. **Mixed lifecycles** - Shared infrastructure (VNet, AKS) mixed with service resources (Key Vaults, Storage)
5. **Deployment coupling** - Cannot deploy services independently without affecting shared resources
6. **Azure limits** - Approaching 800 resources/RG limit as services grow
7. **Ownership ambiguity** - Unclear which team owns which resources

## Decision

We will adopt a **pragmatic two-tier resource group strategy** that retains `core-rg` for shared infrastructure while extracting service-specific resources into dedicated RGs:

### Resource Group Structure

```text
# Tier 1: Shared Infrastructure (existing core-rg, no migration needed)
mys-{env}-core-rg-{region}          # VNet, Subnets, NSGs, AKS, ACR, PostgreSQL, Redis, Log Analytics

# Tier 2: Service-Specific (new, per service)
mys-{env}-chain-rg-{region}         # Chain: Identity, Key Vault, Storage, App Insights
mys-{env}-publisher-rg-{region}     # Publisher: Identity, Key Vault, Storage, Service Bus, App Insights
mys-{env}-story-rg-{region}         # Story Generator: Identity, Key Vault, Storage, App Insights
mys-{env}-admin-rg-{region}         # Admin API: Identity, Key Vault, App Insights
mys-{env}-app-rg-{region}           # App: Cosmos DB, App Service, Static Web App, Storage, Key Vault, ACS

# Tier 3: Global/Shared (environment-independent)
mys-shared-terraform-rg-{region}    # Terraform state backend (existing)
mys-prod-dns-rg-glob                # DNS zones (global, prod only)
mys-prod-frontdoor-rg-glob          # Front Door, WAF (global, prod only)
```

### Resource Allocation Matrix

| Resource Type | Resource Group | Rationale |
|--------------|----------------|-----------|
| **Shared Infrastructure** | | |
| Virtual Network | `core-rg` | Core networking, rarely changes |
| Subnets | `core-rg` | Part of VNet lifecycle |
| NSGs (VNet-level) | `core-rg` | Network security rules |
| Private DNS Zones | `core-rg` | VNet-linked DNS |
| AKS Cluster | `core-rg` | Shared compute platform |
| AKS Node Pools | `core-rg` | Part of AKS lifecycle |
| Container Registry | `core-rg` | Container images (shared) |
| PostgreSQL Server | `core-rg` | Shared database, long-lived |
| PostgreSQL Databases | `core-rg` | Part of server lifecycle |
| Redis Cache | `core-rg` | Shared cache, long-lived |
| Log Analytics Workspace | `core-rg` | Central logging |
| Action Groups | `core-rg` | Alert notifications |
| **Chain Service** | | |
| Chain Identity | `chain-rg` | Service-specific |
| Chain Key Vault | `chain-rg` | Service secrets |
| Chain Storage | `chain-rg` | Service data |
| Chain App Insights | `chain-rg` | Service telemetry |
| Chain NSG | `chain-rg` | Service network rules |
| **Publisher Service** | | |
| Publisher Identity | `publisher-rg` | Service-specific |
| Publisher Key Vault | `publisher-rg` | Service secrets |
| Publisher Storage | `publisher-rg` | Service data |
| Publisher Service Bus | `publisher-rg` | Service messaging |
| Publisher App Insights | `publisher-rg` | Service telemetry |
| Publisher NSG | `publisher-rg` | Service network rules |
| **Story Generator** | | |
| Story Generator Identity | `story-rg` | Service-specific |
| Story Generator Key Vault | `story-rg` | Service secrets |
| Story Generator Storage | `story-rg` | Service data |
| Story Generator App Insights | `story-rg` | Service telemetry |
| **Admin API** | | |
| Admin API Identity | `admin-rg` | Service-specific |
| Admin API Key Vault | `admin-rg` | Service secrets |
| Admin API App Insights | `admin-rg` | Service telemetry |
| Admin API NSG | `admin-rg` | Service network rules |
| **App (PWA + API)** | | |
| App Service Plan | `app-rg` | API hosting |
| App Service (API) | `app-rg` | .NET API backend |
| Static Web App (PWA) | `app-rg` | Blazor WASM frontend |
| Cosmos DB Account | `app-rg` | App document store |
| App Storage Account | `app-rg` | Media, avatars, content |
| App Key Vault | `app-rg` | App secrets |
| App Insights | `app-rg` | App telemetry |
| Communication Services | `app-rg` | Email/SMS |
| Email Communication Svc | `app-rg` | Email sending |
| Azure Bot | `app-rg` | Teams integration (optional) |
| **Global Resources** | | |
| Front Door | `frontdoor-rg` | Global routing |
| WAF Policy | `frontdoor-rg` | Security policy |
| DNS Zone | `dns-rg` | Domain management |
| Terraform State Storage | `terraform-rg` | IaC state |

### Complete Resource Group Inventory

#### Development Environment

```text
mys-dev-core-rg-san         # Shared: VNet, AKS, PostgreSQL, Redis, ACR, Log Analytics
mys-dev-chain-rg-san        # Chain service resources
mys-dev-publisher-rg-san    # Publisher service resources
mys-dev-story-rg-san        # Story Generator resources
mys-dev-admin-rg-san        # Admin API resources
mys-dev-app-rg-san          # App: PWA, API, Cosmos DB, Storage, ACS
```

#### Staging Environment

```text
mys-staging-core-rg-san
mys-staging-chain-rg-san
mys-staging-publisher-rg-san
mys-staging-story-rg-san
mys-staging-admin-rg-san
mys-staging-app-rg-san
```

#### Production Environment

```text
mys-prod-core-rg-san
mys-prod-chain-rg-san
mys-prod-publisher-rg-san
mys-prod-story-rg-san
mys-prod-admin-rg-san
mys-prod-app-rg-san
mys-prod-frontdoor-rg-glob    # Global resources
mys-prod-dns-rg-glob          # Global resources
```

#### Shared (Environment-Independent)

```text
mys-shared-terraform-rg-san   # Existing: Terraform state
```

### Naming Convention

Follows ADR-0008 pattern:

```text
[org]-[env]-[project]-rg-[region]
```

Where project values for resource groups are:

| Project Code | Purpose |
|-------------|---------|
| `core` | Shared infrastructure (VNet, AKS, databases, monitoring) |
| `chain` | Chain service (blockchain/ledger) |
| `publisher` | Publisher service (content publishing) |
| `story` | Story Generator service |
| `admin` | Admin API service |
| `app` | App service (PWA + API, Cosmos DB, Storage, ACS) |
| `frontdoor` | Global edge/CDN |
| `dns` | DNS management |
| `terraform` | IaC state |

## Rationale

### Why Keep `core-rg`?

1. **No immediate migration** - Shared infrastructure stays in place
2. **Already well-established** - VNet, AKS, databases are stable
3. **Single platform team scope** - Platform team manages all shared infra in one RG
4. **Cross-service dependencies** - Services depend on core infra; keeping it together is logical
5. **Simpler Terraform** - No state migration for existing resources

### Why Extract Service RGs?

1. **Service isolation** - Each service's secrets and resources are RBAC-isolated
2. **Independent deployment** - Services can be deployed without touching core infra
3. **Cost attribution** - Track spending per service
4. **Team ownership** - Service teams own their RG
5. **Reduced blast radius** - Deleting a service RG doesn't affect core infra

### Security Isolation (RBAC)

Example RBAC assignments:

```text
Chain Team       → Contributor on mys-{env}-chain-rg-san
                 → Reader on mys-{env}-core-rg-san (to see shared resources)

Publisher Team   → Contributor on mys-{env}-publisher-rg-san
                 → Reader on mys-{env}-core-rg-san

Platform Team    → Contributor on mys-{env}-core-rg-san
                 → Reader on all service RGs

DevOps           → Reader on all RGs
                 → Contributor on mys-shared-terraform-rg-san
```

### Lifecycle Management

| Tier | Change Frequency | Resources |
|------|-----------------|-----------|
| Core | Rarely (months) | VNet, AKS, PostgreSQL, Redis |
| Services | Frequently (days/weeks) | Key Vaults, Storage, App Insights |
| Global | Rarely | DNS, Front Door |

### Cost Attribution

With service-specific RGs:

- **Per-service budgets**: Set cost alerts per service RG
- **Clear ownership**: Each RG tagged with owning team
- **Chargeback**: Bill teams for their service resources
- **Core costs shared**: Core infra costs split across services

## Consequences

### Positive

1. ✅ **Minimal migration**: Core infra stays in place
2. ✅ **Security**: Fine-grained RBAC per service
3. ✅ **Cost visibility**: Track spending by service
4. ✅ **Blast radius**: Service failures/deletions don't affect core
5. ✅ **Team autonomy**: Service teams manage their own RGs
6. ✅ **Simpler than full split**: Only 6 RGs per env (not 8+)

### Negative

1. ⚠️ **Partial isolation**: Core RG still has mixed resources
2. ⚠️ **Service resources need migration**: Key Vaults, Storage, etc. must move
3. ⚠️ **Cross-RG references**: Need explicit resource IDs for dependencies
4. ⚠️ **More RGs to manage**: Increases from 4 to ~20 RGs total (6 per env × 3 + shared/global)

### Mitigations

| Risk | Mitigation |
|------|------------|
| Cross-RG references | Terraform data sources, output passing |
| State migration | Phased approach, start with new resources |
| Complexity | Clear naming, tagging, documentation |

## Alternatives Considered

### Alternative 1: Full 4-Tier Split (More Granular)

**Approach**: Split core into network, compute, data, and monitor RGs.

```text
mys-{env}-network-rg-{region}   # VNet, Subnets, NSGs
mys-{env}-compute-rg-{region}   # AKS, ACR
mys-{env}-data-rg-{region}      # PostgreSQL, Redis
mys-{env}-monitor-rg-{region}   # Log Analytics
mys-{env}-chain-rg-{region}     # Chain service
...
```

**Pros**:
- Maximum granularity
- Separate RBAC for networking vs databases

**Cons**:
- More RGs (8+ per environment)
- More migration effort
- Overkill for current team size

**Decision**: Not selected - Unnecessary complexity for current scale.

### Alternative 2: Keep Single RG (No Change)

**Approach**: Continue with `mys-{env}-core-rg-{region}` for everything.

**Pros**:
- No migration needed
- Simple

**Cons**:
- No service isolation
- Cannot track costs per service
- Large blast radius

**Decision**: Rejected - Does not address security and cost visibility needs.

### Alternative 3: One RG per Service Including Infrastructure

**Approach**: Each service gets its own VNet, AKS, databases.

**Pros**:
- Complete isolation

**Cons**:
- Massive duplication and cost
- Network complexity

**Decision**: Rejected - Too expensive and complex.

## Implementation

### Phase 1: Create Service RGs (Immediate)

Create the new service resource groups in Terraform:

```hcl
# In each environment's main.tf
resource "azurerm_resource_group" "chain" {
  name     = "mys-${var.environment}-chain-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "chain" })
}

resource "azurerm_resource_group" "publisher" {
  name     = "mys-${var.environment}-publisher-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "publisher" })
}

resource "azurerm_resource_group" "story" {
  name     = "mys-${var.environment}-story-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "story-generator" })
}

resource "azurerm_resource_group" "admin" {
  name     = "mys-${var.environment}-admin-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "admin-api" })
}

resource "azurerm_resource_group" "app" {
  name     = "mys-${var.environment}-app-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "app" })
}
```

### Phase 2: Update Modules to Accept RG Parameter

```hcl
# Example: Updated chain module
module "chain" {
  source = "../../modules/chain"

  resource_group_name = azurerm_resource_group.chain.name  # Now uses service RG
  core_rg_name        = azurerm_resource_group.main.name   # Reference to core
  ...
}
```

### Phase 3: New Resources in Service RGs

Deploy new service resources (Key Vaults, Storage, App Insights) directly to service RGs.

### Phase 4: Migrate Existing Resources (Optional, Phased)

For existing resources, choose:

**Option A: Recreate** (Recommended for Key Vaults with rotation)
```hcl
# Create new Key Vault in service RG
# Migrate secrets
# Update references
# Delete old Key Vault
```

**Option B: Azure Resource Mover** (For supported resources)
```bash
az resource move --destination-group mys-dev-chain-rg-san \
  --ids /subscriptions/.../mys-dev-chain-kv-san
```

**Option C: Leave in core-rg** (For stable resources)
- Keep stable resources in core-rg with proper tagging
- Only move new resources to service RGs

### RBAC Configuration

```hcl
# Service team access
resource "azurerm_role_assignment" "chain_team_contributor" {
  scope                = azurerm_resource_group.chain.id
  role_definition_name = "Contributor"
  principal_id         = data.azuread_group.chain_team.object_id
}

resource "azurerm_role_assignment" "chain_team_core_reader" {
  scope                = azurerm_resource_group.main.id
  role_definition_name = "Reader"
  principal_id         = data.azuread_group.chain_team.object_id
}
```

### Tagging Strategy

All RGs should be tagged for cost tracking and ownership:

```hcl
tags = {
  Environment = var.environment
  Project     = "Mystira"
  Service     = "chain"        # or "core", "publisher", etc.
  Team        = "chain-team"   # owning team
  CostCenter  = "engineering"
  ManagedBy   = "terraform"
}
```

## Migration Checklist

- [ ] Create service RGs in dev
- [ ] Update Terraform modules to accept RG parameter
- [ ] Deploy new Key Vaults to service RGs
- [ ] Migrate secrets to new Key Vaults
- [ ] Update service configurations
- [ ] Deploy new Storage Accounts to service RGs
- [ ] Migrate App Insights to service RGs (or create new)
- [ ] Update CI/CD pipelines
- [ ] Repeat for staging
- [ ] Repeat for production
- [ ] Update documentation

## Related ADRs

- [ADR-0001: Infrastructure Organization - Hybrid Approach](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0008: Azure Resource Naming Conventions](./0008-azure-resource-naming-conventions.md)
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md)
- [ADR-0013: Data Management and Storage Strategy](./0013-data-management-and-storage-strategy.md)

## References

- [Azure Resource Group best practices](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/overview#resource-groups)
- [Azure Well-Architected Framework - Organization](https://learn.microsoft.com/en-us/azure/architecture/framework/)
- [Cloud Adoption Framework - Resource organization](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-setup-guide/organize-resources)
- [Azure Landing Zones](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/landing-zone/)
- [RBAC best practices](https://learn.microsoft.com/en-us/azure/role-based-access-control/best-practices)
- [Azure Resource Mover](https://learn.microsoft.com/en-us/azure/resource-mover/overview)
