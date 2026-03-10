> ARCHIVED — December 2025. Superseded by docs/planning/PLAN.md

# Terraform Structure Migration Plan

## Overview

This document outlines the migration from a monolithic Terraform structure to a product-based structure with separate state files and Terragrunt for dependency management.

## Current State

```
infra/terraform/
├── environments/
│   ├── dev/main.tf      # ~600 lines, all products combined
│   ├── staging/main.tf
│   └── prod/main.tf
└── modules/
    ├── mystira-app/
    ├── story-generator/
    ├── admin-api/
    ├── admin-ui/
    └── shared/
```

**Problems:**

- Single state file per environment = deployment bottleneck
- One team's change can block another
- Long `terraform plan` times
- Risk of state corruption affects all products
- No independent deployment cycles

## Target State

```
infra/terraform/
├── terragrunt.hcl                    # Root config
├── shared-infra/                     # Layer 0: Shared resources
│   ├── terragrunt.hcl
│   └── environments/
│       ├── dev/
│       │   ├── terragrunt.hcl
│       │   └── main.tf
│       ├── staging/
│       └── prod/
├── products/                         # Layer 1: Applications
│   ├── mystira-app/
│   │   ├── terragrunt.hcl
│   │   └── environments/
│   │       ├── dev/
│   │       │   ├── terragrunt.hcl
│   │       │   └── main.tf
│   │       ├── staging/
│   │       └── prod/
│   ├── story-generator/
│   ├── admin/                        # Combined admin-api + admin-ui
│   ├── publisher/
│   └── chain/
└── modules/                          # Shared modules (unchanged)
```

## Migration Phases

### Phase 1: Preparation (No Downtime)

**Duration:** 1-2 days

1. **Add Terragrunt files** (this PR)
   - Create root `terragrunt.hcl`
   - Create product-specific configs
   - No changes to existing Terraform

2. **Update CI/CD pipelines**
   - Add Terragrunt installation step
   - Create parallel pipeline jobs per product

3. **Document rollback procedure**

### Phase 2: State Splitting (Requires Maintenance Window)

**Duration:** 2-4 hours per environment (start with dev)

#### Step 2.1: Backup Current State

```bash
# For each environment
cd infra/terraform/environments/dev
terraform state pull > backup-$(date +%Y%m%d).tfstate
```

#### Step 2.2: Create New State Files

```bash
# Create separate state files in Azure Storage
az storage blob directory create \
  --account-name mysterraformstate \
  --container-name tfstate \
  --directory-path shared-infra

az storage blob directory create \
  --account-name mysterraformstate \
  --container-name tfstate \
  --directory-path mystira-app

# Repeat for each product...
```

#### Step 2.3: Move Resources to New States

Use `terraform state mv` to migrate resources:

```bash
# Example: Move shared PostgreSQL to shared-infra state
terraform state mv \
  -state=current.tfstate \
  -state-out=shared-infra/dev.tfstate \
  'module.shared_postgresql'

# Move story-generator resources
terraform state mv \
  -state=current.tfstate \
  -state-out=story-generator/dev.tfstate \
  'module.story_generator'
```

**Resource Mapping:**

| Current Module                     | Target State File             |
| ---------------------------------- | ----------------------------- |
| `module.shared_postgresql`         | `shared-infra/dev.tfstate`    |
| `module.shared_redis`              | `shared-infra/dev.tfstate`    |
| `module.shared_cosmos_db`          | `shared-infra/dev.tfstate`    |
| `module.shared_storage`            | `shared-infra/dev.tfstate`    |
| `module.shared_azure_ai`           | `shared-infra/dev.tfstate`    |
| `module.shared_servicebus`         | `shared-infra/dev.tfstate`    |
| `module.shared_container_registry` | `shared-infra/dev.tfstate`    |
| `module.shared_monitoring`         | `shared-infra/dev.tfstate`    |
| `module.dns`                       | `shared-infra/dev.tfstate`    |
| `module.front_door`                | `shared-infra/dev.tfstate`    |
| `module.mystira_app`               | `mystira-app/dev.tfstate`     |
| `module.story_generator`           | `story-generator/dev.tfstate` |
| `module.admin_api`                 | `admin/dev.tfstate`           |
| `module.admin_ui`                  | `admin/dev.tfstate`           |
| `module.publisher`                 | `publisher/dev.tfstate`       |
| `module.chain`                     | `chain/dev.tfstate`           |

#### Step 2.4: Verify State Integrity

```bash
# For each new state file
cd products/mystira-app/environments/dev
terragrunt plan  # Should show no changes
```

### Phase 3: Update CI/CD (No Downtime)

Update GitHub Actions workflows:

```yaml
# .github/workflows/terraform-product.yml
jobs:
  plan:
    strategy:
      matrix:
        product:
          [shared-infra, mystira-app, story-generator, admin, publisher, chain]
    steps:
      - uses: actions/checkout@v4
      - uses: hashicorp/setup-terraform@v3
      - uses: autero1/action-terragrunt@v3

      - name: Terragrunt Plan
        working-directory: infra/terraform/${{ matrix.product }}/environments/${{ github.event.inputs.environment }}
        run: terragrunt plan
```

### Phase 4: Cleanup (No Downtime)

1. Remove old monolithic environment files
2. Update documentation
3. Archive backup state files (keep for 90 days)

## Rollback Procedure

If issues occur during migration:

```bash
# 1. Stop all Terraform/Terragrunt operations

# 2. Restore from backup
cd infra/terraform/environments/dev
terraform state push backup-YYYYMMDD.tfstate

# 3. Revert Terragrunt configs (git revert)

# 4. Resume using old structure
```

## Risk Mitigation

| Risk                            | Mitigation                                   |
| ------------------------------- | -------------------------------------------- |
| State corruption                | Backup before each change, test in dev first |
| Resource drift during migration | Short maintenance window, freeze deployments |
| Dependency issues               | Terragrunt handles cross-state dependencies  |
| CI/CD failures                  | Keep old pipeline until verified             |

## Validation Checklist

- [ ] All state files created successfully
- [ ] `terragrunt plan` shows no changes for each product
- [ ] CI/CD pipeline runs successfully
- [ ] Cross-product dependencies work (e.g., apps can access shared DB)
- [ ] Rollback tested in dev environment

## Timeline

| Phase | Environment | Duration  | Dependencies      |
| ----- | ----------- | --------- | ----------------- |
| 1     | All         | 1-2 days  | None              |
| 2     | Dev         | 2-4 hours | Phase 1           |
| 2     | Staging     | 2-4 hours | Dev validated     |
| 2     | Prod        | 2-4 hours | Staging validated |
| 3     | All         | 1 day     | Phase 2 complete  |
| 4     | All         | 1 day     | Phase 3 verified  |

**Total estimated time:** 1-2 weeks (conservative)
