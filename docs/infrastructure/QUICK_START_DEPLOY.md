# Quick Start: Deploy Infrastructure

Quick guide to deploy AKS clusters and infrastructure.

## Prerequisites Check

```bash
# Verify you're in the workspace root
pwd
# Should show: .../Mystira.workspace

# Check if Terraform is installed
terraform --version
# Should show: Terraform v1.5.0 or higher
```

## Step 1: Install Terraform (if needed)

### Windows (using Chocolatey)

```powershell
choco install terraform
```

### Windows (using winget)

```powershell
winget install HashiCorp.Terraform
```

### Windows (Manual)

1. Download from https://www.terraform.io/downloads
2. Extract to a folder (e.g., `C:\terraform`)
3. Add to PATH: `System Properties → Environment Variables → Path → Add C:\terraform`

### Verify Installation

```bash
terraform --version
```

## Step 2: Navigate to Correct Directory

```bash
# From workspace root (Mystira.workspace)
cd infra/terraform/environments/dev

# Verify you're in the right place
pwd
# Should show: .../Mystira.workspace/infra/terraform/environments/dev

ls
# Should show: main.tf and other terraform files
```

**Important**: The `infra` directory is a submodule. Make sure it's initialized:

```bash
# From workspace root
git submodule update --init --recursive
```

## Step 3: Initialize Terraform

```bash
cd infra/terraform/environments/dev
terraform init
```

This downloads the Azure provider and sets up the backend.

## Step 4: Review Deployment Plan

```bash
terraform plan
```

This shows what will be created without actually creating it.

**Expected resources**:

- Resource Group: `mystira-dev-rg`
- Virtual Network
- Subnets
- Azure Container Registry: `mystiraacr`
- Azure Kubernetes Service: `mystira-dev-aks`
- Key Vaults
- (And more...)

## Step 5: Deploy Infrastructure

```bash
terraform apply
```

Terraform will ask for confirmation. Type `yes` to proceed.

**⚠️ Important**: This will create real Azure resources and may incur costs!

## Step 6: Verify Deployment

### Check AKS Cluster

```bash
# List clusters
az aks list --output table

# Get cluster details
az aks show --name mystira-dev-aks --resource-group mystira-dev-rg

# Get cluster credentials for kubectl
az aks get-credentials --resource-group mystira-dev-rg --name mystira-dev-aks

# Verify kubectl can connect
kubectl cluster-info
kubectl get nodes
```

### Check in Azure Portal

1. Go to Azure Portal
2. Search for "Kubernetes services"
3. You should see `mystira-dev-aks` listed

## Troubleshooting

### "terraform: command not found"

Terraform is not installed or not in your PATH. See "Install Terraform" above.

### "No such file or directory: infra/terraform/environments/dev"

You're in the wrong directory. Make sure you're in the workspace root:

```bash
cd ~/repos/Mystira.workspace  # or wherever your workspace is
cd infra/terraform/environments/dev
```

### Submodule not initialized

```bash
# From workspace root
git submodule update --init --recursive
```

### Azure authentication errors

```bash
# Login to Azure
az login

# Set correct subscription
az account list --output table
az account set --subscription "Your Subscription Name"
```

### Terraform backend errors

The Terraform backend uses Azure Storage. Make sure:

1. The storage account `mystiraterraformstate` exists
2. The container `tfstate` exists
3. Your Azure credentials have access

If the backend doesn't exist, you may need to create it first or use a local backend for initial testing.

## Next Steps

After deploying dev environment:

1. **Deploy services to Kubernetes**: See [Deployment Guide](../SETUP.md#deployment)
2. **Deploy staging environment**: `cd ../staging` and repeat steps
3. **Configure CI/CD**: Ensure GitHub secrets are set up (see [Setup Guide](../SETUP.md#github-secrets-configuration))
