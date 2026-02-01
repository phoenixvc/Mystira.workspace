# Terraform State Migration Execution Guide

This guide walks through the wave-based migration from monolithic to product-based Terraform state structure.

## Prerequisites

1. **Azure CLI** logged in with appropriate permissions
2. **Terraform** v1.5.0+ installed
3. **Terragrunt** v0.55.0+ installed
4. **Backup access** to Azure Storage for state files

## Migration Overview

```
Current State:
environments/dev/terraform.tfstate        (monolithic)
environments/staging/terraform.tfstate    (monolithic)
environments/prod/terraform.tfstate       (monolithic)

Target State:
shared-infra/environments/dev/terraform.tfstate
products/mystira-app/environments/dev/terraform.tfstate
products/story-generator/environments/dev/terraform.tfstate
products/admin/environments/dev/terraform.tfstate
products/publisher/environments/dev/terraform.tfstate
products/chain/environments/dev/terraform.tfstate
(repeated for staging and prod)
```

---

## Wave 1: Preparation and Backup

### Step 1.1: Create Backup
```bash
cd infra/terraform

# Create timestamped backup directory
BACKUP_DIR="backups/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Backup dev state
cd environments/dev
terraform state pull > "../../$BACKUP_DIR/dev_backup.tfstate"
cd ../..

# Verify backup
ls -la "$BACKUP_DIR"
```

### Step 1.2: Run Migration Analysis
```bash
# Make the script executable
chmod +x scripts/migrate-state.sh

# Run in dry-run mode (won't execute actual moves)
./scripts/migrate-state.sh dev

# Review generated files
cat .shared_infra_resources.txt
cat .story_generator_resources.txt
cat .admin_resources.txt
cat .publisher_resources.txt
cat .chain_resources.txt
```

### Step 1.3: Review Resource Categorization
The script categorizes resources into products. Review and adjust the `.*.txt` files if needed:

| File | Contains |
|------|----------|
| `.shared_infra_resources.txt` | VNet, AKS, PostgreSQL, Redis, ACR, monitoring |
| `.story_generator_resources.txt` | Story Generator module resources |
| `.admin_resources.txt` | Admin API and Admin UI resources |
| `.publisher_resources.txt` | Publisher module resources |
| `.chain_resources.txt` | Chain module resources |

---

## Wave 2: Migrate Shared Infrastructure (Dev)

### Step 2.1: Initialize Shared-Infra
```bash
cd infra/terraform/shared-infra/environments/dev

# Initialize with new backend
export TF_VAR_environment=dev
terragrunt init --terragrunt-non-interactive
```

### Step 2.2: Import Existing Resources
```bash
# For each resource in .shared_infra_resources.txt, import to new state
# Example:
terragrunt import 'module.shared_postgresql.azurerm_postgresql_flexible_server.main' \
  '/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/xxx'
```

### Step 2.3: Validate (No Changes Expected)
```bash
terragrunt plan
# Should show "No changes" if import was successful
```

---

## Wave 3: Migrate Products (Dev)

### Step 3.1: Migrate Story Generator
```bash
cd infra/terraform/products/story-generator/environments/dev

export TF_VAR_environment=dev
terragrunt init --terragrunt-non-interactive

# Import resources from .story_generator_resources.txt
# terragrunt import 'resource.name' 'azure-resource-id'

terragrunt plan
```

### Step 3.2: Migrate Admin
```bash
cd infra/terraform/products/admin/environments/dev

export TF_VAR_environment=dev
terragrunt init --terragrunt-non-interactive

# Import resources
terragrunt plan
```

### Step 3.3: Migrate Publisher
```bash
cd infra/terraform/products/publisher/environments/dev

export TF_VAR_environment=dev
terragrunt init --terragrunt-non-interactive

# Import resources
terragrunt plan
```

### Step 3.4: Migrate Chain
```bash
cd infra/terraform/products/chain/environments/dev

export TF_VAR_environment=dev
terragrunt init --terragrunt-non-interactive

# Import resources
terragrunt plan
```

---

## Wave 4: Validation

### Step 4.1: Run Full Validation
```bash
cd infra/terraform

# Validate all products
terragrunt run-all validate --terragrunt-non-interactive
```

### Step 4.2: Plan All (Verify No Changes)
```bash
# Plan all - should show no changes if migration was correct
export TF_VAR_environment=dev
terragrunt run-all plan --terragrunt-non-interactive
```

### Step 4.3: Test Dependency Chain
```bash
# Destroy-plan a product (don't apply!) to verify dependencies
cd products/story-generator/environments/dev
terragrunt plan -destroy
# Verify it doesn't try to destroy shared resources
```

---

## Wave 5: Migrate Staging (Repeat Waves 2-4)

```bash
export TF_VAR_environment=staging

# Backup
cd environments/staging
terraform state pull > "../../backups/staging_backup.tfstate"
cd ../..

# Initialize and import each product
cd shared-infra/environments/staging
terragrunt init
# Import resources...

# Repeat for all products
```

---

## Wave 6: Migrate Production (Repeat Waves 2-4)

```bash
export TF_VAR_environment=prod

# Backup first!
cd environments/prod
terraform state pull > "../../backups/prod_backup.tfstate"
cd ../..

# Production migration follows same pattern
# Consider doing this during a maintenance window
```

---

## Rollback Procedure

If issues occur during migration:

### Restore from Backup
```bash
cd infra/terraform

# Get backup directory
BACKUP_DIR=$(cat .last_backup_dir)

# Restore state
cd environments/dev
terraform state push "../../$BACKUP_DIR/dev_backup.tfstate"
```

### Reset New State Files
```bash
# Delete new state files in Azure Storage
az storage blob delete \
  --account-name myssharedtfstatesan \
  --container-name tfstate \
  --name "shared-infra/dev.tfstate"
```

---

## Post-Migration Checklist

- [ ] All products plan shows "No changes"
- [ ] Old monolithic state can be archived
- [ ] CI/CD pipelines updated to use new workflows
- [ ] Documentation updated
- [ ] Team trained on new structure

---

## CI/CD Usage

After migration, use the new workflows:

### Validate on PR
```yaml
# Automatically runs on PR changes to infra/terraform/**
# See: .github/workflows/infra-terragrunt-validate.yml
```

### Manual Deployment
```bash
# Via GitHub Actions UI:
# 1. Go to Actions > "[CD] Infrastructure - Terragrunt Deploy"
# 2. Select environment, product, and action
# 3. Run workflow
```

### CLI Deployment
```bash
# Deploy shared-infra
cd infra/terraform/shared-infra/environments/dev
terragrunt apply

# Deploy a product
cd infra/terraform/products/story-generator/environments/dev
terragrunt apply

# Deploy all
cd infra/terraform
terragrunt run-all apply
```

---

## Support

- **Migration issues**: Check backup files and rollback if needed
- **Dependency errors**: Ensure shared-infra is deployed before products
- **State conflicts**: Use `terraform state rm` to remove orphaned resources
