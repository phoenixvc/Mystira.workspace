#!/bin/bash
# Bootstrap script to create Azure Storage Account for Terraform state
# This script is idempotent - it can be run multiple times safely

set -e

# Configuration
RESOURCE_GROUP_NAME="mystira-terraform-state"
STORAGE_ACCOUNT_NAME="mystiraterraformstate"
CONTAINER_NAME="tfstate"
LOCATION="${AZURE_LOCATION:-eastus}"
STORAGE_SKU="${AZURE_STORAGE_SKU:-Standard_LRS}"

# Error handler for authorization failures
error_handler() {
  local exit_code=$?
  if [ $exit_code -ne 0 ]; then
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "❌ BOOTSTRAP FAILED"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "The bootstrap script failed. This is commonly caused by insufficient"
    echo "Azure permissions."
    echo ""
    echo "Required Permissions:"
    echo "  The Azure service principal needs the 'Contributor' role at the"
    echo "  subscription level to create and manage infrastructure resources."
    echo ""
    echo "To fix authorization errors:"
    echo ""
    echo "  1. Grant Contributor role to the service principal:"
    echo "     az role assignment create \\"
    echo "       --assignee <service-principal-id> \\"
    echo "       --role Contributor \\"
    echo "       --scope /subscriptions/<subscription-id>"
    echo ""
    echo "  2. Wait 5-10 minutes for permissions to propagate"
    echo ""
    echo "  3. Re-run this workflow"
    echo ""
    echo "For detailed setup instructions, see:"
    echo "  infra/azure-setup.md in the repository"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  fi
}

# Set trap to catch errors
trap error_handler EXIT

echo "Bootstrapping Terraform backend..."
echo "Resource Group: $RESOURCE_GROUP_NAME"
echo "Storage Account: $STORAGE_ACCOUNT_NAME"
echo "Container: $CONTAINER_NAME"
echo "Location: $LOCATION"
echo ""

# Check if resource group exists
if az group show --name "$RESOURCE_GROUP_NAME" &>/dev/null; then
  echo "✓ Resource group '$RESOURCE_GROUP_NAME' already exists"
else
  echo "Creating resource group '$RESOURCE_GROUP_NAME'..."
  az group create \
    --name "$RESOURCE_GROUP_NAME" \
    --location "$LOCATION" \
    --tags "Purpose=TerraformState" "ManagedBy=Bootstrap"
  echo "✓ Resource group created"
fi

# Check if storage account exists
if az storage account show --name "$STORAGE_ACCOUNT_NAME" --resource-group "$RESOURCE_GROUP_NAME" &>/dev/null; then
  echo "✓ Storage account '$STORAGE_ACCOUNT_NAME' already exists"
else
  echo "Creating storage account '$STORAGE_ACCOUNT_NAME'..."
  az storage account create \
    --name "$STORAGE_ACCOUNT_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --location "$LOCATION" \
    --sku "$STORAGE_SKU" \
    --encryption-services blob \
    --allow-blob-public-access false \
    --min-tls-version TLS1_2 \
    --tags "Purpose=TerraformState" "ManagedBy=Bootstrap"
  echo "✓ Storage account created"
fi

# Check if container exists
if az storage container show \
  --name "$CONTAINER_NAME" \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --auth-mode login &>/dev/null; then
  echo "✓ Container '$CONTAINER_NAME' already exists"
else
  echo "Creating container '$CONTAINER_NAME'..."
  az storage container create \
    --name "$CONTAINER_NAME" \
    --account-name "$STORAGE_ACCOUNT_NAME" \
    --auth-mode login
  echo "✓ Container created"
fi

echo ""
echo "✓ Terraform backend bootstrap completed successfully!"
echo ""
echo "Backend configuration:"
echo "  resource_group_name  = \"$RESOURCE_GROUP_NAME\""
echo "  storage_account_name = \"$STORAGE_ACCOUNT_NAME\""
echo "  container_name       = \"$CONTAINER_NAME\""

# Clear trap on success
trap - EXIT
