# Mystira Admin-UI Infrastructure Module - Variables
# Module: infra/terraform/modules/admin-ui
# See main.tf for resource definitions and outputs.tf for outputs

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment (primary)"
  type        = string
  default     = "southafricanorth"
}

variable "fallback_location" {
  description = "Fallback region for resources not available in primary location (SWA)"
  type        = string
  default     = "eastus2"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# Static Web App Settings
variable "static_web_app_sku" {
  description = "SKU for Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "enable_custom_domain" {
  description = "Enable custom domain for Static Web App"
  type        = bool
  default     = false
}

variable "custom_domain" {
  description = "Custom domain for Static Web App (e.g., dev.admin.mystira.app)"
  type        = string
  default     = ""
}
