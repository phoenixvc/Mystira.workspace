# Dev Resources Standardization Plan

## Current Problems

### 1. Naming Inconsistencies
- **Pattern A (Services):** `mys-dev-mystira-{service}-{resource}-{region}`
- **Pattern B (Shared):** `mystira-shared-{service}-{env}-{resource}`
- **No clear standard** between the two patterns

### 2. Redundant Resources
| Resource Type | Current Count | Should Be | Waste |
|--------------|---------------|-----------|-------|
| Log Analytics Workspaces | 3+ | 1 | ~$150/mo |
| Application Insights | 3+ | 3-4 (service-specific) | Acceptable |
| Email Communication Services | 2-3? | 1 | ~$50/mo |

### 3. Terraform Module Issues
- **Publisher module** creates own Log Analytics (should use shared)
- **Chain module** creates own Log Analytics (should use shared)
- **Story-Generator** correctly uses shared resources ✓

---

## Proposed Standardized Naming Convention

### Format
```
mys-{env}-{service}-{resource}-{region}
```

### Region Codes
- `san` = South Africa North
- `glob` = Global (Front Door, Email, DNS, etc.)

### Service Abbreviations
| Service | Code | Notes |
|---------|------|-------|
| Chain | chain | Full name, not "chn" |
| Publisher | publisher | Full name, not "pub" |
| Story Generator | storygen | Shortened for length |
| AKS | aks | Standard abbreviation |

### Resource Type Abbreviations
| Resource | Code | Notes |
|----------|------|-------|
| Resource Group | rg | Standard |
| Virtual Network | vnet | Standard |
| Network Security Group | nsg | Standard |
| Key Vault | kv | Standard |
| Log Analytics | logs | Not "log" |
| Application Insights | ai | Not "appins" |
| Storage Account | {no hyphens} | Azure limitation |
| Container Registry | acr | Standard |
| PostgreSQL | postgres | Not "pg" |
| Front Door | fd | Standard |

---

## Final Resource List for Dev Environment

### Core Infrastructure
```
mys-dev-rg-san                              # Resource Group
mys-dev-vnet-san                            # Virtual Network
mys-dev-aks-san                             # AKS Cluster
mys-dev-acr-glob                            # Container Registry (shared, but in dev RG for now)
mysdevstsan                                 # General Storage Account
```

### Shared Services
```
mys-dev-postgres-san                        # Shared PostgreSQL Flexible Server
mys-dev-redis-san                           # Shared Redis Cache
mys-dev-logs-san                            # Shared Log Analytics (ONE instance)
mys-dev-monitor-ai-san                      # Shared Application Insights (monitoring)
```

### Chain Service
```
mys-dev-chain-nsg-san                       # Chain NSG
mys-dev-chain-identity-san                  # Chain Managed Identity
mys-dev-chain-kv-san                        # Chain Key Vault
mys-dev-chain-ai-san                        # Chain Application Insights (uses shared logs)
mysdevchainsan                              # Chain Storage Account (Premium FileStorage)
```

### Publisher Service
```
mys-dev-publisher-nsg-san                   # Publisher NSG
mys-dev-publisher-identity-san              # Publisher Managed Identity
mys-dev-publisher-kv-san                    # Publisher Key Vault
mys-dev-publisher-ai-san                    # Publisher Application Insights (uses shared logs)
mysdevpubqueuesan                           # Publisher Service Bus Namespace
```

### Story Generator Service
```
mys-dev-storygen-nsg-san                    # Story Gen NSG
mys-dev-storygen-identity-san               # Story Gen Managed Identity
mys-dev-storygen-kv-san                     # Story Gen Key Vault
mys-dev-storygen-ai-san                     # Story Gen Application Insights (uses shared logs)
```

### Global Services
```
mys-dev-fd-glob                             # Front Door
mys-dev-waf-glob                            # WAF Policy
mys-dev-email-glob                          # Email Communication Service
mys-dev-email-domain-glob                   # Email Managed Domain
```

---

## Resources to DELETE (Redundant)

### Log Analytics Workspaces (Keep only 1)
- ✗ `mys-dev-mystira-chain-log-san` - DELETE
- ✗ `mys-dev-mystira-pub-logs` - DELETE
- ✓ `mystira-shared-mon-dev-logs` - KEEP (rename to `mys-dev-logs-san`)

### Email Communication Services (Consolidate)
Need to determine which are duplicates:
- `AzureManagedDomain` (part of email service?)
- `mys-dev-mystira-acs-glob`
- `mys-dev-mystira-email-glob`

**Action:** Keep ONE email communication service + ONE managed domain

### Application Insights (Keep service-specific, but connect to shared logs)
- Keep all service-specific App Insights
- Update them to use the shared Log Analytics workspace
- Ensures service-level telemetry isolation while centralizing storage

---

## Terraform Code Changes Required

### 1. Update Chain Module
**File:** `infra/terraform/modules/chain/main.tf`

