# =============================================================================
# Admin Services - Staging Environment
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
variable "shared_postgresql_server_id" {
  description = "ID of shared PostgreSQL server"
  type        = string
  default     = null
}

variable "shared_log_analytics_workspace_id" {
  type = string
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
}

# Admin UI configuration
variable "admin_ui_sku" {
  description = "SKU for Static Web App (Free or Standard)"
  type        = string
  default     = "Standard"
}

variable "enable_custom_domain" {
  description = "Enable custom domain for Static Web App"
  type        = bool
  default     = true
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

  shared_log_analytics_workspace_id = var.shared_log_analytics_workspace_id
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

  # UI configuration from terragrunt inputs
  static_web_app_sku   = var.admin_ui_sku
  enable_custom_domain = var.enable_custom_domain
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
