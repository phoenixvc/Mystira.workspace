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

We will adopt a **tiered resource group strategy** that separates resources by lifecycle, ownership, and security boundary:

### Resource Group Structure

```text
# Tier 1: Shared Infrastructure (long-lived, cross-service)
mys-{env}-network-rg-{region}       # VNet, Subnets, NSGs, DNS
mys-{env}-compute-rg-{region}       # AKS, ACR
mys-{env}-data-rg-{region}          # PostgreSQL, Redis (shared databases)
mys-{env}-monitor-rg-{region}       # Log Analytics, Action Groups

# Tier 2: Service-Specific (per service, independent lifecycle)
mys-{env}-chain-rg-{region}         # Chain: Identity, Key Vault, Storage, App Insights
mys-{env}-publisher-rg-{region}     # Publisher: Identity, Key Vault, Storage, Service Bus, App Insights
mys-{env}-story-rg-{region}         # Story Generator: Identity, Key Vault, Storage, App Insights
mys-{env}-admin-rg-{region}         # Admin API: Identity, Key Vault, App Insights

# Tier 3: Global/Shared (environment-independent)
mys-shared-terraform-rg-{region}    # Terraform state backend
mys-shared-acr-rg-{region}          # Container Registry (if separated from compute)
mys-prod-dns-rg-glob                # DNS zones (global, prod only)
mys-prod-frontdoor-rg-glob          # Front Door, WAF (global, prod only)
mys-shared-identity-rg-glob         # Entra ID apps (optional, if managed separately)
```

### Resource Allocation Matrix

| Resource Type | Resource Group | Rationale |
|--------------|----------------|-----------|
| Virtual Network | `network-rg` | Core networking, rarely changes |
| Subnets | `network-rg` | Part of VNet lifecycle |
| NSGs (VNet-level) | `network-rg` | Network security rules |
| Private DNS Zones | `network-rg` | VNet-linked DNS |
| AKS Cluster | `compute-rg` | Shared compute platform |
| AKS Node Pools | `compute-rg` | Part of AKS lifecycle |
| Container Registry | `compute-rg` or `shared-acr-rg` | Container images |
| PostgreSQL Server | `data-rg` | Shared database, long-lived |
| PostgreSQL Databases | `data-rg` | Part of server lifecycle |
| Redis Cache | `data-rg` | Shared cache, long-lived |
| Log Analytics | `monitor-rg` | Central logging |
| Action Groups | `monitor-rg` | Alert notifications |
| Chain Identity | `chain-rg` | Service-specific |
| Chain Key Vault | `chain-rg` | Service secrets |
| Chain Storage | `chain-rg` | Service data |
| Chain App Insights | `chain-rg` | Service telemetry |
| Publisher Identity | `publisher-rg` | Service-specific |
| Publisher Key Vault | `publisher-rg` | Service secrets |
| Publisher Service Bus | `publisher-rg` | Service messaging |
| Publisher App Insights | `publisher-rg` | Service telemetry |
| Story Generator Identity | `story-rg` | Service-specific |
| Story Generator Key Vault | `story-rg` | Service secrets |
| Story Generator App Insights | `story-rg` | Service telemetry |
| Admin API Identity | `admin-rg` | Service-specific |
| Admin API Key Vault | `admin-rg` | Service secrets |
| Admin API App Insights | `admin-rg` | Service telemetry |
| Front Door | `frontdoor-rg` | Global routing |
| WAF Policy | `frontdoor-rg` | Security policy |
| DNS Zone | `dns-rg` | Domain management |
| Entra ID Apps | `identity-rg` or in tenant | Identity platform |

### Complete Resource Group Inventory

#### Development Environment

```text
mys-dev-network-rg-san      # VNet, Subnets, NSGs, Private DNS
mys-dev-compute-rg-san      # AKS
mys-dev-data-rg-san         # PostgreSQL, Redis
mys-dev-monitor-rg-san      # Log Analytics, Action Groups
mys-dev-chain-rg-san        # Chain service resources
mys-dev-publisher-rg-san    # Publisher service resources
mys-dev-story-rg-san        # Story Generator resources
mys-dev-admin-rg-san        # Admin API resources
```

#### Staging Environment

```text
mys-staging-network-rg-san
mys-staging-compute-rg-san
mys-staging-data-rg-san
mys-staging-monitor-rg-san
mys-staging-chain-rg-san
mys-staging-publisher-rg-san
mys-staging-story-rg-san
mys-staging-admin-rg-san
```

