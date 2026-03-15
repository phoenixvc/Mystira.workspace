# =============================================================================
# Mystira.App Infrastructure - Staging
# 
# OPTIMIZATION: Uses Dev's shared resources (Dev + Staging share infrastructure)
# This reduces costs by not duplicating: Cosmos DB, Redis, PostgreSQL, Storage
# =============================================================================

# =============================================================================
# Data Sources: Reference Dev's Shared Resources
# =============================================================================

# Dev's Cosmos DB
data "azurerm_cosmosdb_account" "dev_cosmos" {
  name                = "mys-dev-core-cosmos-san"
  resource_group_name = "mys-dev-core-rg-san"
}

# Dev's Storage Account
data "azurerm_storage_account" "dev_storage" {
  name                = "mysdevcorestsan"
  resource_group_name = "mys-dev-core-rg-san"
}

# Dev's Redis
data "azurerm_redis_cache" "dev_redis" {
  name                = "mys-dev-core-cache"
  resource_group_name = "mys-dev-core-rg-san"
}

# Dev's PostgreSQL
data "azurerm_postgresql_flexible_server" "dev_postgres" {
  name                = "mys-dev-core-db"
  resource_group_name = "mys-dev-core-rg-san"
}

# Dev's Key Vault (for app secrets)
data "azurerm_key_vault" "dev_app_kv" {
  name                = "mys-dev-core-kv-san"
  resource_group_name = "mys-dev-core-rg-san"
}

# Dev's Log Analytics
data "azurerm_log_analytics_workspace" "dev_log_analytics" {
  name                = "mys-dev-core-log"
  resource_group_name = "mys-dev-core-rg-san"
}

# Shared Communication Services (cross-environment)
data "azurerm_communication_service" "shared" {
  name                = "mys-shared-acs"
  resource_group_name = "mys-shared-comms-rg-glob"
}

# =============================================================================
# Local Values - Use Dev's for shared, Staging for app-specific
# =============================================================================

locals {
  # Use dev resources for shared, staging-specific for app
  cosmos_account_id     = data.azurerm_cosmosdb_account.dev_cosmos.id
  cosmos_endpoint       = data.azurerm_cosmosdb_account.dev_cosmos.endpoint
  storage_account_id    = data.azurerm_storage_account.dev_storage.id
  storage_blob_endpoint = "https://${data.azurerm_storage_account.dev_storage.name}.blob.core.windows.net"
  redis_hostname        = data.azurerm_redis_cache.dev_redis.hostname
  redis_ssl_port        = data.azurerm_redis_cache.dev_redis.ssl_port
  postgres_fqdn         = data.azurerm_postgresql_flexible_server.dev_postgres.fqdn
  key_vault_uri         = data.azurerm_key_vault.dev_app_kv.vault_uri
  log_analytics_id      = data.azurerm_log_analytics_workspace.dev_log_analytics.id
}

# =============================================================================
# Mystira App Module - Uses Shared Dev Resources
# =============================================================================

module "mystira_app" {
  source = "../../modules/mystira-app"

  environment         = "staging"
  location            = var.location
  fallback_location   = "eastus2"
  resource_group_name = azurerm_resource_group.app.name
  project_name        = "mystira"
  org                 = "mys"

  # -----------------------------------------------------------------------------
  # Cosmos DB Configuration - USE DEV'S COSMOS (different database)
  # -----------------------------------------------------------------------------
  skip_cosmos_creation              = true
  existing_cosmos_connection_string = data.azurerm_cosmosdb_account.dev_cosmos.primary_sql_connection_string
  shared_cosmos_endpoint            = data.azurerm_cosmosdb_account.dev_cosmos.endpoint
  shared_cosmos_database_name       = "MystiraAppDbStaging"

  # -----------------------------------------------------------------------------
  # App Service Configuration (API Backend)
  # -----------------------------------------------------------------------------
  app_service_sku = "B1"
  dotnet_version  = "10.0"

  # Custom domains
  enable_api_custom_domain = false
  api_custom_domain        = "staging.api.mystira.app"

  # -----------------------------------------------------------------------------
  # Static Web App Configuration (Blazor WASM PWA)
  # -----------------------------------------------------------------------------
  enable_static_web_app    = true
  static_web_app_sku       = "Free"
  enable_app_custom_domain = false
  app_custom_domain        = "staging.mystira.app"

  # -----------------------------------------------------------------------------
  # Storage Configuration - USE DEV'S STORAGE (different container)
  # -----------------------------------------------------------------------------
  skip_storage_creation            = true
  shared_storage_connection_string = data.azurerm_storage_account.dev_storage.primary_connection_string
  shared_storage_blob_endpoint     = local.storage_blob_endpoint

  cors_allowed_origins = [
    "https://staging.mystira.app",
    "https://staging.app.mystira.app",
  ]

  # -----------------------------------------------------------------------------
  # Communication Services - USE SHARED (mys-shared-comms-rg-glob)
  # -----------------------------------------------------------------------------
  enable_communication_services = false
  use_shared_acs                = true
  shared_acs_connection_string  = data.azurerm_communication_service.shared.primary_connection_string
  sender_email                  = "DoNotReply@mystira.app"

  # -----------------------------------------------------------------------------
  # Redis - USE DEV'S REDIS (different DB number)
  # -----------------------------------------------------------------------------
  enable_redis          = true
  use_shared_redis      = true
  shared_redis_hostname = data.azurerm_redis_cache.dev_redis.hostname

  # -----------------------------------------------------------------------------
  # PostgreSQL - USE DEV'S POSTGRES (different database)
  # -----------------------------------------------------------------------------
  enable_postgresql             = true
  use_shared_postgresql         = true
  shared_postgresql_server_id   = data.azurerm_postgresql_flexible_server.dev_postgres.id
  shared_postgresql_server_fqdn = data.azurerm_postgresql_flexible_server.dev_postgres.fqdn

  # -----------------------------------------------------------------------------
  # Key Vault - USE DEV'S KEY VAULT
  # -----------------------------------------------------------------------------
  use_shared_keyvault = true
  shared_keyvault_id  = data.azurerm_key_vault.dev_app_kv.id
  shared_keyvault_uri = data.azurerm_key_vault.dev_app_kv.vault_uri

  # -----------------------------------------------------------------------------
  # Monitoring - USE DEV'S LOG ANALYTICS (let module create App Insights)
  # -----------------------------------------------------------------------------
  use_shared_monitoring             = true
  shared_log_analytics_workspace_id = local.log_analytics_id

  enable_alerts = true

  # -----------------------------------------------------------------------------
  # Tags
  # -----------------------------------------------------------------------------
  tags = local.common_tags

  depends_on = [
    data.azurerm_cosmosdb_account.dev_cosmos,
    data.azurerm_storage_account.dev_storage,
    data.azurerm_redis_cache.dev_redis,
    data.azurerm_postgresql_flexible_server.dev_postgres,
    data.azurerm_key_vault.dev_app_kv,
    data.azurerm_log_analytics_workspace.dev_log_analytics,
  ]
}

# =============================================================================
# Outputs
# =============================================================================

output "mystira_app_api_url" {
  description = "Mystira.App API URL"
  value       = module.mystira_app.app_service_url
}

output "mystira_app_web_url" {
  description = "Mystira.App Web URL"
  value       = module.mystira_app.static_web_app_url
}

output "shared_with_dev" {
  description = "Resources shared with dev environment"
  value       = true
}
