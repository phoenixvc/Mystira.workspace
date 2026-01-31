# Mystira Admin-UI Infrastructure Module - Azure
# Terraform module for deploying Mystira.Admin.UI as a Static Web App
#
# This module provisions:
#   - Azure Static Web App for the Admin UI (React/Vite frontend)
#   - Custom domain configuration (optional)
#
# The Admin UI is a client-side React application that connects to Admin API.
# See ADR-0019 for architecture documentation.
#
# Related files:
#   - variables.tf: Input variable definitions
#   - outputs.tf: Output value definitions

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

  lifecycle {
    prevent_destroy = true
  }
}

# Custom domain for Static Web App (optional)
# Note: When using Front Door, custom domain is usually managed there instead
resource "azurerm_static_web_app_custom_domain" "admin_ui" {
  count = var.enable_custom_domain && var.custom_domain != "" ? 1 : 0

  static_web_app_id = azurerm_static_web_app.admin_ui.id
  domain_name       = var.custom_domain
  validation_type   = "cname-delegation"
}
