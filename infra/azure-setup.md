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

#### Azure AD / Entra ID Permissions (Required for Identity Management)

If your infrastructure includes Azure AD app registrations, managed identities, or Azure AD B2C integration, the service principal needs additional permissions:

**Option 1: Microsoft Graph API Permissions (Recommended)**

```bash
# Get your service principal's client ID
SP_CLIENT_ID=$(az ad sp list --display-name "mystira-github-actions" --query "[0].appId" -o tsv)

# Add Application.ReadWrite.All permission (required for creating/managing app registrations)
az ad app permission add \
  --id "$SP_CLIENT_ID" \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions 1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9=Role

# Add DelegatedPermissionGrant.ReadWrite.All permission (required for OAuth2 permission grants)
az ad app permission add \
  --id "$SP_CLIENT_ID" \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions 8e8e4742-1d95-4f68-9d56-6ee75648c72a=Role

# Grant admin consent (requires Global Administrator or Privileged Role Administrator)
az ad app permission admin-consent --id "$SP_CLIENT_ID"

# Verify the permission was granted
az ad app permission list --id "$SP_CLIENT_ID" -o table
```

**Option 2: Azure AD Directory Role (Alternative)**

Instead of API permissions, you can assign the **Application Administrator** directory role:

```bash
# Get your service principal's object ID
SP_OBJECT_ID=$(az ad sp list --display-name "mystira-github-actions" --query "[0].id" -o tsv)

# Assign Application Administrator role
# Note: This requires Global Administrator privileges
az rest --method POST \
  --uri "https://graph.microsoft.com/v1.0/roleManagement/directory/roleAssignments" \
  --body "{
    \"@odata.type\": \"#microsoft.graph.unifiedRoleAssignment\",
    \"principalId\": \"$SP_OBJECT_ID\",
    \"roleDefinitionId\": \"9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3\",
    \"directoryScopeId\": \"/\"
  }"
```

**When are these permissions needed?**

- Creating Azure AD app registrations (Microsoft Entra External ID integration)
- Configuring managed identity AAD authentication for PostgreSQL
- Setting up workload identity federation for AKS

**Error if missing:**

```
Authorization_RequestDenied: Insufficient privileges to complete the operation
```

#### User Access Administrator Role (Required for RBAC)

If your infrastructure assigns RBAC roles to managed identities (e.g., AcrPull, Key Vault access, PostgreSQL access), the service principal needs the **User Access Administrator** role:

```bash
# Get your subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Get service principal object ID
SP_OBJECT_ID=$(az ad sp list --display-name "mystira-github-actions" --query "[0].id" -o tsv)

# Grant User Access Administrator role at subscription level
az role assignment create \
  --assignee "$SP_OBJECT_ID" \
  --role "User Access Administrator" \
  --scope "/subscriptions/$SUBSCRIPTION_ID"

# Verify the role assignment
az role assignment list --assignee "$SP_OBJECT_ID" --role "User Access Administrator" -o table
```

**When is this permission needed?**

- Assigning AcrPull role to AKS cluster identity
- Granting Key Vault Secrets User role to service identities
- Setting up Redis Cache Contributor role
- Any Terraform resources using `azurerm_role_assignment`

**Error if missing:**

```
AuthorizationFailed: The client does not have authorization to perform action
'Microsoft.Authorization/roleAssignments/write' over scope '/subscriptions/...'
```

#### Complete Permission Setup Script

Here's a complete script to set up all required permissions:

