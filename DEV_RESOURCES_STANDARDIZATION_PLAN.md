# Dev Resources Standardization Plan (v2.2 Compliant)

## Azure Naming Convention v2.2 Standard

**Pattern:** `[org]-[env]-[project]-[type]-[region]`

### Components:

- **org:** Organization code (`mys` for Mystira)
- **env:** Environment (`dev`, `staging`, `prod`)
- **project:** What it belongs to (domain/system) - `core`, `chain`, `publisher`, `story`
- **type:** Resource class (from controlled vocabulary)
- **region:** `san` (South Africa North) or `glob` (Global)

### Key Rules:

1. ✓ 5-part structure only (no 6-part names)
2. ✓ No org duplication (never `mys-dev-mys-...`)
3. ✓ Global resources use `glob` region
4. ✓ Project codes are nouns (what it belongs to)
5. ✓ Type codes are resource classes (what it is)

---

## Current Problems

### 1. Six-Part Names (Non-Compliant)

Current Terraform creates **6-part names** which violates v2.2:

- ❌ `mys-dev-mystira-chain-log-san` (org-env-project-service-type-region)
- ❌ `mys-dev-mystira-pub-nsg-san` (has both project and service)
- ✅ Should be: `mys-dev-chain-log-san` (5 parts)

**Root cause:** Terraform modules use:

```terraform
name_prefix = "mys-${var.environment}-mystira-chain"  # WRONG - 4 parts before type
```

Should be:

```terraform
name_prefix = "mys-${var.environment}-chain"  # CORRECT - 3 parts before type
```

### 2. Shared Modules Use Wrong Pattern