#### Production Environment

```text
mys-prod-network-rg-san
mys-prod-compute-rg-san
mys-prod-data-rg-san
mys-prod-monitor-rg-san
mys-prod-chain-rg-san
mys-prod-publisher-rg-san
mys-prod-story-rg-san
mys-prod-admin-rg-san
mys-prod-frontdoor-rg-glob    # Global resources
mys-prod-dns-rg-glob          # Global resources
```

#### Shared (Environment-Independent)

```text
mys-shared-terraform-rg-san   # Existing: Terraform state
mys-shared-acr-rg-san         # Optional: If ACR separated from compute
```

### Naming Convention

Follows ADR-0008 pattern:

```text
[org]-[env]-[project]-rg-[region]
```

Where project values for resource groups are:

| Project Code | Purpose |
|-------------|---------|
| `network` | Networking resources |
| `compute` | Compute platforms (AKS) |
| `data` | Shared data stores |
| `monitor` | Monitoring/observability |
| `chain` | Chain service |
| `publisher` | Publisher service |
| `story` | Story Generator service |
| `admin` | Admin API service |
| `frontdoor` | Global edge/CDN |
| `dns` | DNS management |
| `terraform` | IaC state |
| `acr` | Container registry |

## Rationale

### 1. Security Isolation (RBAC)

Separating services into dedicated resource groups enables:

- **Least privilege**: Grant developers access only to their service's RG
- **Secrets isolation**: Each service's Key Vault is in a separate RBAC scope
- **Audit clarity**: Know exactly who accessed which service's resources
- **Third-party access**: Grant external vendors access to specific services only

Example RBAC assignments:

```text
Chain Team       → Contributor on mys-{env}-chain-rg-san
Publisher Team   → Contributor on mys-{env}-publisher-rg-san
Platform Team    → Contributor on mys-{env}-network-rg-san, compute-rg, data-rg, monitor-rg
DevOps           → Reader on all RGs, Contributor on mys-shared-terraform-rg-san
```

### 2. Lifecycle Management

Resources are grouped by how often they change:

| Tier | Change Frequency | Example |
|------|-----------------|---------|
| Network | Rarely (months) | VNets, Subnets |
| Compute | Occasionally (weeks) | AKS versions, node pools |
| Data | Rarely (months) | Database servers |
| Monitor | Occasionally | Log retention, alerts |
| Services | Frequently (daily) | Service configs, secrets |

### 3. Cost Attribution

Each resource group enables:

- **Cost tagging**: Tag RGs with cost center, owner, service
- **Budget alerts**: Set budgets per service or infrastructure tier
- **Chargeback**: Bill teams for their service resource consumption
- **Optimization**: Identify high-cost services independently

### 4. Blast Radius Reduction

| Current (Single RG) | Proposed (Multi-RG) |
|--------------------|---------------------|
| Accidental RG delete = entire environment lost | Accidental delete = one service or component |
| Terraform destroy affects everything | Terraform modules target specific RGs |
| One bad deployment breaks all | Failures isolated to service |

### 5. Azure Best Practices Alignment

This approach aligns with:

- **Azure Well-Architected Framework**: Recommends logical grouping by lifecycle
- **Cloud Adoption Framework**: Suggests separating workloads for governance
- **Landing Zone patterns**: Uses tiered resource organization

## Consequences

### Positive

1. ✅ **Security**: Fine-grained RBAC per service
2. ✅ **Cost visibility**: Track spending by service/component
3. ✅ **Blast radius**: Failures/deletions are isolated
4. ✅ **Lifecycle alignment**: Resources grouped by change frequency
5. ✅ **Team autonomy**: Teams manage their own RGs
6. ✅ **Compliance**: Easier to audit and demonstrate isolation
7. ✅ **Scalability**: Won't hit 800 resources/RG limit
8. ✅ **Parallel deployment**: Services deploy independently

### Negative

1. ⚠️ **Migration effort**: Moving existing resources requires planning
2. ⚠️ **More RGs to manage**: Increases from 4 to ~28 RGs (9 per env × 3 + shared)
3. ⚠️ **Cross-RG references**: Need explicit resource IDs for cross-RG dependencies
4. ⚠️ **Terraform refactoring**: State migration and module updates required
5. ⚠️ **Naming complexity**: More RGs means more names to remember

### Mitigations

| Risk | Mitigation |
|------|------------|
| Migration complexity | Phased approach, start with new services |
| RG sprawl | Consistent naming, tagging, documentation |
| Cross-RG references | Terraform data sources, output passing |
| State migration | Use `terraform state mv`, test in dev first |

