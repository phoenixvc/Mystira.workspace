# Azure Resources Migration to v2.2 Naming Convention

## Migration Summary

All infrastructure resources have been updated to comply with Azure Naming Convention v2.2:
**Pattern:** `[org]-[env]-[project]-[type]-[region]`

**Last Updated:** December 2025
**Status:** ✅ Complete - All environments aligned
**Current Branch:** `claude/update-readme-ci-badge-jWXQm`

---

## 📋 Recent Updates (December 2025)

### Environment Alignment
- **Staging/Prod aligned with Dev**: Added missing admin-api module, subnets, PostgreSQL AAD auth
- **Workload Identity**: All services (story-generator, publisher, chain, admin-api) now have `workload_identities` configured
- **Microsoft Entra External ID**: Module added to staging/prod (disabled until External ID tenant created)
- **PostgreSQL AAD Auth**: `aad_auth_enabled` and `aad_admin_identities` added for passwordless database access

### Kubernetes Standardization
- **Resource Naming**: All K8s resources standardized to `mys-*` prefix (deployments, services, configmaps, HPAs)
- **Kustomization Patches**: Fixed all overlay patch targets to match actual resource names in dev/staging/prod
- **ServiceAccounts**: Consolidated to `service-accounts.yaml`, removed duplicates from deployment files
- **Image Registry**: Updated to `myssharedacr.azurecr.io` across all manifests

### New Terraform Modules
- **Admin API Module** (`infra/terraform/modules/admin-api/`): Managed identity, Key Vault, Application Insights

---

## ✅ Completed Changes

### 1. **Terraform Modules** (7 modules)

All modules updated with new naming pattern and shared resources:

| Module            | Old Pattern                  | New Pattern             | Changes                                       |
| ----------------- | ---------------------------- | ----------------------- | --------------------------------------------- |
| Chain             | `mys-{env}-mystira-chain`    | `mys-{env}-chain`       | Removed "mystira", added shared Log Analytics |
| Publisher         | `mys-{env}-mystira-pub`      | `mys-{env}-publisher`   | Removed "mystira", added shared Log Analytics |
| Story-Generator   | `mys-{env}-mystira-sg`       | `mys-{env}-story`       | Changed sg→story, removed "mystira"           |
| Shared PostgreSQL | `mystira-shared-pg-{env}`    | `mys-{env}-core-db`     | Reversed order, standardized                  |
| Shared Redis      | `mystira-shared-redis-{env}` | `mys-{env}-core-cache`  | Reversed order, standardized                  |
| Shared Monitoring | `mystira-shared-mon-{env}`   | `mys-{env}-core-log/ai` | Reversed order, standardized                  |

**Key Vault Naming (Service-Specific):**

- Publisher: `mys-{env}-pub-kv-{region}`
- Chain: `mys-{env}-chain-kv-{region}`
- Story-Generator: `mys-{env}-story-kv-{region}`

**Rationale:** Separate Key Vaults per service for better security isolation, RBAC management, and zero-trust compliance.

---

### 2. **Environment Configurations** (3 environments)

All environments updated to use **South Africa North** region and new naming:

| Resource Type      | Dev                     | Staging                     | Prod                     |
| ------------------ | ----------------------- | --------------------------- | ------------------------ |
| Resource Group     | `mys-dev-core-rg-san`   | `mys-staging-core-rg-san`   | `mys-prod-core-rg-san`   |
| Virtual Network    | `mys-dev-core-vnet-san` | `mys-staging-core-vnet-san` | `mys-prod-core-vnet-san` |
| AKS Cluster        | `mys-dev-core-aks-san`  | `mys-staging-core-aks-san`  | `mys-prod-core-aks-san`  |
| Container Registry | `myssharedacr` (shared) | `myssharedacr` (shared)     | `myssharedacr` (shared)  |
| PostgreSQL         | `mys-dev-core-db`       | `mys-staging-core-db`       | `mys-prod-core-db`       |
| Redis              | `mys-dev-core-cache`    | `mys-staging-core-cache`    | `mys-prod-core-cache`    |
| Log Analytics      | `mys-dev-core-log`      | `mys-staging-core-log`      | `mys-prod-core-log`      |

---

### 3. **GitHub Workflows** (6 files)

All CI/CD pipelines updated:

- **`infra-deploy.yml`** - Core deployment workflow - Updated: ACR, resource groups, AKS, VNets, DNS zone RG
  - Region: All environments → `southafricanorth`

- **Component CI Workflows** (`chain-ci.yml`, `publisher-ci.yml`, `story-generator-ci.yml`)
  - Updated: ACR name `mysprodacr` → `myssharedacr`

- **Release Workflows** (`production-release.yml`, `staging-release.yml`)
  - Updated: Resource groups and AKS cluster names
  - Region: Changed from `eastus` → `southafricanorth`

---

### 4. **Kubernetes Manifests** (9 files)

All K8s configurations updated with new ACR and Key Vault references:

**Base Manifests:**

- `chain/kustomization.yaml` - ACR updated
- `publisher/kustomization.yaml` - ACR updated
- `story-generator/kustomization.yaml` - ACR updated

**Environment Overlays:**

- **Dev**: ACR + Key Vault URLs updated
  - Publisher: `https://mys-dev-pub-kv-san.vault.azure.net/`
  - Story-Generator: `https://mys-dev-story-kv-san.vault.azure.net/`