- ❌ `mystira-shared-pg-dev-server` (doesn't follow v2.2 at all)
- ❌ `mystira-shared-redis-dev-cache` (wrong order)
- ✅ Should be: `mys-dev-core-db-san` (using "core" for shared infra)

### 3. Redundant Resources

| Resource Type                | Current Count | Should Be | Waste    |
| ---------------------------- | ------------- | --------- | -------- |
| Log Analytics Workspaces     | 3+            | 1         | ~$150/mo |
| Email Communication Services | 2-3?          | 1         | ~$50/mo  |

---

## Project Structure for Mystira

Following v2.2, we'll use these projects:

| Project Code | Purpose                 | Examples                                        |
| ------------ | ----------------------- | ----------------------------------------------- |
| `core`       | Shared infrastructure   | VNet, AKS, shared DB, shared Redis, shared Logs |
| `chain`      | Blockchain service      | Chain-specific KV, Storage, NSG, App Insights   |
| `publisher`  | Publisher service       | Publisher KV, Service Bus, NSG, App Insights    |
| `story`      | Story Generator service | Story Gen KV, NSG, App Insights                 |

### Why "core" for shared resources?

Following v2.2 canonical projects pattern (e.g., `nl-prod-core-kv-glob`), shared infrastructure should use the "core" project code.

---

## v2.2 Compliant Resource Names for Dev

### Core Infrastructure (Shared)

```
mys-dev-core-rg-san                         # Resource Group
mys-dev-core-vnet-san                       # Virtual Network
mys-dev-core-aks-san                        # AKS Cluster
mys-dev-core-acr-glob                       # Container Registry (global scope)
mys-dev-core-db-san                         # Shared PostgreSQL Flexible Server
mys-dev-core-cache-san                      # Shared Redis Cache
mys-dev-core-log-san                        # Shared Log Analytics (ONE instance)
mys-dev-core-ai-san                         # Shared Application Insights

Storage accounts (no hyphens allowed):
mysdevcorestsan                             # General storage account
```

### Chain Service

```
mys-dev-chain-nsg-san                       # Network Security Group
mys-dev-chain-identity-san                  # Managed Identity
mys-dev-chain-kv-san                        # Key Vault
mys-dev-chain-ai-san                        # Application Insights (uses shared logs)
mysdevchainsan                              # Storage Account (Premium FileStorage)
```

### Publisher Service

```
mys-dev-publisher-nsg-san                   # Network Security Group
mys-dev-publisher-identity-san              # Managed Identity
mys-dev-publisher-kv-san                    # Key Vault
mys-dev-publisher-ai-san                    # Application Insights (uses shared logs)
mysdevpublisherqueuesan                     # Service Bus Namespace (no hyphens)
```

### Story Generator Service

```
mys-dev-story-nsg-san                       # Network Security Group
mys-dev-story-identity-san                  # Managed Identity
mys-dev-story-kv-san                        # Key Vault
mys-dev-story-ai-san                        # Application Insights (uses shared logs)
```

### Global Services

```
mys-dev-core-fd-glob                        # Front Door (global scope)
mys-dev-core-waf-glob                       # WAF Policy (global)
mys-dev-core-email-glob                     # Email Communication Service
mys-dev-core-dns-glob                       # DNS Zone (if needed)
```

---

## Type Vocabulary (v2.2 Controlled List)

| Type Code  | Resource                    | Example                      |
| ---------- | --------------------------- | ---------------------------- |
| `rg`       | Resource Group              | `mys-dev-core-rg-san`        |
| `vnet`     | Virtual Network             | `mys-dev-core-vnet-san`      |
| `subnet`   | Subnet                      | `mys-dev-core-subnet-san`    |
| `nsg`      | Network Security Group      | `mys-dev-chain-nsg-san`      |
| `aks`      | AKS Cluster                 | `mys-dev-core-aks-san`       |
| `acr`      | Container Registry          | `mys-dev-core-acr-glob`      |
| `kv`       | Key Vault                   | `mys-dev-chain-kv-san`       |
| `db`       | Database (PostgreSQL/MySQL) | `mys-dev-core-db-san`        |
| `cache`    | Redis Cache                 | `mys-dev-core-cache-san`     |
| `storage`  | Storage Account             | `mysdevcorestorage`          |
| `queue`    | Service Bus/Queue           | `mysdevpublisherqueue`       |
| `log`      | Log Analytics               | `mys-dev-core-log-san`       |
| `ai`       | Application Insights        | `mys-dev-chain-ai-san`       |
| `identity` | Managed Identity            | `mys-dev-chain-identity-san` |
| `fd`       | Front Door                  | `mys-dev-core-fd-glob`       |
| `waf`      | WAF Policy                  | `mys-dev-core-waf-glob`      |
| `dns`      | DNS Zone                    | `mys-dev-core-dns-glob`      |
| `email`    | Email Service               | `mys-dev-core-email-glob`    |

---

## Resources to DELETE (Redundant)

### Log Analytics Workspaces (Keep only 1)

- ❌ `mys-dev-mystira-chain-log-san` - DELETE (chain creates its own)
- ❌ `mys-dev-mystira-pub-logs` - DELETE (publisher creates its own)
- ✅ `mystira-shared-mon-dev-logs` - KEEP and RENAME to `mys-dev-core-log-san`

### Email Communication Services (Consolidate)

Current resources (from screenshots):

- `AzureManagedDomain` (managed domain for email)
- `mys-dev-mystira-acs-glob` (Azure Communication Service?)
- `mys-dev-mystira-email-glob` (Email Communication Service)

**Action:** Consolidate to ONE email service + ONE managed domain:

- Keep: `mys-dev-core-email-glob` (rename from `mys-dev-mystira-email-glob`)
- Keep: Managed domain (associate with email service)
- Delete: Duplicate/redundant services

### Application Insights Strategy

**Keep service-specific App Insights, but connect to shared Log Analytics:**

- ✅ `mys-dev-chain-ai-san` (chain telemetry)
- ✅ `mys-dev-publisher-ai-san` (publisher telemetry)
- ✅ `mys-dev-story-ai-san` (story generator telemetry)
- ✅ `mys-dev-core-ai-san` (infrastructure monitoring)

All should use `mys-dev-core-log-san` as the workspace backend.

---

## Terraform Code Changes Required

### 1. Update Chain Module

**File:** `infra/terraform/modules/chain/main.tf`

**Changes:**

```terraform
# Line 88: Fix name prefix (remove "mystira")
locals {
  name_prefix = "mys-${var.environment}-chain"  # was: mys-${var.environment}-mystira-chain
  region_code = var.region_code
  # ...
}

# Line 91: Fix Key Vault name
kv_name = "mys-${var.environment}-chain-kv-${local.region_code}"  # was: mys-${var.environment}-chn-kv-${local.region_code}

# Lines 232-240: REMOVE dedicated Log Analytics Workspace
# DELETE: resource "azurerm_log_analytics_workspace" "chain" { ... }

# ADD: Variable for shared Log Analytics
variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace"
  type        = string
}

# UPDATE: Application Insights to reference shared workspace
# (Create new resource if doesn't exist)
resource "azurerm_application_insights" "chain" {
  name                = "${local.name_prefix}-ai-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = var.shared_log_analytics_workspace_id  # Use shared
  application_type    = "other"
  tags                = local.common_tags
}
```

### 2. Update Publisher Module

**File:** `infra/terraform/modules/publisher/main.tf`

**Changes:**

```terraform
# Line 64: Fix name prefix (remove "mystira")
locals {
  name_prefix = "mys-${var.environment}-publisher"  # was: mys-${var.environment}-mystira-pub
  region_code = var.region_code
  # ...
}

# Lines 142-150: REMOVE dedicated Log Analytics Workspace
# DELETE: resource "azurerm_log_analytics_workspace" "publisher" { ... }

# ADD: Variable for shared Log Analytics
variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace"
  type        = string
}

# Line 157: UPDATE Application Insights to use shared workspace
resource "azurerm_application_insights" "publisher" {
  name                = "${local.name_prefix}-ai-${local.region_code}"
  # ...
  workspace_id        = var.shared_log_analytics_workspace_id  # was: azurerm_log_analytics_workspace.publisher.id
  # ...
}
```

### 3. Update Story Generator Module

**File:** `infra/terraform/modules/story-generator/main.tf`

**Changes:**

```terraform
# Line 93: Fix name prefix (remove "mystira")
locals {
  name_prefix = "mys-${var.environment}-story"  # was: mys-${var.environment}-mystira-sg
  region_code = var.region_code
  # ...
}

# Story Gen already uses shared resources correctly ✅
# Just needs name prefix fix
```

### 4. Update Shared PostgreSQL Module

**File:** `infra/terraform/modules/shared/postgresql/main.tf`

**Changes:**

```terraform
# Line 100: Fix name prefix to use "core" project
locals {
  name_prefix = "mys-${var.environment}-core"  # was: mystira-shared-pg-${var.environment}
  # ...
}

# Line 114: Fix Private DNS Zone name
resource "azurerm_private_dns_zone" "postgres" {
  name                = "mys-${var.environment}-core-db.postgres.database.azure.com"  # was: ${local.name_prefix}.postgres...
  # ...
}

# Line 134: Fix PostgreSQL server name
resource "azurerm_postgresql_flexible_server" "shared" {
  name                   = "${local.name_prefix}-db"  # was: ${local.name_prefix}-server
  # ...
}
```

### 5. Update Shared Redis Module

**File:** `infra/terraform/modules/shared/redis/main.tf`

**Changes:**

```terraform
# Line 79: Fix name prefix to use "core" project
locals {
  name_prefix = "mys-${var.environment}-core"  # was: mystira-shared-redis-${var.environment}
  # ...
}

# Line 91: Fix Redis cache name
resource "azurerm_redis_cache" "shared" {
  name                 = "${local.name_prefix}-cache"  # This is now correct
  # ...
}
```

### 6. Update Shared Monitoring Module

**File:** `infra/terraform/modules/shared/monitoring/main.tf`

**Changes:**

```terraform
# Line 49: Fix name prefix to use "core" project
locals {
  name_prefix = "mys-${var.environment}-core"  # was: mystira-shared-mon-${var.environment}
  # ...
}

# Line 64: Fix Log Analytics name
resource "azurerm_log_analytics_workspace" "shared" {
  name                = "${local.name_prefix}-log"  # was: ${local.name_prefix}-logs
  # ...
}

# Line 75: Fix Application Insights name
resource "azurerm_application_insights" "shared" {
  name                = "${local.name_prefix}-ai"  # was: ${local.name_prefix}-insights
  # ...
}
```

### 7. Update Dev Environment

**File:** `infra/terraform/environments/dev/main.tf`

**Changes:**

```terraform
# Line 48: Fix Resource Group name
resource "azurerm_resource_group" "main" {
  name     = "mys-dev-core-rg-san"  # was: mys-dev-mystira-rg-san
  # ...
}

# Line 60: Fix VNet name
resource "azurerm_virtual_network" "main" {
  name                = "mys-dev-core-vnet-san"  # was: mys-dev-mystira-vnet-san
  # ...
}

# Line 260: Fix AKS name
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mys-dev-core-aks-san"  # was: mys-dev-mystira-aks-san
  # ...
}

# Line 128: Fix ACR name
resource "azurerm_container_registry" "shared" {
  name                = "mysdevacr"  # was: mysprodacr
  # ...
}

# Pass shared logs to chain module
module "chain" {
  source = "../../modules/chain"
  # ... existing config ...
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
}

# Pass shared logs to publisher module
module "publisher" {
  source = "../../modules/publisher"
  # ... existing config ...
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
}
```

---

## v2.2 Compliance Validation

### Regex Pattern (from v2.2 spec):

```regex
^(nl|pvc|tws|mys)-(dev|staging|prod)-[a-z0-9-]+-(rg|app|api|func|swa|db|storage|kv|queue|cache|ai|acr|vnet|subnet|dns|log|fd)-(euw|eun|wus|eus|san|saf|swe|uks|usw|glob)$
```

### Compliance Tests:

✅ Must NOT contain `-mys-mystira-` (org duplication)
✅ Must NOT contain 6-part names
✅ If type is `dns` or `fd`, region must be `glob`
✅ Storage accounts: no hyphens, lowercase only

---

## Implementation Plan

### Phase 1: Update Terraform Code (No Infrastructure Changes)

1. ✅ Create branch: `claude/standardize-dev-resources-cT39Z`
2. Update all module name prefixes to remove "mystira" redundancy
3. Change shared modules from `mystira-shared-*` to `mys-{env}-core-*`
4. Add `shared_log_analytics_workspace_id` parameter to chain and publisher modules
5. Remove dedicated Log Analytics from chain and publisher modules
6. Update dev environment configuration
7. Run `terraform fmt` and validate

### Phase 2: Terraform Plan Review

1. Run `terraform plan` to see destroy/recreate changes
2. Export all secrets from existing Key Vaults
3. Document configuration values
4. Review plan with team

### Phase 3: Backup and Prepare

1. Export all Key Vault secrets
2. Backup storage account data
3. Document all configuration
4. Take snapshots if applicable

### Phase 4: Destroy and Recreate

1. Run `terraform destroy` targeting old Log Analytics workspaces
2. Run `terraform apply` to create resources with v2.2 compliant names
3. Restore secrets to new Key Vaults
4. Update Kubernetes manifests to reference new resource names

### Phase 5: Validation and Cleanup

1. Verify all resources created successfully
2. Test all services
3. Delete orphaned resources manually (if any)
4. Update documentation
5. Update CI/CD pipelines
6. Commit and push final changes

---

## Estimated Cost Savings

| Resource                | Current | Proposed | Monthly Savings |
| ----------------------- | ------- | -------- | --------------- |
| Log Analytics (3 → 1)   | ~$150   | ~$50     | $100            |
| Email Services (3? → 1) | ~$50    | ~$25     | $25             |
| **Total**               |         |          | **~$125/mo**    |

---

## Questions Before Proceeding

1. **Project Structure Confirmation:**
   - Is using "core" for shared infra and separate projects for each service (chain, publisher, story) acceptable?
   - Or should all be under "mystira" project with tags to differentiate services?

2. **Container Registry Scope:**
   - Should ACR be `mys-dev-core-acr-glob` (dev-specific) or `mys-shared-acr-glob` (shared across all environments)?
   - Current code has `mysprodacr` in dev, which seems wrong

3. **Email Services:**
   - Which email resources are actually needed vs duplicates?
   - Can you clarify the purpose of each?

4. **Application Insights:**
   - Confirmed: Keep service-specific App Insights connected to shared Log Analytics?

5. **Ready to proceed with Phase 1?**
   - Should I start updating Terraform code on this branch?

---

## Next Steps

Once you confirm the above questions, I'll:

1. Update all Terraform module files
2. Run `terraform fmt` and `terraform validate`
3. Generate a `terraform plan` for review
4. Commit changes to the branch
5. Await your approval before any `terraform apply`
