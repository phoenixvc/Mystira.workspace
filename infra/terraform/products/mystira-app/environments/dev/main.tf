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

variable "shared_redis_hostname" {
  type = string
}

variable "shared_cosmos_db_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_cosmos_db_endpoint" {
  type = string
}

variable "shared_storage_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_storage_blob_endpoint" {
  type = string
}

variable "shared_log_analytics_workspace_id" {
  type = string
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_application_insights_id" {
  type    = string
  default = ""
}

variable "use_shared_monitoring" {
  type    = bool
  default = true
}

variable "enable_redis" {
  type    = bool
  default = true
}

variable "use_shared_redis" {
  type    = bool
  default = true
}

variable "shared_acs_connection_string" {
  type      = string
  sensitive = true
  default   = ""
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

  shared_postgresql_server_id   = var.shared_postgresql_server_id
  shared_postgresql_server_fqdn = var.shared_postgresql_server_fqdn

  skip_cosmos_creation              = true
  existing_cosmos_connection_string = var.shared_cosmos_db_connection_string
  shared_cosmos_endpoint            = var.shared_cosmos_db_endpoint
  shared_cosmos_database_name       = "MystiraAppDb"

  skip_storage_creation            = true
  shared_storage_connection_string = var.shared_storage_connection_string
  shared_storage_blob_endpoint     = var.shared_storage_blob_endpoint

  use_shared_monitoring                         = var.use_shared_monitoring
  shared_log_analytics_workspace_id             = var.shared_log_analytics_workspace_id
  shared_application_insights_id                = var.shared_application_insights_id
  shared_application_insights_connection_string = var.shared_application_insights_connection_string

  enable_redis          = var.enable_redis
  use_shared_redis      = var.use_shared_redis
  shared_redis_hostname = var.shared_redis_hostname

  enable_communication_services = false
  use_shared_acs                = length(trimspace(var.shared_acs_connection_string)) > 0
  shared_acs_connection_string  = var.shared_acs_connection_string
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
