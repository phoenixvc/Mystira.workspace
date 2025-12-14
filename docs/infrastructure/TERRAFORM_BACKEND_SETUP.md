# Terraform Backend Setup

## Problem: Backend Storage Account Doesn't Exist

Terraform uses an Azure Storage Account to store state files. If you see this error:

```
Error: Failed to get existing workspaces: listing blobs: executing request:
Get "https://mystiraterraformstate.blob.core.windows.net/..."
dial tcp: lookup mystiraterraformstate.blob.core.windows.net: no such host
```

The storage account `mystiraterraformstate` doesn't exist yet. You need to create it first.

**Naming Convention**: Storage accounts follow the pattern `{org}{env}{description}` (no dashes, lowercase only) per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md). For example: `nlprodterraformstate` for NeuralLiquid prod Terraform state.

## Solution: Create Storage Account Manually

### Step 1: Create Resource Group for Terraform State

**Naming Convention**: Resource groups follow the pattern `{org}-{env}-{project}-rg-{region}` or `{org}-{env}-terraform-rg-{region}` for Terraform state storage per [ADR-0008](../architecture/adr/0008-azure-resource-naming-conventions.md).

```powershell
# Create resource group for Terraform state
az group create \
  --name mystira-terraform-state \
  --location eastus
```

### Step 2: Create Storage Account

**Naming Convention**: Storage accounts use pattern `mystira{description}` (no dashes, lowercase only) per [ADR-0008](../architecture/adr/0008-azure-resource-naming-conventions.md).

```powershell
# Create storage account for Terraform state
az storage account create \
  --name mystiraterraformstate \
  --resource-group mystira-terraform-state \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2
```

**Important**: Storage account names must be globally unique, 3-24 characters, lowercase alphanumeric only (no dashes allowed). The standard name is `mysprodterraformstate` per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md). If this name is taken, choose a different name following the pattern and update the Terraform backend configuration.

### Step 3: Create Storage Container

```powershell
# Get storage account key
$STORAGE_KEY = az storage account keys list \
  --resource-group mystira-terraform-state \
  --account-name mystiraterraformstate \
  --query "[0].value" -o tsv

# Create container for Terraform state
az storage container create \
  --name tfstate \
  --account-name mystiraterraformstate \
  --account-key $STORAGE_KEY
```

### Step 4: Verify Setup

```powershell
# Verify storage account exists
az storage account show --name mystiraterraformstate --resource-group mystira-terraform-state

# Verify container exists
az storage container show --name tfstate --account-name mystiraterraformstate --account-key $STORAGE_KEY
```

### Step 5: Retry Terraform Init

Now go back to your Terraform directory and retry:

```powershell
cd C:\Users\smitj\repos\Mystira.workspace\infra\terraform\environments\dev
terraform init
```

## Alternative: Use Local Backend (For Testing)

If you want to test Terraform without setting up the backend first, you can temporarily use a local backend:

### Step 1: Modify Backend Configuration

Edit `infra/terraform/environments/dev/main.tf` and comment out the backend block:

```hcl
terraform {
  required_version = ">= 1.5.0"

  # Temporarily disable remote backend
  # backend "azurerm" {
  #   resource_group_name  = "mys-prod-terraform-rg-eus"
  #   storage_account_name = "mysprodterraformstate"
  #   container_name       = "tfstate"
  #   key                  = "dev/terraform.tfstate"
  #   use_azuread_auth     = true
  # }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}
```

### Step 2: Initialize with Local Backend

```powershell
terraform init
```

**⚠️ Warning**: Local backend is fine for testing, but for production you should use the remote backend. State files are stored locally and won't be shared with your team.

### Step 3: Migrate to Remote Backend Later

Once the storage account is created, you can migrate:

1. Uncomment the backend block in `main.tf`
2. Run `terraform init -migrate-state`
3. Confirm migration when prompted

## Storage Account Naming

If the name `mystiraterraformstate` is already taken, you'll need to:

1. **Choose a different name** (must be globally unique, 3-24 characters, lowercase):

   ```powershell
   # Example: mystiraterraformstate123
   az storage account create --name mystiraterraformstate123 ...
   ```

2. **Update Terraform backend configuration**:

   Edit `infra/terraform/environments/dev/main.tf`:

   ```hcl
   backend "azurerm" {
     resource_group_name  = "mys-prod-terraform-rg-eus"
     storage_account_name = "mystiraterraformstate123"  # Updated name
     container_name       = "tfstate"
     key                  = "dev/terraform.tfstate"
     use_azuread_auth     = true
   }
   ```

   Also update:
   - `infra/terraform/environments/staging/main.tf`
   - `infra/terraform/environments/prod/main.tf`

## Complete Setup Script

Here's a complete PowerShell script to set up the backend:

```powershell
# Set variables
$RESOURCE_GROUP = "mys-prod-terraform-rg-eus"
$STORAGE_ACCOUNT = "mystiraterraformstate"
$LOCATION = "eastus"
$CONTAINER = "tfstate"

# Login to Azure (if not already)
az login

# Set subscription (if needed)
# az account set --subscription "Your Subscription Name"

# Create resource group
Write-Host "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create storage account
Write-Host "Creating storage account..."
az storage account create `
  --name $STORAGE_ACCOUNT `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --sku Standard_LRS `
  --kind StorageV2

# Get storage account key
Write-Host "Getting storage account key..."
$STORAGE_KEY = az storage account keys list `
  --resource-group $RESOURCE_GROUP `
  --account-name $STORAGE_ACCOUNT `
  --query "[0].value" -o tsv

# Create container
Write-Host "Creating container..."
az storage container create `
  --name $CONTAINER `
  --account-name $STORAGE_ACCOUNT `
  --account-key $STORAGE_KEY

Write-Host "Backend setup complete!"
Write-Host "Now run: cd infra\terraform\environments\dev && terraform init"
```

## Troubleshooting

### "Storage account name is not available"

The name `mystiraterraformstate` is already taken. Choose a different name and update all backend configurations.

### "Resource group already exists"

The resource group exists. This is fine - proceed with creating the storage account.

### "Container already exists"

The container exists. This is fine - Terraform init should work now.

### Still getting DNS errors after creating storage account

1. Verify the storage account exists:

   ```powershell
   az storage account show --name mysprodterraformstate --resource-group mys-prod-terraform-rg-eus
   ```

2. Check you're using the correct subscription:

   ```powershell
   az account show
   ```

3. Try `terraform init` again - DNS propagation can take a few minutes.

## Next Steps

After the backend is set up:

1. Run `terraform init` - should succeed now
2. Run `terraform plan` - review what will be created
3. Run `terraform apply` - deploy infrastructure

## Related Documentation

- [Quick Start Deployment Guide](./QUICK_START_DEPLOY.md)
- [Infrastructure Guide](./infrastructure.md)
- [Setup Guide](../SETUP.md)