- **Staging**: ACR + Key Vault URLs + region updated
  - Publisher: `https://mys-staging-pub-kv-san.vault.azure.net/`
  - Story-Generator: `https://mys-staging-story-kv-san.vault.azure.net/`

- **Prod**: ACR + Key Vault URLs + region updated
  - Publisher: `https://mys-prod-pub-kv-san.vault.azure.net/`
  - Story-Generator: `https://mys-prod-story-kv-san.vault.azure.net/`

---

### 5. **Scripts** (2 files)

Infrastructure automation scripts updated:

- **`bootstrap-infra.sh`**
  - ACR name: `mysprodacr` → `myssharedacr`

- **`debug-certificates.sh`**
  - Resource group: `mys-dev-mystira-rg-san` → `mys-dev-core-rg-san`
  - AKS cluster: `mys-dev-mystira-aks-san` → `mys-dev-core-aks-san`

---

## 📋 Architecture Decisions

### Shared vs Separate Resources

| Resource               | Strategy                                 | Rationale                                                      |
| ---------------------- | ---------------------------------------- | -------------------------------------------------------------- |
| **Container Registry** | **Shared** across all environments       | Single source of truth for images, use tags for env separation |
| **Key Vaults**         | **Separate** per service per environment | Security isolation, better RBAC, zero-trust compliance         |
| **Log Analytics**      | **Shared** per environment               | Cost optimization, consolidated monitoring per env             |
| **PostgreSQL**         | **Shared** per environment               | Cost optimization, use separate databases per service          |
| **Redis**              | **Shared** per environment               | Cost optimization, use keyspace separation                     |

---

## 🔒 Security Improvements

1. **Service-Specific Key Vaults**
   - Each service has its own vault
   - Managed Identity grants access only to service's vault
   - Limits blast radius on compromise

2. **Consolidated Monitoring**
   - Single Log Analytics workspace per environment
   - Easier threat detection and correlation
   - Reduced cost (~$300/month savings)

3. **Zero-Trust Compliance**
   - Services can only access their own secrets
   - Principle of least privilege enforced
   - Clear audit trails per service

---

## 💰 Cost Impact

### Before (Old Naming):

- 9 Log Analytics workspaces (3 per service × 3 environments)
- Estimated cost: ~$450/month

### After (New Naming):

- 3 Log Analytics workspaces (1 per environment)
- Estimated cost: ~$150/month
- **Savings: ~$300/month (~$3,600/year)**

---

## ✅ Validation & Testing

### Automated Validation (`infra-validate.yml`)

- ✅ Terraform formatting and validation (all environments)
- ✅ Kubernetes manifests with kubectl dry-run
- ✅ Docker file linting with hadolint
- ✅ Security scanning with Checkov
- ✅ Cost estimation with Infracost

### Deployment Workflow (`infra-deploy.yml`)

Comprehensive deployment pipeline:

1. Validates all prerequisites (backend, ACR, DNS, AKS)
2. Bootstraps missing infrastructure
3. Terraform plan and apply
4. Builds and pushes Docker images
5. Deploys to Kubernetes with cert-manager
6. Configures DNS records
7. Verifies deployments

---

## 📝 Submodules

**No changes required** in application code submodules:

- Applications use environment variables and Key Vault for configuration
- No hardcoded resource names in application code
- All configuration is external (Kubernetes ConfigMaps)

Submodules:

- `Mystira.Chain` (Python)
- `Mystira.Publisher` (Node.js)
- `Mystira.StoryGenerator` (.NET)
- `Mystira.App`, `Mystira.DevHub`, `Mystira.Admin.Api`, `Mystira.Admin.UI`

---

## 🚀 Next Steps

1. **Deploy to Dev Environment**

   ```bash
   # Run infrastructure deployment
   gh workflow run infra-deploy.yml \
     --ref claude/standardize-dev-resources-cT39Z \
     -f environment=dev \
     -f components=all
   ```

2. **Verify Resources Created**

   ```bash
   # Check resource groups
   az group list --query "[?starts_with(name, 'mys-')].name" -o table

   # Check AKS clusters
   az aks list --query "[].{Name:name, RG:resourceGroup, Region:location}" -o table

   # Check container registry
   az acr list --query "[].{Name:name, LoginServer:loginServer}" -o table
   ```

3. **Test Application Deployments**
   - Verify pods are running: `kubectl get pods -n mys-dev`
   - Check cert-manager certificates: `kubectl get certificates -n mys-dev`
   - Test endpoints: `https://dev.publisher.mystira.app`

4. **Update Documentation** (Optional)
   - Architecture diagrams
   - Deployment guides
   - ADR documents

---

## 📚 Reference

### Commits

- Module refactor: All Terraform modules updated to v2.2
- Environment updates: Dev, staging, prod configurations
- Region migration: All environments → South Africa North
- Workflow updates: All CI/CD pipelines and K8s manifests

### Files Modified

- Terraform modules: 7 files
- Environment configs: 3 files
- GitHub workflows: 6 files
- Kubernetes manifests: 9 files
- Scripts: 2 files
- **Total: 27 files**

---

**Migration Status:** ✅ **COMPLETE**  
**Branch:** `claude/update-readme-ci-badge-jWXQm`  
**Ready for:** Deployment and testing
