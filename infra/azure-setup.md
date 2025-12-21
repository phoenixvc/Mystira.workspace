# Azure Setup Guide

This guide explains how to set up Azure credentials and permissions for the Mystira infrastructure deployment.

## Prerequisites

- Azure CLI installed (`az --version` to verify)
- An Azure subscription
- Owner or User Access Administrator role on the Azure subscription (required to grant permissions)

## Service Principal Setup

The GitHub Actions workflows use a service principal to authenticate with Azure. This service principal needs specific permissions to manage infrastructure resources.

### Step 1: Create a Service Principal

```bash
# Login to Azure
az login

# Set your subscription (replace with your subscription ID)
# To find your subscription ID: az account show --query id -o tsv
SUBSCRIPTION_ID="your-subscription-id"
az account set --subscription $SUBSCRIPTION_ID

# Create a service principal with Contributor role at subscription level
# Note: --sdk-auth is deprecated, so we'll format the output manually
SP_OUTPUT=$(az ad sp create-for-rbac \
  --name "mystira-github-actions" \
  --role "Contributor" \
  --scopes "/subscriptions/$SUBSCRIPTION_ID")

# Display the output
echo $SP_OUTPUT

# The output will look like:
# {
#   "appId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
#   "displayName": "mystira-github-actions",
#   "password": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
#   "tenant": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
# }
```

**Important**: Save this output securely. You'll need to format it for the GitHub secret (see Step 3).

### Step 2: Required Permissions

The service principal needs the following permissions to successfully run the infrastructure deployment:

#### Minimum Required Roles

At the **subscription level**, the service principal needs:

- **Contributor** role - Required for:
  - Creating and managing resource groups
  - Creating and managing storage accounts
  - Creating and managing AKS clusters
  - Creating and managing all infrastructure resources

Alternatively, you can use more granular roles:

- **Resource Group Contributor** - For managing resource groups
- **Storage Account Contributor** - For managing storage accounts
- **Azure Kubernetes Service Contributor** - For managing AKS clusters

#### Verify Permissions

To verify the service principal has the correct permissions:

```bash
# Get the service principal object ID
SP_OBJECT_ID=$(az ad sp list --display-name "mystira-github-actions" --query "[0].id" -o tsv)

# Check role assignments
az role assignment list --assignee $SP_OBJECT_ID --all -o table
```

You should see at least one role assignment with:
- Role: `Contributor`
- Scope: `/subscriptions/{subscription-id}`

### Step 3: Configure GitHub Secret

1. Go to your GitHub repository settings
2. Navigate to **Secrets and variables** → **Actions**
3. Create a new repository secret named `AZURE_CREDENTIALS`
4. Format and paste the JSON credentials from Step 1

The JSON format required for GitHub Actions (map the values from Step 1 output):

```json
{
  "clientId": "<appId-from-step1>",
  "clientSecret": "<password-from-step1>",
  "subscriptionId": "<your-subscription-id>",
  "tenantId": "<tenant-from-step1>"
}
```

**Mapping from `az ad sp create-for-rbac` output:**
- `appId` → `clientId`
- `password` → `clientSecret`
- `tenant` → `tenantId`
- Add your `subscriptionId` manually

## Troubleshooting

### Authorization Failed Error

If you see an error like:

```
ERROR: (AuthorizationFailed) The client '***' with object id 'xxx' does not have 
authorization to perform action 'Microsoft.Resources/subscriptions/resourcegroups/write'
```

This means:

1. **The service principal doesn't have sufficient permissions**
   - Solution: Grant the `Contributor` role at the subscription level (see Step 1)
   
2. **Permissions were recently granted but not yet propagated**
   - Solution: Wait 5-10 minutes for Azure to propagate the permissions
   - Then re-run the workflow

3. **The service principal credentials have expired**
   - Solution: Regenerate the service principal credentials and update the GitHub secret

### Verifying Service Principal Access

To test if your service principal can create resource groups:

```bash
# Login as the service principal
# Use the values from Step 1: appId as -u, password as -p, tenant as --tenant
az login --service-principal \
  -u <appId-from-step1> \
  -p <password-from-step1> \
  --tenant <tenant-from-step1>

# Try to create a test resource group
az group create --name test-permissions-rg --location eastus

# Clean up
az group delete --name test-permissions-rg --yes --no-wait
```

## Security Best Practices

1. **Principle of Least Privilege**: Only grant the minimum permissions required
2. **Credential Rotation**: Rotate service principal credentials regularly (every 90 days recommended)
3. **Audit Access**: Regularly review service principal permissions and usage
4. **Use Managed Identities**: Where possible, use managed identities instead of service principals (for Azure-hosted runners)

## Resources Managed by the Service Principal

The service principal will create and manage the following Azure resources:

### Bootstrap Resources (Terraform State)
- Resource Group: `mystira-terraform-state`
- Storage Account: `mystiraterraformstate`
- Storage Container: `tfstate`

### Application Resources (per environment)
- Resource Group: `mystira-{env}-rg` (dev/staging/prod)
- AKS Cluster: `mystira-{env}-aks`
- Azure DNS Zone: `mystira.app` (production only)
- Additional resources as defined in Terraform configurations

## Additional Configuration

### Custom Azure Location

By default, resources are created in `eastus`. To use a different location:

1. Set the `AZURE_LOCATION` environment variable in the GitHub workflow
2. Or modify the default in `infra/scripts/bootstrap-terraform-backend.sh`

### Storage Account SKU

By default, the Terraform state storage account uses `Standard_LRS`. To use a different SKU:

1. Set the `AZURE_STORAGE_SKU` environment variable in the GitHub workflow
2. Or modify the default in `infra/scripts/bootstrap-terraform-backend.sh`

Options: `Standard_LRS`, `Standard_GRS`, `Standard_RAGRS`, `Standard_ZRS`

## Next Steps

After completing this setup:

1. The GitHub Actions workflow should successfully authenticate with Azure
2. The bootstrap script will create the Terraform state storage
3. Terraform will use the remote state for infrastructure management

If you encounter any issues, refer to the Troubleshooting section above or check the Azure Activity Log for detailed error messages.
