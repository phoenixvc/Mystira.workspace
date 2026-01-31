# =============================================================================
# Mystira App - Dev Environment
# =============================================================================
# Consumer-facing Blazor WASM PWA with .NET API backend
# =============================================================================

variable "environment" {
  type = string
}

variable "location" {
  type = string
}

variable "fallback_location" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# Shared infrastructure inputs
variable "shared_postgresql_server_id" {
  type = string
}

variable "shared_postgresql_server_fqdn" {
  type = string
}

variable "shared_redis_cache_id" {
  type = string
}

variable "shared_redis_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_cosmos_db_account_id" {
  type = string
}

variable "shared_cosmos_db_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_storage_account_id" {
  type = string
}

variable "shared_storage_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_log_analytics_workspace_id" {
  type = string
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
}

# =============================================================================
# Mystira App Module
# =============================================================================

module "mystira_app" {
  source = "../../../../modules/mystira-app"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  # Pass shared infrastructure references
  postgresql_server_fqdn            = var.shared_postgresql_server_fqdn
  redis_connection_string           = var.shared_redis_connection_string
  cosmos_db_connection_string       = var.shared_cosmos_db_connection_string
  storage_connection_string         = var.shared_storage_connection_string
  log_analytics_workspace_id        = var.shared_log_analytics_workspace_id
  application_insights_connection_string = var.shared_application_insights_connection_string
}

# =============================================================================
# Outputs
# =============================================================================

output "static_web_app_url" {
  value = module.mystira_app.static_web_app_default_hostname
}

output "api_url" {
  value = module.mystira_app.api_url
}
