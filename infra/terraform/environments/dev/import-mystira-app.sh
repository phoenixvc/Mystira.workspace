#!/bin/bash
# =============================================================================
# Import Existing Mystira.App Resources into Terraform State
# =============================================================================
#
# Run this script AFTER:
# 1. terraform init
# 2. Creating the resource group manually or via first apply
#
# This imports resources that were previously deployed via Bicep
#
# NOTE: Mystira.App now uses the shared core resource group (mys-dev-core-rg-san)
# and shared monitoring. If you have existing resources in a separate RG,
# you may need to migrate them first.
# =============================================================================

set -e

# Configuration
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
RG_NAME="mys-dev-core-rg-san"  # Now using shared core resource group
OLD_RG_NAME="mys-dev-mystira-rg-san"  # Previous separate resource group (if exists)
LOCATION="southafricanorth"
FALLBACK_LOCATION="eastus2"

echo "=== Mystira.App Terraform Import Script ==="
echo "Subscription: $SUBSCRIPTION_ID"
echo "Resource Group: $RG_NAME (shared core)"
echo "Note: Using shared monitoring from shared_monitoring module"
echo ""

# Check if logged in
if [ -z "$SUBSCRIPTION_ID" ]; then
    echo "ERROR: Not logged in to Azure. Run 'az login' first."
    exit 1
fi

# Function to import a resource
import_resource() {
    local tf_address=$1
    local azure_id=$2
    local resource_name=$3

    echo "Importing $resource_name..."
    if terraform import "$tf_address" "$azure_id" 2>/dev/null; then
        echo "  ✓ $resource_name imported successfully"
    else
        echo "  ⚠ $resource_name not found or already imported"
    fi
}

# =============================================================================
# NOTE: Resource Group and Monitoring
# =============================================================================
echo ""
echo "--- Resource Group & Monitoring ---"
echo "Mystira.App now uses:"
echo "  - Shared resource group: mys-dev-core-rg-san (already exists)"
echo "  - Shared monitoring from shared_monitoring module (already exists)"
echo "  - No separate imports needed for these resources"
echo ""
echo "If you have resources in the old separate resource group ($OLD_RG_NAME),"
echo "you may need to migrate them to $RG_NAME first."

# =============================================================================
# Import Key Vault
# =============================================================================
echo ""
echo "--- Importing Key Vault ---"
import_resource \
    'module.mystira_app.azurerm_key_vault.main' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.KeyVault/vaults/mys-dev-app-kv-san" \
    "Key Vault"

# =============================================================================
# Import Storage Account
# =============================================================================
echo ""
echo "--- Importing Storage ---"
import_resource \
    'module.mystira_app.azurerm_storage_account.main[0]' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Storage/storageAccounts/mysdevmystirastsan" \
    "Storage Account"

import_resource \
    'module.mystira_app.azurerm_storage_container.media[0]' \
    "https://mysdevmystirastsan.blob.core.windows.net/mystira-app-media" \
    "Media Container"

# =============================================================================
# Import Cosmos DB
# =============================================================================
echo ""
echo "--- Importing Cosmos DB ---"
import_resource \
    'module.mystira_app.azurerm_cosmosdb_account.main[0]' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.DocumentDB/databaseAccounts/mys-dev-mystira-cosmos-san" \
    "Cosmos DB Account"

import_resource \
    'module.mystira_app.azurerm_cosmosdb_sql_database.main[0]' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.DocumentDB/databaseAccounts/mys-dev-mystira-cosmos-san/sqlDatabases/MystiraAppDb" \
    "Cosmos DB Database"

# Import Cosmos DB containers
CONTAINERS=("UserProfiles" "Accounts" "Scenarios" "GameSessions" "ContentBundles" "PendingSignups" "CompassTrackings")
for container in "${CONTAINERS[@]}"; do
    import_resource \
        "module.mystira_app.azurerm_cosmosdb_sql_container.containers[\"$container\"]" \
        "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.DocumentDB/databaseAccounts/mys-dev-mystira-cosmos-san/sqlDatabases/MystiraAppDb/containers/$container" \
        "Cosmos DB Container: $container"
done

# =============================================================================
# Import App Service
# =============================================================================
echo ""
echo "--- Importing App Service ---"
import_resource \
    'module.mystira_app.azurerm_service_plan.main' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Web/serverFarms/mys-dev-mystira-asp-san" \
    "App Service Plan"

import_resource \
    'module.mystira_app.azurerm_linux_web_app.api' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Web/sites/mys-dev-mystira-api-san" \
    "App Service (API)"

# =============================================================================
# Import Static Web App (in fallback region)
# =============================================================================
echo ""
echo "--- Importing Static Web App ---"
import_resource \
    'module.mystira_app.azurerm_static_web_app.main[0]' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Web/staticSites/mys-dev-mystira-swa-eus2" \
    "Static Web App"

# =============================================================================
# Import Communication Services
# =============================================================================
echo ""
echo "--- Importing Communication Services ---"
import_resource \
    'module.mystira_app.azurerm_communication_service.main[0]' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Communication/communicationServices/mys-dev-mystira-acs-san" \
    "Communication Service"

import_resource \
    'module.mystira_app.azurerm_email_communication_service.main[0]' \
    "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Communication/emailServices/mys-dev-mystira-ecs-san" \
    "Email Communication Service"

# =============================================================================
# Summary
# =============================================================================
echo ""
echo "=== Import Complete ==="
echo ""
echo "Next steps:"
echo "1. Run 'terraform plan' to see any drift between state and actual resources"
echo "2. Adjust variables in mystira-app.tf if needed to match existing configuration"
echo "3. Run 'terraform apply' to reconcile any differences"
echo ""
echo "Note: Some resources may need manual configuration adjustment."
echo "Check terraform plan output carefully before applying."