```bash
#!/bin/bash
# Complete permission setup for Mystira GitHub Actions service principal

# Configuration
SP_NAME="mystira-github-actions"

# Get IDs
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
SP_CLIENT_ID=$(az ad sp list --display-name "$SP_NAME" --query "[0].appId" -o tsv)
SP_OBJECT_ID=$(az ad sp list --display-name "$SP_NAME" --query "[0].id" -o tsv)

echo "Setting up permissions for service principal: $SP_NAME"
echo "  Client ID: $SP_CLIENT_ID"
echo "  Object ID: $SP_OBJECT_ID"
echo "  Subscription: $SUBSCRIPTION_ID"
echo ""

# 1. Azure AD - Application.ReadWrite.All permission
echo "1. Adding Azure AD Application.ReadWrite.All permission..."
az ad app permission add \
  --id "$SP_CLIENT_ID" \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions 1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9=Role 2>/dev/null || true

# 1b. Azure AD - DelegatedPermissionGrant.ReadWrite.All permission (for OAuth2 grants)
echo "1b. Adding Azure AD DelegatedPermissionGrant.ReadWrite.All permission..."
az ad app permission add \
  --id "$SP_CLIENT_ID" \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions 8e8e4742-1d95-4f68-9d56-6ee75648c72a=Role 2>/dev/null || true

echo "   Granting admin consent..."
az ad app permission admin-consent --id "$SP_CLIENT_ID" 2>/dev/null || echo "   (Requires Global Administrator - ask your admin to run this)"

# 2. User Access Administrator role
echo ""
echo "2. Assigning User Access Administrator role..."
az role assignment create \
  --assignee "$SP_OBJECT_ID" \
  --role "User Access Administrator" \
  --scope "/subscriptions/$SUBSCRIPTION_ID" 2>/dev/null || echo "   (Already assigned or insufficient privileges)"

# 3. Storage Blob Data Contributor (for Terraform state)
echo ""
echo "3. Assigning Storage Blob Data Contributor for Terraform state..."
az role assignment create \
  --assignee "$SP_OBJECT_ID" \
  --role "Storage Blob Data Contributor" \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/mys-shared-terraform-rg-san/providers/Microsoft.Storage/storageAccounts/myssharedtfstatesan" 2>/dev/null || echo "   (Already assigned or storage account doesn't exist yet)"

echo ""
echo "Permission setup complete!"
echo ""
echo "To verify all permissions:"
echo "  az ad app permission list --id $SP_CLIENT_ID -o table"
echo "  az role assignment list --assignee $SP_OBJECT_ID --all -o table"
```

#### Automated Permission Checks

The infrastructure deployment workflow (`infra-deploy.yml`) includes an automated permission check step that validates these permissions before running Terraform. If permissions are missing, the workflow will:

1. Detect the missing permissions
2. Provide clear error messages
3. Include fix commands in the workflow summary

Look for the "Check Service Principal Permissions" step in the workflow run for detailed diagnostics

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

#### Terraform Backend Storage Permissions (REQUIRED)

**CRITICAL**: The service principal needs **Storage Blob Data Contributor** role on the Terraform state storage account to access the backend using Azure AD authentication.

This is a **one-time manual setup** that must be completed before running deployments:

**IMPORTANT**: On Windows, use **PowerShell** (not Git Bash) to run these commands. Git Bash mangles the scope paths and causes "MissingSubscription" errors.

```bash
# Get your service principal object ID
SP_OBJECT_ID=$(az ad sp list --display-name "mystira-github-actions" --query "[0].id" -o tsv)

# Grant Storage Blob Data Contributor role on the Terraform state storage account
# This allows the service principal to read/write Terraform state blobs
# NOTE: Use --assignee (not --assignee-object-id) to avoid path mangling issues
az role assignment create \
  --assignee "$SP_OBJECT_ID" \
  --role "Storage Blob Data Contributor" \
  --scope "/subscriptions/22f9eb18-6553-4b7d-9451-47d0195085fe/resourceGroups/mys-shared-terraform-rg-san/providers/Microsoft.Storage/storageAccounts/myssharedtfstatesan"

# Verify the role assignment was created
az role assignment list \
  --assignee "$SP_OBJECT_ID" \
  --scope "/subscriptions/22f9eb18-6553-4b7d-9451-47d0195085fe/resourceGroups/mys-shared-terraform-rg-san/providers/Microsoft.Storage/storageAccounts/myssharedtfstatesan" \
  --query "[].{Role:roleDefinitionName}" -o table
```

**Why is this needed?**

The Terraform backend configuration uses `use_azuread_auth = true` for enhanced security instead of storage account keys. This requires the service principal to have explicit RBAC permissions on the storage account.

**When to run this:**

- **Once** during initial infrastructure setup
- After creating or regenerating the service principal
- If you see 403 authorization errors during `terraform init`

**Why can't the workflow grant this automatically?**

The service principal cannot grant role assignments to itself because:

- It only has **Contributor** role (creates/manages resources)
- Granting role assignments requires **Owner** or **User Access Administrator** role
- This is a security feature - only users with elevated permissions can grant access

**Error if missing:**

If you don't grant this permission, Terraform init will fail with:

```
Error: Failed to get existing workspaces: containers.Client#ListBlobs: Failure responding to request: StatusCode=403
Code="AuthorizationPermissionMismatch" Message="This request is not authorized to perform this operation using this permission."
```

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