## Alternatives Considered

### Alternative 1: Keep Single RG per Environment (Rejected)

**Approach**: Continue with `mys-{env}-core-rg-{region}` for everything.

**Pros**:
- Simple, no migration needed
- Fewer RGs to manage
- All resources visible in one place

**Cons**:
- No isolation or fine-grained RBAC
- Cannot track costs per service
- Large blast radius
- Approaching Azure limits

**Decision**: Rejected - Does not address security and cost visibility needs.

### Alternative 2: One RG per Resource Type (Rejected)

**Approach**: Group by Azure resource type (all Key Vaults in one RG, all Storage in another).

```text
mys-{env}-keyvaults-rg-{region}
mys-{env}-storage-rg-{region}
mys-{env}-identities-rg-{region}
```

**Pros**:
- Easy to find resources by type
- Consistent management patterns

**Cons**:
- Breaks service isolation (all Key Vaults accessible together)
- Lifecycle mismatch (different services' Key Vaults change at different times)
- No cost attribution per service

**Decision**: Rejected - Does not provide service-level isolation.

### Alternative 3: One RG per Service Only (Considered)

**Approach**: Each service gets its own RG with everything, including networking and databases.

```text
mys-{env}-chain-rg-{region}      # VNet, AKS, databases, everything
mys-{env}-publisher-rg-{region}  # Duplicate VNet, AKS, databases
```

**Pros**:
- Complete isolation per service
- Full autonomy for each team

**Cons**:
- Massive duplication (each service has its own VNet, AKS)
- Much higher costs
- Harder to manage shared dependencies
- Networking complexity (VNet peering everywhere)

**Decision**: Rejected - Too much duplication and cost.

### Alternative 4: Hybrid with Fewer Tiers (Considered)

**Approach**: Only 2 tiers - shared and service-specific.

```text
mys-{env}-shared-rg-{region}  # VNet, AKS, PostgreSQL, Redis, Monitoring
mys-{env}-chain-rg-{region}
mys-{env}-publisher-rg-{region}
mys-{env}-story-rg-{region}
mys-{env}-admin-rg-{region}
```

**Pros**:
- Simpler than 4-tier approach
- Still provides service isolation
- Fewer RGs to manage

**Cons**:
- "Shared" becomes a dumping ground
- Cannot grant network access without database access
- Mixed lifecycles in shared RG

**Decision**: Viable but less flexible. The 4-tier approach provides better separation.

## Implementation

### Phase 1: New Services (Immediate)

Deploy any new services to dedicated resource groups following this pattern.

### Phase 2: Terraform Refactoring (Week 1-2)

1. Create new resource group resources in Terraform
2. Update module outputs to expose resource group names
3. Add resource group parameters to all modules

```hcl
# Example: Updated module structure
module "chain" {
  source = "../../modules/chain"

  resource_group_name = azurerm_resource_group.chain.name  # Service-specific RG
  network_rg_name     = azurerm_resource_group.network.name
  data_rg_name        = azurerm_resource_group.data.name
  ...
}
```

### Phase 3: Dev Environment Migration (Week 2-3)

1. Create new RGs in dev
2. Use `terraform state mv` to migrate resources
3. Update cross-RG references
4. Validate all services work
5. Update CI/CD pipelines

### Phase 4: Staging Migration (Week 3-4)

Repeat Phase 3 for staging environment.

### Phase 5: Production Migration (Week 4-5)

1. Schedule maintenance window
2. Repeat Phase 3 for production
3. Verify all monitoring and alerts
4. Update documentation

### Terraform State Migration Example

```bash
# Move Chain resources to new RG (after creating new RG in TF)
terraform state mv \
  'module.chain.azurerm_user_assigned_identity.main' \
  'module.chain.azurerm_user_assigned_identity.main'

# Most resources need recreation - plan carefully
terraform plan -target=module.chain
```

### RBAC Configuration

```hcl
# Example: Team access to service RG
resource "azurerm_role_assignment" "chain_team" {
  scope                = azurerm_resource_group.chain.id
  role_definition_name = "Contributor"
  principal_id         = data.azuread_group.chain_team.object_id
}

# Platform team access to shared RGs
resource "azurerm_role_assignment" "platform_network" {
  scope                = azurerm_resource_group.network.id
  role_definition_name = "Contributor"
  principal_id         = data.azuread_group.platform_team.object_id
}
```

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