**Change:**
- Remove dedicated Log Analytics Workspace (lines 232-240)
- Add parameter: `shared_log_analytics_workspace_id`
- Update Application Insights to use shared workspace

### 2. Update Publisher Module
**File:** `infra/terraform/modules/publisher/main.tf`

**Change:**
- Remove dedicated Log Analytics Workspace (lines 142-150)
- Add parameter: `shared_log_analytics_workspace_id`
- Update Application Insights to use shared workspace (line 157)

### 3. Update All Modules - Naming Convention
**Files:** All module `main.tf` files

**Change:**
```terraform
# OLD (inconsistent)
locals {
  name_prefix = "mys-${var.environment}-mystira-chain"
}

# NEW (standardized)
locals {
  name_prefix = "mys-${var.environment}-chain"
}
```

### 4. Update Shared Modules - Naming Convention
**Files:** `modules/shared/*/main.tf`

**Change:**
```terraform
# OLD
locals {
  name_prefix = "mystira-shared-pg-${var.environment}"
}

# NEW
locals {
  name_prefix = "mys-${var.environment}-postgres"
}
```

### 5. Update Dev Environment
**File:** `infra/terraform/environments/dev/main.tf`

**Change:**
```terraform
# Update resource group name (line 48)
resource "azurerm_resource_group" "main" {
  name     = "mys-dev-rg-san"  # was: mys-dev-mystira-rg-san
  # ...
}

# Update VNet name (line 60)
resource "azurerm_virtual_network" "main" {
  name     = "mys-dev-vnet-san"  # was: mys-dev-mystira-vnet-san
  # ...
}

# Update AKS name (line 260)
resource "azurerm_kubernetes_cluster" "main" {
  name       = "mys-dev-aks-san"  # was: mys-dev-mystira-aks-san
  # ...
}

# Update ACR name (line 128)
resource "azurerm_container_registry" "shared" {
  name     = "mysdevacr"  # was: mysprodacr (no hyphens allowed)
  # ...
}

# Pass shared logs to chain module
module "chain" {
  # ... existing config ...
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
}

# Pass shared logs to publisher module
module "publisher" {
  # ... existing config ...
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
}
```

---

## Implementation Plan

### Phase 1: Prepare Terraform Code (No Infrastructure Changes)
1. ✓ Create new branch: `feature/standardize-dev-resources`
2. Update all module naming conventions
3. Add `shared_log_analytics_workspace_id` to chain and publisher modules
4. Update dev environment to pass shared logs
5. Review and test terraform plan (should show destroy + recreate)

### Phase 2: Destroy Old Resources
1. Export all necessary secrets from Key Vaults
2. Backup any data from storage accounts
3. Note down all configuration values
4. Run: `terraform destroy -target=module.chain.azurerm_log_analytics_workspace.chain`
5. Run: `terraform destroy -target=module.publisher.azurerm_log_analytics_workspace.publisher`
6. Delete redundant email communication services manually (if Terraform doesn't manage them)

### Phase 3: Apply New Resources
1. Run: `terraform apply` with new naming convention
2. Verify all resources created successfully
3. Update Kubernetes configs to point to new resource names
4. Restore secrets to new Key Vaults
5. Test all services

### Phase 4: Cleanup and Validation
1. Verify no orphaned resources in Azure Portal
2. Update documentation
3. Update CI/CD pipelines if they reference resource names
4. Merge to main branch

---

## Estimated Cost Savings

| Resource | Current | Proposed | Monthly Savings |
|----------|---------|----------|-----------------|
| Log Analytics (3 → 1) | ~$150 | ~$50 | $100 |
| Email Services (3? → 1) | ~$50 | ~$25 | $25 |
| **Total** | | | **~$125/mo** |

---

## Questions to Answer Before Proceeding

1. **Email Communication Services:**
   - Which email services are actually needed?
   - Are there multiple instances due to failed deployments?

2. **Container Registry:**
   - Should ACR be truly shared across all environments, or per-environment?
   - Current code has `mysprodacr` in dev environment - is this intentional?

3. **Front Door:**
   - Is Front Door actively being used in dev?
   - The config file suggests it's optional (disabled by default)

4. **Static Web App:**
   - Saw `mys-dev-mystira-swa-eus2` in screenshots (East US 2)
   - Is this managed by Terraform? Not found in current code.

5. **Service-specific vs Shared App Insights:**
   - Keep service-specific Application Insights for better telemetry isolation?
   - Or consolidate to single shared instance?
   - **Recommendation:** Keep service-specific, connect to shared Log Analytics

---

## Next Steps

Please review this plan and answer:
1. Should I proceed with the Terraform code updates?
2. Do you want to keep service-specific Application Insights or consolidate?
3. Can you clarify the email service situation?
4. Is the Container Registry meant to be shared across all environments?

Once you confirm, I'll start implementing Phase 1 (Terraform code updates) on the current branch.
