# Mystira Story-Generator Module - Output Definitions
# Module: infra/terraform/modules/story-generator
#
# This file contains all output value declarations for the story-generator module.
# See main.tf for resource definitions and variables.tf for input variables.

output "nsg_id" {
  description = "Network Security Group ID for story-generator service"
  value       = azurerm_network_security_group.story_generator.id
}

output "identity_id" {
  description = "Managed Identity ID for story-generator service"
  value       = azurerm_user_assigned_identity.story_generator.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.story_generator.principal_id
}

output "postgresql_server_id" {
  description = "PostgreSQL server ID (shared or dedicated)"
  value       = var.use_shared_postgresql ? var.shared_postgresql_server_id : azurerm_postgresql_flexible_server.story_generator[0].id
}

output "postgresql_database_name" {
  description = "PostgreSQL database name"
  value       = var.use_shared_postgresql ? "storygenerator" : azurerm_postgresql_flexible_server_database.story_generator[0].name
}

output "redis_cache_id" {
  description = "Redis cache ID (shared or dedicated)"
  value       = var.use_shared_redis ? var.shared_redis_cache_id : azurerm_redis_cache.story_generator[0].id
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID (shared or dedicated)"
  value       = var.use_shared_log_analytics ? var.shared_log_analytics_workspace_id : azurerm_log_analytics_workspace.story_generator[0].id
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.story_generator.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for story-generator secrets"
  value       = azurerm_key_vault.story_generator.id
}

output "key_vault_uri" {
  description = "Key Vault URI for story-generator secrets"
  value       = azurerm_key_vault.story_generator.vault_uri
}

output "identity_client_id" {
  description = "Managed Identity Client ID (for workload identity)"
  value       = azurerm_user_assigned_identity.story_generator.client_id
}

# -----------------------------------------------------------------------------
# Static Web App Outputs
# -----------------------------------------------------------------------------

output "static_web_app_id" {
  description = "Static Web App ID"
  value       = var.enable_static_web_app ? azurerm_static_web_app.story_generator[0].id : null
}

output "static_web_app_name" {
  description = "Static Web App name"
  value       = var.enable_static_web_app ? azurerm_static_web_app.story_generator[0].name : null
}

output "static_web_app_default_hostname" {
  description = "Static Web App default hostname"
  value       = var.enable_static_web_app ? azurerm_static_web_app.story_generator[0].default_host_name : null
}

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = var.enable_static_web_app ? "https://${azurerm_static_web_app.story_generator[0].default_host_name}" : null
}

output "static_web_app_api_key" {
  description = "Static Web App API key for deployments"
  value       = var.enable_static_web_app ? azurerm_static_web_app.story_generator[0].api_key : null
  sensitive   = true
}

output "swa_custom_domain" {
  description = "Static Web App custom domain (if enabled)"
  value       = var.enable_swa_custom_domain && var.swa_custom_domain != "" ? var.swa_custom_domain : null
}
