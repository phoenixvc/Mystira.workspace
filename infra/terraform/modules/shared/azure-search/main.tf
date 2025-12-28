# Azure AI Search Module (formerly Cognitive Search)
# Provides shared search infrastructure for RAG, vector search, and semantic search
#
# This module creates:
# - Azure AI Search service
# - Optional semantic search configuration
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
  name = "mys-${var.environment}-search-${var.region_code}"

  common_tags = merge(var.tags, {
    Component   = "azure-ai-search"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

# =============================================================================
# Azure AI Search Service
# =============================================================================
# Pricing tiers:
# - free: 50MB storage, 3 indexes (dev/testing only)
# - basic: 2GB storage, 15 indexes, no semantic search
# - standard: 25GB storage, 50 indexes, semantic search available
# - standard2: 100GB storage, 200 indexes
# - standard3: 200GB storage, 200 indexes, high density option

resource "azurerm_search_service" "main" {
  name                = local.name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku

  # Replica and partition configuration (for standard+ tiers)
  replica_count   = var.replica_count
  partition_count = var.partition_count

  # Security settings
  public_network_access_enabled = var.public_network_access_enabled
  local_authentication_enabled  = var.local_authentication_enabled

  # Semantic search (requires standard tier or higher)
  # Convert "disabled" to null for Azure API
  semantic_search_sku = var.semantic_search_sku == "disabled" ? null : var.semantic_search_sku

  # Managed identity for secure access to other Azure resources
  identity {
    type = "SystemAssigned"
  }

  tags = local.common_tags
}

# =============================================================================
# Private Endpoint (Optional)
# =============================================================================

resource "azurerm_private_endpoint" "search" {
  count = var.enable_private_endpoint ? 1 : 0

  name                = "${local.name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${local.name}-psc"
    private_connection_resource_id = azurerm_search_service.main.id
    is_manual_connection           = false
    subresource_names              = ["searchService"]
  }

  tags = local.common_tags
}

# =============================================================================
# Private DNS Zone (Optional)
# =============================================================================

resource "azurerm_private_dns_zone" "search" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                = "privatelink.search.windows.net"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "search" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                  = "${local.name}-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.search[0].name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_a_record" "search" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                = azurerm_search_service.main.name
  zone_name           = azurerm_private_dns_zone.search[0].name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [azurerm_private_endpoint.search[0].private_service_connection[0].private_ip_address]
}
