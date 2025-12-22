# Quick Start: Deploy Infrastructure

Quick guide to deploy AKS clusters and infrastructure.

## Prerequisites Check

```powershell
# Verify you're in the workspace root
Get-Location
# Should show: C:\Users\smitj\repos\Mystira.workspace

# Check if Terraform is installed
terraform --version
# Should show: Terraform v1.5.0 or higher
```

### Service Principal Permissions (CI/CD)

If deploying via GitHub Actions, ensure the service principal has the required permissions:

| Permission | Purpose | When Required |
|------------|---------|---------------|
| **Contributor** | Create/manage Azure resources | Always |
| **User Access Administrator** | Assign RBAC roles to identities | When using managed identities |
| **Azure AD Application.ReadWrite.All** | Manage app registrations | When using Entra External ID |
| **Azure AD DelegatedPermissionGrant.ReadWrite.All** | Manage OAuth2 permission grants | When using Entra External ID |
| **Storage Blob Data Contributor** | Access Terraform state | Always |

See [Azure Setup Guide](../../infra/azure-setup.md#step-2-required-permissions) for complete setup instructions.

## Step 1: Install Terraform (if needed)

### Windows (using winget)

```powershell
winget install HashiCorp.Terraform
```

### Windows (using Chocolatey)

```powershell
choco install terraform
```

### Windows (Manual)

1. Download from https://www.terraform.io/downloads
2. Extract to a folder (e.g., `C:\terraform`)
3. Add to PATH: `System Properties → Environment Variables → Path → Add C:\terraform`

### Verify Installation

```powershell
terraform --version
```

## Step 2: Initialize Submodule (IMPORTANT!)

The `infra` directory is a **Git submodule** and must be initialized first:

```powershell
# From workspace root
git submodule update --init --recursive

# Verify infra directory exists
Test-Path infra\terraform\environments\dev\main.tf
# Should return: True
```

If you get an error or the directory doesn't exist, the submodule might not be initialized.

## Step 3: Navigate to Terraform Directory

**IMPORTANT**: You must navigate to the Terraform environment directory, not run from workspace root!

```powershell
# From workspace root
cd infra\terraform\environments\dev

# Verify you're in the right place
Get-Location
# Should show: ...\Mystira.workspace\infra\terraform\environments\dev

# Verify terraform files exist
Get-ChildItem *.tf
# Should show: main.tf and other .tf files
```

**Common Mistake**: Running `terraform` commands from the workspace root won't work - Terraform files are in `infra/terraform/environments/dev/`

## Step 4: Initialize Terraform

```powershell
# Make sure you're in infra\terraform\environments\dev
terraform init
```

This downloads the Azure provider and sets up the backend.

**Expected output**:

```
Initializing the backend...
Initializing provider plugins...
Terraform has been successfully initialized!
```

## Step 5: Review Deployment Plan

```powershell
terraform plan
```

This shows what will be created without actually creating it.

**Expected resources** (following [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md)):

**Note**: Existing resources use legacy naming and are kept as-is. New resources should follow the new convention `[org]-[env]-[project]-[type]-[region]`.

**Resource Names** (per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md)):

- Resource Group: `mys-dev-core-rg-eus`
- Virtual Network: `mys-dev-core-vnet-eus`
- Subnets: `chain-subnet`, `publisher-subnet`, `aks-subnet`, etc.
- Azure Container Registry: `myssharedacr` (shared)
- Azure Kubernetes Service: `mys-dev-core-aks-eus`
- Key Vaults: `mys-dev-mystira-{component}-kv-eus`
- Shared PostgreSQL: `mys-dev-mystira-pg-eus`
- Shared Redis: `mys-dev-mystira-cache-eus`
- (And more...)

## Step 6: Deploy Infrastructure

```powershell
terraform apply
```

Terraform will ask for confirmation. Type `yes` to proceed.

**⚠️ Important**: This will create real Azure resources and may incur costs!

## Step 7: Verify Deployment

### Check AKS Cluster

```powershell
# List clusters
az aks list --output table

# Get cluster details
az aks show --name mys-dev-core-aks-eus --resource-group mys-dev-core-rg-eus

# Get cluster credentials for kubectl
az aks get-credentials --resource-group mys-dev-core-rg-eus --name mys-dev-core-aks-eus

# Verify kubectl can connect
kubectl cluster-info
kubectl get nodes
```

### Check in Azure Portal

1. Go to Azure Portal
2. Search for "Kubernetes services" (NOT "Kubernetes center")
3. You should see `mys-dev-core-aks-eus` listed

## Quick Reference: Correct Paths

```powershell
# Workspace root
C:\Users\smitj\repos\Mystira.workspace

# Terraform dev environment
C:\Users\smitj\repos\Mystira.workspace\infra\terraform\environments\dev

# Terraform staging environment
C:\Users\smitj\repos\Mystira.workspace\infra\terraform\environments\staging

# Terraform prod environment
C:\Users\smitj\repos\Mystira.workspace\infra\terraform\environments\prod
```

## Troubleshooting

### "terraform: command not found"

Terraform is not installed or not in your PATH. See "Install Terraform" above.

### "No configuration files" Error

**You're in the wrong directory!** You must be in `infra\terraform\environments\dev` (or staging/prod).

```powershell
# Check where you are
Get-Location

# Navigate to correct directory
cd infra\terraform\environments\dev

# Verify terraform files exist
Get-ChildItem *.tf
```

### "No such file or directory: infra/terraform/environments/dev"

The submodule is not initialized:

```powershell
# From workspace root
git submodule update --init --recursive

# Verify infra exists
Test-Path infra\terraform\environments\dev\main.tf
```

### Azure authentication errors

```powershell
# Login to Azure
az login

# Set correct subscription
az account list --output table
az account set --subscription "Your Subscription Name"
```

### Permission errors (CI/CD)

If you see errors like `AuthorizationFailed` or `Authorization_RequestDenied`:

1. **Check the workflow summary** - The "Check Service Principal Permissions" step shows missing permissions
2. **Grant required permissions** - See [Azure Setup Guide](../../infra/azure-setup.md#step-2-required-permissions)
3. **Common permission issues:**
   - `AuthorizationFailed` on role assignments → Need **User Access Administrator** role
   - `Authorization_RequestDenied` on AD operations → Need **Azure AD permissions**
   - `AuthorizationPermissionMismatch` on state → Need **Storage Blob Data Contributor**

### Terraform backend errors

The Terraform backend uses Azure Storage. Make sure:

1. The storage account `myssharedtfstatesan` exists
2. The container `tfstate` exists
3. Your Azure credentials have access

If the backend doesn't exist, you may need to create it first or use a local backend for initial testing.

## Next Steps

After deploying dev environment:

1. **Deploy services to Kubernetes**: See [Deployment Guide](../guides/setup.md#deployment)
2. **Deploy staging environment**: `cd ..\staging` and repeat steps
3. **Configure CI/CD**: Ensure GitHub secrets are set up (see [Setup Guide](../guides/setup.md#github-secrets-configuration))
