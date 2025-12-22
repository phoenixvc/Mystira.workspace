# =============================================================================
# Mystira.App Infrastructure Module - Outputs
# =============================================================================

# -----------------------------------------------------------------------------
# Cosmos DB Outputs
# -----------------------------------------------------------------------------

output "cosmos_db_account_name" {
  description = "Cosmos DB account name"
  value       = var.skip_cosmos_creation ? null : azurerm_cosmosdb_account.main[0].name
}

output "cosmos_db_account_id" {
  description = "Cosmos DB account ID"
  value       = var.skip_cosmos_creation ? null : azurerm_cosmosdb_account.main[0].id
}

output "cosmos_db_endpoint" {
  description = "Cosmos DB endpoint URL"
  value       = var.skip_cosmos_creation ? null : azurerm_cosmosdb_account.main[0].endpoint
}

output "cosmos_db_connection_string" {
  description = "Cosmos DB connection string"
  value       = var.skip_cosmos_creation ? null : azurerm_cosmosdb_account.main[0].primary_sql_connection_string
  sensitive   = true
}

output "cosmos_db_database_name" {
  description = "Cosmos DB database name"
  value       = var.skip_cosmos_creation ? null : azurerm_cosmosdb_sql_database.main[0].name
}

# -----------------------------------------------------------------------------
# App Service Outputs
# -----------------------------------------------------------------------------

output "app_service_id" {
  description = "App Service ID"
  value       = azurerm_linux_web_app.api.id
}

output "app_service_name" {
  description = "App Service name"
  value       = azurerm_linux_web_app.api.name
}

output "app_service_default_hostname" {
  description = "App Service default hostname"
  value       = azurerm_linux_web_app.api.default_hostname
}

output "app_service_url" {
  description = "App Service URL"
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "app_service_principal_id" {
  description = "App Service managed identity principal ID"
  value       = azurerm_linux_web_app.api.identity[0].principal_id
}

output "app_service_plan_id" {
  description = "App Service Plan ID"
  value       = azurerm_service_plan.main.id
}

# -----------------------------------------------------------------------------
# Static Web App Outputs
# -----------------------------------------------------------------------------

output "static_web_app_id" {
  description = "Static Web App ID"
  value       = var.enable_static_web_app ? azurerm_static_web_app.main[0].id : null
}

output "static_web_app_name" {
  description = "Static Web App name"
  value       = var.enable_static_web_app ? azurerm_static_web_app.main[0].name : null
}

output "static_web_app_default_hostname" {
  description = "Static Web App default hostname"
  value       = var.enable_static_web_app ? azurerm_static_web_app.main[0].default_host_name : null
}

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = var.enable_static_web_app ? "https://${azurerm_static_web_app.main[0].default_host_name}" : null
}

output "static_web_app_api_key" {
  description = "Static Web App API key for deployments"
  value       = var.enable_static_web_app ? azurerm_static_web_app.main[0].api_key : null
  sensitive   = true
}

output "api_custom_domain" {
  description = "API custom domain (if enabled)"
  value       = var.enable_api_custom_domain && var.api_custom_domain != "" ? var.api_custom_domain : null
}

output "app_custom_domain" {
  description = "Static Web App custom domain (if enabled)"
  value       = var.enable_app_custom_domain && var.app_custom_domain != "" ? var.app_custom_domain : null
}

# -----------------------------------------------------------------------------
# Storage Outputs
# -----------------------------------------------------------------------------

output "storage_account_id" {
  description = "Storage account ID"
  value       = var.skip_storage_creation ? null : azurerm_storage_account.main[0].id
}

output "storage_account_name" {
  description = "Storage account name"
  value       = var.skip_storage_creation ? null : azurerm_storage_account.main[0].name
}

output "storage_primary_blob_endpoint" {
  description = "Storage primary blob endpoint"
  value       = var.skip_storage_creation ? null : azurerm_storage_account.main[0].primary_blob_endpoint
}

output "storage_connection_string" {
  description = "Storage account connection string"
  value       = var.skip_storage_creation ? null : azurerm_storage_account.main[0].primary_connection_string
  sensitive   = true
}

output "media_container_url" {
  description = "Media container URL"
  value       = var.skip_storage_creation ? null : "${azurerm_storage_account.main[0].primary_blob_endpoint}mystira-app-media"
}

output "avatars_container_url" {
  description = "Avatars container URL"
  value       = var.skip_storage_creation ? null : "${azurerm_storage_account.main[0].primary_blob_endpoint}avatars"
}

output "audio_container_url" {
  description = "Audio container URL"
  value       = var.skip_storage_creation ? null : "${azurerm_storage_account.main[0].primary_blob_endpoint}audio"
}

output "content_bundles_container_url" {
  description = "Content bundles container URL"
  value       = var.skip_storage_creation ? null : "${azurerm_storage_account.main[0].primary_blob_endpoint}content-bundles"
}

output "archives_container_name" {
  description = "Archives container name (private, use SAS tokens)"
  value       = var.skip_storage_creation ? null : "archives"
}

output "uploads_container_name" {
  description = "Uploads container name (private, use SAS tokens)"
  value       = var.skip_storage_creation ? null : "uploads"
}

output "storage_tiering_enabled" {
  description = "Whether storage lifecycle tiering is enabled"
  value       = var.skip_storage_creation ? false : var.enable_storage_tiering
}

