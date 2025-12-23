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
# Tier 1: Shared Infrastructure (existing core-rg, extended)
mys-{env}-core-rg-{region}          # VNet, AKS, PostgreSQL, Redis, Cosmos DB, Shared Storage, Log Analytics

# Tier 2: Service-Specific (service compute & secrets only)
mys-{env}-chain-rg-{region}         # Chain: Identity, Key Vault, App Insights
mys-{env}-publisher-rg-{region}     # Publisher: Identity, Key Vault, Service Bus, App Insights
mys-{env}-story-rg-{region}         # Story Generator: Identity, Key Vault, App Insights
mys-{env}-admin-rg-{region}         # Admin API: Identity, Key Vault, App Insights
mys-{env}-app-rg-{region}           # App: App Service, Static Web App, Key Vault, App Insights

# Tier 3: Global/Shared (environment-independent, like ACR)
mys-shared-terraform-rg-{region}    # Terraform state backend (existing)
mys-shared-acr-rg-{region}          # Container Registry (existing, in core-rg currently)
mys-shared-comms-rg-glob            # Communication Services, Email (cross-environment)
mys-prod-dns-rg-glob                # DNS zones (global, prod only)
mys-prod-frontdoor-rg-glob          # Front Door, WAF (global, prod only)
```

### Resource Allocation Matrix

| Resource Type | Resource Group | Rationale |
|--------------|----------------|-----------|
| **Shared Infrastructure (per-environment)** | | |
| Virtual Network | `core-rg` | Core networking, rarely changes |
| Subnets | `core-rg` | Part of VNet lifecycle |
| NSGs (VNet-level) | `core-rg` | Network security rules |
| Private DNS Zones | `core-rg` | VNet-linked DNS |
| AKS Cluster | `core-rg` | Shared compute platform |
| AKS Node Pools | `core-rg` | Part of AKS lifecycle |
| PostgreSQL Server | `core-rg` | Shared database, long-lived |
| PostgreSQL Databases | `core-rg` | Part of server lifecycle |
| Redis Cache | `core-rg` | Shared cache, long-lived |
| Service Bus Namespace | `core-rg` | Shared messaging (used by publisher, admin, app, future services) |
| Cosmos DB Account | `core-rg` | Shared document store (used by App, Admin, future services) |
| Shared Storage Account | `core-rg` | Media, content (used by multiple services) |
| Log Analytics Workspace | `core-rg` | Central logging |
| Action Groups | `core-rg` | Alert notifications |
| **Chain Service** | | |
| Chain Identity | `chain-rg` | Service-specific |
| Chain Key Vault | `chain-rg` | Service secrets |
| Chain App Insights | `chain-rg` | Service telemetry |
| **Publisher Service** | | |
| Publisher Identity | `publisher-rg` | Service-specific |
| Publisher Key Vault | `publisher-rg` | Service secrets |
| Publisher App Insights | `publisher-rg` | Service telemetry |
| **Story Generator** | | |
| Story Generator Identity | `story-rg` | Service-specific |
| Story Generator Key Vault | `story-rg` | Service secrets |
| Story Generator App Insights | `story-rg` | Service telemetry |
| **Admin API** | | |
| Admin API Identity | `admin-rg` | Service-specific |
| Admin API Key Vault | `admin-rg` | Service secrets |
| Admin API App Insights | `admin-rg` | Service telemetry |
| **App (PWA + API)** | | |
| App Service Plan | `app-rg` | API hosting |
| App Service (API) | `app-rg` | .NET API backend |
| Static Web App (PWA) | `app-rg` | Blazor WASM frontend |
| App Key Vault | `app-rg` | App secrets |
| App Insights | `app-rg` | App telemetry |
| Azure Bot | `app-rg` | Teams integration (optional) |
| **Cross-Environment Shared (like ACR)** | | |
| Container Registry | `shared-acr-rg` | Container images (all envs) |
| Communication Services | `shared-comms-rg` | Email/SMS (all envs) |
| Email Communication Svc | `shared-comms-rg` | Email sending (all envs) |
| Terraform State Storage | `shared-terraform-rg` | IaC state |
| **Global Resources (prod only)** | | |
| Front Door | `frontdoor-rg` | Global routing |
| WAF Policy | `frontdoor-rg` | Security policy |
| DNS Zone | `dns-rg` | Domain management |

### Complete Resource Group Inventory

#### Development Environment

```text
mys-dev-core-rg-san         # Shared: VNet, AKS, PostgreSQL, Redis, Cosmos DB, Storage, Log Analytics
mys-dev-chain-rg-san        # Chain: Identity, Key Vault, App Insights
mys-dev-publisher-rg-san    # Publisher: Identity, Key Vault, Service Bus, App Insights
mys-dev-story-rg-san        # Story Generator: Identity, Key Vault, App Insights
mys-dev-admin-rg-san        # Admin API: Identity, Key Vault, App Insights
mys-dev-app-rg-san          # App: App Service, Static Web App, Key Vault, App Insights
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
mys-prod-frontdoor-rg-glob    # Global edge/CDN
mys-prod-dns-rg-glob          # DNS zones
```

#### Cross-Environment Shared (like ACR)

```text
mys-shared-terraform-rg-san   # Existing: Terraform state
mys-shared-acr-rg-san         # Container Registry (currently in dev core-rg)
mys-shared-comms-rg-glob      # Communication Services, Email Service
```

**Note**: Cross-environment resources follow the pattern `mys-shared-{purpose}-rg-{region}` and are used by all environments (dev, staging, prod). Similar to how ACR uses image tags to separate environments, these resources use configuration/data separation rather than resource duplication.

### Naming Convention

Follows ADR-0008 pattern:

```text
[org]-[env]-[project]-rg-[region]
```

Where project values for resource groups are:

| Project Code | Purpose |
|-------------|---------|
| `core` | Shared infrastructure (VNet, AKS, databases, Cosmos DB, Storage, monitoring) |
| `chain` | Chain service (blockchain/ledger) |
| `publisher` | Publisher service (content publishing) |
| `story` | Story Generator service |
| `admin` | Admin API service |
| `app` | App service (PWA + API compute) |
| `acr` | Container Registry (cross-environment) |
| `comms` | Communication Services (cross-environment) |
| `frontdoor` | Global edge/CDN |
| `dns` | DNS management |
| `terraform` | IaC state |

### Cross-Environment Resource Sharing

Some resources are shared across all environments (dev, staging, prod) to reduce costs and simplify management:

| Resource | Pattern | Separation Strategy |
|----------|---------|---------------------|
| Container Registry (ACR) | `mys-shared-acr-rg-san` | Image tags: `dev/*`, `staging/*`, `prod/*` |
| Communication Services | `mys-shared-comms-rg-glob` | Configuration per environment |
| Email Service | `mys-shared-comms-rg-glob` | Sender addresses per environment |
| Terraform State | `mys-shared-terraform-rg-san` | State files: `dev/`, `staging/`, `prod/` |

**Why share these resources?**

1. **ACR**: Images are environment-agnostic; tags provide separation. One registry reduces costs.
2. **Communication Services**: Email/SMS infrastructure is stateless; configuration determines behavior.
3. **Terraform State**: Single backend with environment-keyed state files.

**RBAC for shared resources:**
- Platform team has Contributor access
- Service teams have Reader access (or specific role for pushing images to ACR)

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

## Decision Analysis

### Weighted Decision Matrix

We evaluated four approaches against seven criteria, weighted by importance to Mystira's current needs:

| Criteria | Weight | A: Status Quo | B: Proposed | C: Full Split | D: Per-Service |
|----------|--------|---------------|-------------|---------------|----------------|
| **Migration Effort** | 25% | 5 (none) | 4 (minimal) | 2 (significant) | 1 (massive) |
| **RBAC/Security Isolation** | 25% | 1 (none) | 4 (secrets isolated) | 5 (full) | 5 (full) |
| **Cost Visibility** | 15% | 1 (none) | 4 (per service) | 4 (per service) | 5 (complete) |
| **Blast Radius** | 15% | 1 (entire env) | 3 (core + services) | 4 (component) | 5 (single service) |
| **Operational Complexity** | 10% | 5 (simple) | 4 (manageable) | 2 (complex) | 1 (very complex) |
| **Terraform Management** | 5% | 5 (simple) | 4 (modular) | 3 (many states) | 2 (fragmented) |
| **Team Scalability** | 5% | 2 (poor) | 4 (good) | 4 (good) | 3 (overkill) |
| **Weighted Score** | 100% | **2.45** | **3.85** | **3.30** | **2.95** |

**Scoring**: 1 = Poor, 3 = Adequate, 5 = Excellent

### Score Breakdown

**Option A: Status Quo (Score: 2.45)**
- Easiest to maintain but fails on isolation, cost tracking, and blast radius
- Not viable for multi-team or compliance scenarios

**Option B: Proposed Strategy (Score: 3.85)** ✅ Selected
- Best balance of migration effort vs. security isolation
- Secrets (Key Vaults) are isolated per service - primary goal achieved
- Core infrastructure stays together - minimal disruption
- Cross-environment sharing reduces costs

**Option C: Full 4-Tier Split (Score: 3.30)**
- Better isolation but significantly more migration effort
- 8+ RGs per environment creates operational overhead
- Overkill for current team size (< 10 developers)
- Consider if team grows beyond 20 or compliance requires it

**Option D: Full Per-Service Isolation (Score: 2.95)**
- Massive cost (duplicate VNets, AKS clusters, databases)
- Extreme complexity with VNet peering
- Only justified for completely independent business units

### Honest Assessment of Proposed Strategy

**Strengths:**
1. ✅ **Solves the primary pain point** - Key Vault isolation per service
2. ✅ **Minimal disruption** - Core infra unchanged
3. ✅ **Right-sized for team** - Not over-engineered
4. ✅ **Evolvable** - Can split core-rg later if needed
5. ✅ **Cross-env sharing** - ACR/Comms pattern is proven

**Acknowledged Trade-offs:**
1. ⚠️ **core-rg remains large** - VNet + AKS + PostgreSQL + Redis + Cosmos DB + Storage
2. ⚠️ **Blast radius for core** - Deleting core-rg still destroys environment
3. ⚠️ **Shared data access** - All services can access Cosmos DB (no database-level RBAC)
4. ⚠️ **Mixed patterns** - PaaS (App Service) and containers (AKS) in same structure

**Why these trade-offs are acceptable:**
1. **Core-rg size**: Platform team owns it entirely; no service team has write access
2. **Blast radius**: Mitigated by Terraform state, Azure locks, and backup policies
3. **Shared data**: Application-level auth handles data isolation; DB-level separation is future work
4. **Mixed patterns**: Both deploy independently to their RGs; core-rg just holds shared infra

### When to Reconsider

Upgrade to **Option C (Full Split)** when:
- Team grows beyond 20 developers
- Multiple distinct platform teams form
- Compliance requires network/data separation
- Need separate RBAC for DBA vs. Network Admin roles

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

## Implementation Gap Analysis

### Current State Assessment

The infrastructure is **fully aligned** with this ADR. All environments (dev, staging, prod) have been updated with the 3-tier resource group structure and supporting infrastructure.

**Analysis (updated 2025-12-23):**

| Component | Current State | Required State | Status |
|-----------|---------------|----------------|--------|
| **Resource Groups/Env** | 6 (core + 5 service) | 6 (core + 5 service) | ✅ All environments |
| **ACR Location** | `mys-shared-acr-rg-san` | `mys-shared-acr-rg-san` | ✅ Shared module |
| **Communication Svc** | `mys-shared-comms-rg-glob` | `mys-shared-comms-rg-glob` | ✅ Module created |
| **Service Bus** | In core-rg (shared) | In core-rg (shared) | ✅ Shared module |
| **Service Key Vaults** | In service-rg | In service-rg | ✅ All services |
| **Module RG Params** | Modules use service RGs | Modules use service RGs | ✅ Updated |
| **RBAC Configuration** | Explicit boolean flags | Explicit boolean flags | ✅ All environments |
| **Service Bus RBAC** | Publisher has sender/receiver | Publisher has sender/receiver | ✅ Identity module |
| **CI/CD Workflows** | Multi-RG support | Multi-RG support | ✅ Updated |

### Files Updated

All required files have been updated to implement this ADR:

#### Terraform Environments (Completed)

| File | Changes Made |
|------|-------------|
| `infra/terraform/environments/dev/main.tf` | ✅ 5 service RGs, shared ACR/comms RGs, Service Bus, RBAC flags |
| `infra/terraform/environments/staging/main.tf` | ✅ 5 service RGs, Service Bus, RBAC flags, ACR reference fixed |
| `infra/terraform/environments/prod/main.tf` | ✅ 5 service RGs, Premium Service Bus, RBAC flags, ACR reference fixed |

#### Terraform Modules (Completed)

| Module | Purpose | Status |
|--------|---------|--------|
| `modules/shared/container-registry/` | Shared ACR for all environments | ✅ Created |
| `modules/shared/communications/` | ACS/Email services | ✅ Created |
| `modules/shared/servicebus/` | Shared Service Bus messaging | ✅ Created |
| `modules/shared/identity/` | Cross-RG RBAC, federated credentials per service RG | ✅ Updated |
| `modules/story-generator/` | Key Vault with Terraform SP access policy | ✅ Fixed |
| `modules/chain/` | Key Vault with Terraform SP access policy | ✅ Fixed |
| `modules/publisher/` | Key Vault with Terraform SP access policy | ✅ Has policy |
| `modules/admin-api/` | Key Vault with Terraform SP access policy | ✅ Has policy |

#### CI/CD Workflows (Completed)

| File | Changes Made |
|------|-------------|
| `.github/workflows/infra-deploy.yml` | ✅ Service RG env variables, ACR import logic |
| `.github/workflows/staging-release.yml` | ✅ Service RG env variables |
| `.github/workflows/production-release.yml` | ✅ Service RG env variables |
| `.github/workflows/keyvault-secrets.yml` | ✅ Multi-service Key Vault support |

### What's Working

1. ✅ **Terraform modules accept RG parameters** - All modules use service-specific RGs
2. ✅ **Kubernetes manifests** - Reference Key Vaults by name, auto-resolves
3. ✅ **Shared monitoring module** - Uses core-rg across all environments
4. ✅ **Identity module** - Supports cross-RG RBAC with Service Bus access
5. ✅ **Service Bus** - Shared in core-rg, publisher has sender/receiver RBAC
6. ✅ **Federated identity credentials** - Created in service-specific RGs (matching parent identity)
7. ✅ **Key Vault access policies** - All modules grant Terraform SP access for secret management

### Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Key Vault secret loss during migration | HIGH | Backup secrets before any changes |
| Service interruption during transition | HIGH | Blue-green deployment, dev first |
| Cross-RG RBAC failures | MEDIUM | Pre-test identity assignments |
| CI/CD pipeline breaks | MEDIUM | Update workflows before prod migration |
| Terraform state drift | MEDIUM | Use `terraform import` carefully |

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

# Cross-environment shared RGs (create once, typically in a shared workspace)
resource "azurerm_resource_group" "shared_acr" {
  count    = var.environment == "dev" ? 1 : 0  # Only create once
  name     = "mys-shared-acr-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, {
    Environment = "shared"
    Service     = "acr"
  })
}

resource "azurerm_resource_group" "shared_comms" {
  count    = var.environment == "dev" ? 1 : 0  # Only create once
  name     = "mys-shared-comms-rg-glob"
  location = var.location  # Logical location, resource is global
  tags     = merge(local.common_tags, {
    Environment = "shared"
    Service     = "communications"
  })
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

### Phase 1: Preparation
- [x] Backup all Key Vault secrets (all environments)
- [x] Document current RBAC assignments
- [x] Create new Terraform modules:
  - [x] `modules/shared/container-registry/` - Created with SKU support, geo-replication
  - [x] `modules/shared/communications/` - Created with Email service, domain management
  - [x] `modules/shared/servicebus/` - Created for cross-service messaging

### Phase 2: Development Environment
- [x] Add 5 new resource group definitions to `dev/main.tf`
- [x] Create `mys-shared-acr-rg-san` and `mys-shared-comms-rg-glob`
- [x] Move ACR resource to shared ACR module (`modules/shared/container-registry/`)
- [x] Add shared Service Bus module to core-rg
- [x] Update module calls to use service-specific RGs
- [x] Update CI/CD workflows with service RG documentation and ACR module import
- [x] Enable RBAC boolean flags in identity module configuration
- [ ] Run `terraform plan` - verify changes
- [ ] Run `terraform apply` - creates new RGs
- [ ] Migrate secrets to new Key Vaults
- [ ] Test all services connect to correct Key Vaults

### Phase 3: Staging Environment
- [x] Update `staging/main.tf` with service RGs
- [x] Add shared Service Bus module
- [x] Update ACR data source to reference `mys-shared-acr-rg-san`
- [x] Enable RBAC boolean flags and Service Bus access
- [x] Remove incorrect Redis subnet delegation (Standard SKU)
- [ ] Apply Terraform changes
- [ ] Validate all services

### Phase 4: Production Environment
- [x] Update `prod/main.tf` with service RGs
- [x] Add Premium Service Bus module (zone redundant)
- [x] Update ACR data source
- [x] Fix invalid ACR output references
- [x] Enable RBAC boolean flags and Service Bus access
- [x] Remove incorrect Redis subnet delegation (Standard SKU)
- [ ] Schedule maintenance window
- [ ] Apply Terraform changes
- [ ] Validate all services
- [ ] Monitor for 24-48 hours

### Phase 5: Cleanup & Documentation
- [ ] Remove old Key Vaults from core-rg (after validation)
- [ ] Update `infra/terraform/README.md`
- [ ] Update `docs/infrastructure/azure-setup.md`
- [ ] Update monitoring alerts for new RGs
- [ ] Archive old Terraform state references

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
