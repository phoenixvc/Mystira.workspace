# =============================================================================
# Admin Services - Dev Environment
# =============================================================================
# Admin API and Admin UI (Static Web App)
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
variable "shared_log_analytics_workspace_id" {
  type = string
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
}

# =============================================================================
# Admin API Module
# =============================================================================

module "admin_api" {
  source = "../../../../modules/admin-api"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  log_analytics_workspace_id        = var.shared_log_analytics_workspace_id
  application_insights_connection_string = var.shared_application_insights_connection_string
}

# =============================================================================
# Admin UI Module
# =============================================================================

module "admin_ui" {
  source = "../../../../modules/admin-ui"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# =============================================================================
# Outputs
# =============================================================================

output "admin_api_url" {
  value = module.admin_api.api_url
}

output "admin_ui_url" {
  value = module.admin_ui.static_web_app_default_hostname
}