# -----------------------------------------------------------------------------
# Key Vault Outputs
# -----------------------------------------------------------------------------

output "key_vault_id" {
  description = "Key Vault ID"
  value       = azurerm_key_vault.main.id
}

output "key_vault_name" {
  description = "Key Vault name"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.main.vault_uri
}

# -----------------------------------------------------------------------------
# Monitoring Outputs
# -----------------------------------------------------------------------------

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID (shared or created)"
  value       = local.log_analytics_workspace_id
}

output "log_analytics_workspace_name" {
  description = "Log Analytics Workspace name (null if using shared)"
  value       = var.use_shared_monitoring ? null : azurerm_log_analytics_workspace.main[0].name
}

output "application_insights_id" {
  description = "Application Insights ID (null if using shared)"
  value       = var.use_shared_monitoring ? var.shared_application_insights_id : azurerm_application_insights.main[0].id
}

output "application_insights_name" {
  description = "Application Insights name (null if using shared)"
  value       = var.use_shared_monitoring ? null : azurerm_application_insights.main[0].name
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = local.application_insights_connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key (null if using shared)"
  value       = var.use_shared_monitoring ? null : azurerm_application_insights.main[0].instrumentation_key
  sensitive   = true
}

output "using_shared_monitoring" {
  description = "Whether shared monitoring resources are being used"
  value       = var.use_shared_monitoring
}

# -----------------------------------------------------------------------------
# Communication Services Outputs
# -----------------------------------------------------------------------------

output "communication_service_id" {
  description = "Communication Service ID"
  value       = var.enable_communication_services ? azurerm_communication_service.main[0].id : null
}

output "communication_service_name" {
  description = "Communication Service name"
  value       = var.enable_communication_services ? azurerm_communication_service.main[0].name : null
}

output "email_communication_service_id" {
  description = "Email Communication Service ID"
  value       = var.enable_communication_services ? azurerm_email_communication_service.main[0].id : null
}

# -----------------------------------------------------------------------------
# Azure Bot Outputs
# -----------------------------------------------------------------------------

output "bot_id" {
  description = "Azure Bot ID"
  value       = var.enable_azure_bot && var.bot_microsoft_app_id != "" ? azurerm_bot_service_azure_bot.main[0].id : null
}

output "bot_name" {
  description = "Azure Bot name"
  value       = var.enable_azure_bot && var.bot_microsoft_app_id != "" ? azurerm_bot_service_azure_bot.main[0].name : null
}

# -----------------------------------------------------------------------------
# PostgreSQL Outputs (Hybrid Data Strategy)
# -----------------------------------------------------------------------------

output "postgresql_database_name" {
  description = "PostgreSQL database name"
  value       = var.enable_postgresql && var.use_shared_postgresql ? azurerm_postgresql_flexible_server_database.mystira_app[0].name : null
}

output "postgresql_database_id" {
  description = "PostgreSQL database ID"
  value       = var.enable_postgresql && var.use_shared_postgresql ? azurerm_postgresql_flexible_server_database.mystira_app[0].id : null
}

output "postgresql_enabled" {
  description = "Whether PostgreSQL is enabled for this deployment"
  value       = var.enable_postgresql
}

output "data_migration_phase" {
  description = "Current data migration phase (0=CosmosOnly, 1=DualWriteCosmosRead, 2=DualWritePostgresRead, 3=PostgresOnly)"
  value       = var.data_migration_phase
}

# -----------------------------------------------------------------------------
# Redis Cache Outputs
# -----------------------------------------------------------------------------

output "redis_cache_id" {
  description = "Redis Cache ID"
  value       = var.enable_redis ? azurerm_redis_cache.main[0].id : null
}

output "redis_cache_name" {
  description = "Redis Cache name"
  value       = var.enable_redis ? azurerm_redis_cache.main[0].name : null
}

output "redis_cache_hostname" {
  description = "Redis Cache hostname"
  value       = var.enable_redis ? azurerm_redis_cache.main[0].hostname : null
}

output "redis_cache_port" {
  description = "Redis Cache SSL port"
  value       = var.enable_redis ? azurerm_redis_cache.main[0].ssl_port : null
}

output "redis_cache_connection_string" {
  description = "Redis Cache connection string"
  value       = var.enable_redis ? azurerm_redis_cache.main[0].primary_connection_string : null
  sensitive   = true
}

output "redis_enabled" {
  description = "Whether Redis Cache is enabled for this deployment"
  value       = var.enable_redis
}

# -----------------------------------------------------------------------------
# Resource Import Commands
# -----------------------------------------------------------------------------

output "import_commands" {
  description = "Terraform import commands for existing resources"
  value = var.skip_cosmos_creation || var.skip_storage_creation ? <<-EOT
    # Import commands for existing resources:
    ${var.skip_cosmos_creation ? "" : "# terraform import 'module.mystira_app.azurerm_cosmosdb_account.main[0]' /subscriptions/<sub>/resourceGroups/${var.resource_group_name}/providers/Microsoft.DocumentDB/databaseAccounts/${local.name_prefix}-cosmos-${local.region_code}"}
    ${var.skip_storage_creation ? "" : "# terraform import 'module.mystira_app.azurerm_storage_account.main[0]' /subscriptions/<sub>/resourceGroups/${var.resource_group_name}/providers/Microsoft.Storage/storageAccounts/${local.storage_account_name}"}
  EOT : null
}
