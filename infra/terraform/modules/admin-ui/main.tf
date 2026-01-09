# Mystira Admin-UI Infrastructure Module - Azure
# Terraform module for deploying Mystira.Admin.UI as a Static Web App
#
# This module provisions:
#   - Azure Static Web App for the Admin UI (React/Vite frontend)
#   - Custom domain configuration (optional)
#
# The Admin UI is a client-side React application that connects to Admin API.
# See ADR-0019 for architecture documentation.

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

# -----------------------------------------------------------------------------
# Variables
# -----------------------------------------------------------------------------

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

variable "region_code" {
  description = "Short region code for naming (san, eus, etc.)"
  type        = string
  default     = "san"
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

# -----------------------------------------------------------------------------
# Local Values
# -----------------------------------------------------------------------------

locals {
  name_prefix          = "mys-${var.environment}-admin-ui"
  fallback_region_code = var.fallback_location == "eastus2" ? "eus2" : var.fallback_location == "eastus" ? "eus" : "glob"

  common_tags = merge(var.tags, {
    Environment = var.environment
    Service     = "admin-ui"
    ManagedBy   = "terraform"
  })
}

# -----------------------------------------------------------------------------
# Static Web App (React/Vite Frontend)
# Note: SWA not available in South Africa North, deployed to fallback region
# -----------------------------------------------------------------------------

resource "azurerm_static_web_app" "admin_ui" {
  name                = "${local.name_prefix}-swa-${local.fallback_region_code}"
  location            = var.fallback_location # SWA not available in all regions
  resource_group_name = var.resource_group_name
  sku_tier            = var.static_web_app_sku
  sku_size            = var.static_web_app_sku

  tags = merge(local.common_tags, {
    Component = "admin-ui"
    Type      = "static-web-app"
  })
}

# Custom domain for Static Web App (optional)
# Note: When using Front Door, custom domain is usually managed there instead
resource "azurerm_static_web_app_custom_domain" "admin_ui" {
  count = var.enable_custom_domain && var.custom_domain != "" ? 1 : 0

  static_web_app_id = azurerm_static_web_app.admin_ui.id
  domain_name       = var.custom_domain
  validation_type   = "cname-delegation"
}

# -----------------------------------------------------------------------------
# Outputs
# -----------------------------------------------------------------------------

output "static_web_app_id" {
  description = "Static Web App ID"
  value       = azurerm_static_web_app.admin_ui.id
}

output "static_web_app_name" {
  description = "Static Web App name"
  value       = azurerm_static_web_app.admin_ui.name
}

output "static_web_app_default_hostname" {
  description = "Static Web App default hostname (use this for Front Door backend)"
  value       = azurerm_static_web_app.admin_ui.default_host_name
}

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = "https://${azurerm_static_web_app.admin_ui.default_host_name}"
}

output "static_web_app_api_key" {
  description = "Static Web App API key for deployments (use in GitHub Actions)"
  value       = azurerm_static_web_app.admin_ui.api_key
  sensitive   = true
}

output "custom_domain" {
  description = "Static Web App custom domain (if enabled)"
  value       = var.enable_custom_domain && var.custom_domain != "" ? var.custom_domain : null
}
