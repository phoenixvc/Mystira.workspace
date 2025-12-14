# Troubleshooting: Kubernetes Center Shows No Clusters

## Why the Kubernetes Center is Empty

The **Kubernetes Center (preview)** in Azure Portal shows no clusters because:

1. **No AKS clusters have been deployed yet** - Clusters need to be created via Terraform first
2. **Preview feature limitation** - Kubernetes Center is a preview feature and may not show all clusters
3. **Wrong subscription/resource group** - Clusters might be in a different subscription

## Quick Check: Do Clusters Exist?

### Check via Azure CLI

```bash
# List all AKS clusters in current subscription
az aks list --output table

# List clusters in specific resource group
az aks list --resource-group mystira-dev-rg --output table

# Check if dev cluster exists
az aks show --name mystira-dev-aks --resource-group mystira-dev-rg
```

### Check via Azure Portal (Traditional View)

1. Go to **Azure Portal** → **Kubernetes services** (not "Kubernetes center")
2. Or search for "Kubernetes services" in the top search bar
3. You should see: `mystira-dev-aks`, `mystira-staging-aks`, `mystira-prod-aks` (if deployed)

## Deploying AKS Clusters

### Step 1: Deploy Dev Environment

The dev environment includes an AKS cluster definition:

```bash
cd infra/terraform/environments/dev

# Initialize Terraform (if not done)
terraform init

# Review what will be created
terraform plan

# Apply to create infrastructure (including AKS cluster)
terraform apply
```

**What gets created**:

- Resource Group: `mystira-dev-rg`
- Virtual Network and subnets
- Azure Container Registry: `mystiraacr`
- **Azure Kubernetes Service**: `mystira-dev-aks`
- Key Vaults
- Shared resources (PostgreSQL, Redis if configured)

### Step 2: Verify Cluster Deployment

After Terraform apply completes:

```bash
# Get AKS credentials
az aks get-credentials --resource-group mystira-dev-rg --name mystira-dev-aks

# Verify cluster is accessible
kubectl cluster-info
kubectl get nodes
```

### Step 3: Check in Azure Portal

1. Go to **Azure Portal** → **Kubernetes services**
2. You should see `mystira-dev-aks` listed
3. Click on it to see cluster details, node pools, etc.

## Why Kubernetes Center Might Not Show Clusters

### Kubernetes Center is a Preview Feature

The **Kubernetes Center (preview)** is a newer Azure Portal feature that may:

- Not show all existing clusters immediately
- Require clusters to be registered with it
- Only show clusters in certain subscriptions/regions

### Use Traditional Kubernetes Services View

Instead of Kubernetes Center, use:

- **Azure Portal** → Search "Kubernetes services"
- Or direct link: `https://portal.azure.com/#view/Microsoft_Azure_ContainerService/AksArmTypeBlade`

This traditional view shows all AKS clusters regardless of when they were created.

## Expected Cluster Names

Based on Terraform configuration:

| Environment | Cluster Name          | Resource Group       | Status               |
| ----------- | --------------------- | -------------------- | -------------------- |
| Dev         | `mystira-dev-aks`     | `mystira-dev-rg`     | Created by Terraform |
| Staging     | `mystira-staging-aks` | `mystira-staging-rg` | Created by Terraform |
| Prod        | `mystira-prod-aks`    | `mystira-prod-rg`    | Created by Terraform |

## Troubleshooting Steps

### 1. Verify Terraform State

```bash
cd infra/terraform/environments/dev
terraform show
```

Look for `azurerm_kubernetes_cluster.main` resource - if it exists, cluster should be deployed.

### 2. Check Resource Group

```bash
# List all resources in dev resource group
az resource list --resource-group mystira-dev-rg --output table
```

Look for `Microsoft.ContainerService/managedClusters` resources.

### 3. Check Subscription

Make sure you're looking in the correct Azure subscription:

```bash
# Show current subscription
az account show

# List all subscriptions
az account list --output table

# Switch subscription if needed
az account set --subscription "Your Subscription Name"
```

### 4. Verify Permissions

Ensure your Azure account has permissions to view Kubernetes resources:

```bash
# Check your role assignments
az role assignment list --assignee $(az account show --query user.name --output tsv) --all
```

## Next Steps

1. **If clusters don't exist**: Deploy via Terraform (see above)
2. **If clusters exist but not showing**: Use traditional "Kubernetes services" view instead of Kubernetes Center
3. **If you see errors**: Check Terraform apply output for deployment errors

## Related Documentation

- [Infrastructure Guide](./infrastructure.md)
- [Setup Guide](../SETUP.md#azure-infrastructure-setup)
- [ACR Strategy](./acr-strategy.md)
