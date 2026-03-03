# =============================================================================
# Mystira App - Production Environment
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
  type    = string
  default = "eastus2"
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# Shared infrastructure inputs (optional - use defaults when not provided)
variable "shared_postgresql_server_id" {
  type    = string
  default = null
}

variable "shared_postgresql_server_fqdn" {
  type    = string
  default = null
}

variable "shared_cosmos_db_connection_string" {
  type      = string
  sensitive = true
  default   = null
}

variable "shared_storage_connection_string" {
  type      = string
  sensitive = true
  default   = null
}

variable "shared_log_analytics_workspace_id" {
  type    = string
  default = null
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
  default   = null
}

# =============================================================================
# Mystira App Module
# =============================================================================

module "mystira_app" {
  source = "../../../../modules/mystira-app"

  environment         = var.environment
  location            = var.location
  fallback_location   = var.fallback_location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  # Pass shared infrastructure references
  shared_postgresql_server_id                   = var.shared_postgresql_server_id
  shared_postgresql_server_fqdn                 = var.shared_postgresql_server_fqdn
  existing_cosmos_connection_string             = var.shared_cosmos_db_connection_string
  shared_storage_connection_string              = var.shared_storage_connection_string
  shared_log_analytics_workspace_id             = var.shared_log_analytics_workspace_id
  shared_application_insights_connection_string = var.shared_application_insights_connection_string
}

# =============================================================================
# Outputs
# =============================================================================

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = module.mystira_app.static_web_app_url
}

output "api_url" {
  description = "API service URL"
  value       = module.mystira_app.app_service_url
}
