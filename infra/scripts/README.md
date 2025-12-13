# Infrastructure Scripts

This directory contains scripts for managing infrastructure operations.

## Bootstrap Scripts

### bootstrap-terraform-backend.sh

Creates the Azure Storage Account and Container for storing Terraform state files.

**Usage:**

```bash
./infra/scripts/bootstrap-terraform-backend.sh
```

**Prerequisites:**
- Azure CLI installed and logged in (`az login`)
- Appropriate Azure permissions to create resource groups and storage accounts

**What it does:**
1. Creates resource group `mystira-terraform-state` (if it doesn't exist)
2. Creates storage account `mystiraterraformstate` (if it doesn't exist)
3. Creates container `tfstate` (if it doesn't exist)

**Environment Variables:**
- `AZURE_LOCATION` - Azure region for the storage account (default: `eastus`)
- `AZURE_STORAGE_SKU` - Storage account SKU (default: `Standard_LRS`, other options: `Standard_GRS`, `Standard_RAGRS`, `Standard_ZRS`, etc.)

**Notes:**
- This script is idempotent - it can be run multiple times safely
- The script is automatically executed by the GitHub Actions workflow before `terraform init`
- The storage account uses:
  - Standard LRS redundancy
  - TLS 1.2 minimum
  - Blob encryption enabled
  - Public access disabled
