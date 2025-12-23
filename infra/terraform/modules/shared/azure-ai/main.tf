# Azure AI Foundry (Azure OpenAI Service) Module
# Provides shared Azure AI resources for all services
#
# This module creates:
# - Azure OpenAI Service account
# - Model deployments (GPT-4, GPT-4o, etc.)
# - Private endpoint (optional)

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

# Local variables
locals {
  name = "mys-${var.environment}-ai-${var.region_code}"

  common_tags = merge(var.tags, {
    Component   = "azure-ai"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

# =============================================================================
# Azure OpenAI Service (Azure AI Foundry)
# =============================================================================

resource "azurerm_cognitive_account" "openai" {
  name                  = local.name
  location              = var.location
  resource_group_name   = var.resource_group_name
  kind                  = "OpenAI"
  sku_name              = var.sku_name
  custom_subdomain_name = local.name

  # Network rules
  dynamic "network_acls" {
    for_each = var.enable_network_rules ? [1] : []
    content {
      default_action = var.network_default_action
      ip_rules       = var.allowed_ip_ranges
    }
  }

  # Identity for managed identity access
  identity {
    type = "SystemAssigned"
  }

  # Responsible AI acknowledgment (required for Azure OpenAI)
  local_auth_enabled                 = true
  public_network_access_enabled      = var.public_network_access_enabled
  outbound_network_access_restricted = false

  tags = local.common_tags
}

# =============================================================================
# Model Deployments
# =============================================================================

resource "azurerm_cognitive_deployment" "models" {
  for_each = var.model_deployments

  name                 = each.key
  cognitive_account_id = azurerm_cognitive_account.openai.id

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }
}

# =============================================================================
# Private Endpoint (Optional)
# =============================================================================

resource "azurerm_private_endpoint" "openai" {
  count = var.enable_private_endpoint ? 1 : 0

  name                = "${local.name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${local.name}-psc"
    private_connection_resource_id = azurerm_cognitive_account.openai.id
    is_manual_connection           = false
    subresource_names              = ["account"]
  }

  tags = local.common_tags
}

# =============================================================================
# Private DNS Zone (Optional)
# =============================================================================

resource "azurerm_private_dns_zone" "openai" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                = "privatelink.openai.azure.com"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "openai" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                  = "${local.name}-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.openai[0].name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_a_record" "openai" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                = azurerm_cognitive_account.openai.name
  zone_name           = azurerm_private_dns_zone.openai[0].name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [azurerm_private_endpoint.openai[0].private_service_connection[0].private_ip_address]
}
