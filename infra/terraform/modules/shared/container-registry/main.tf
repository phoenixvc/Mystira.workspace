# Shared Container Registry Module - Azure
# Cross-environment ACR shared by all environments (dev, staging, prod)
# Separation via image tags: dev/*, staging/*, prod/*

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

variable "resource_group_name" {
  description = "Name of the resource group for ACR"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "southafricanorth"
}

variable "acr_name" {
  description = "Name of the container registry (must be globally unique, no dashes)"
  type        = string
  default     = "myssharedacr"
}

variable "sku" {
  description = "ACR SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Standard"
}

variable "admin_enabled" {
  description = "Enable admin user for ACR"
  type        = bool
  default     = false
}

variable "public_network_access_enabled" {
  description = "Allow public network access (set to true only if private endpoints not available)"
  type        = bool
  default     = false
}

variable "zone_redundancy_enabled" {
  description = "Enable zone redundancy (Premium only)"
  type        = bool
  default     = false
}

variable "georeplications" {
  description = "List of geo-replication locations (Premium only)"
  type = list(object({
    location                = string
    zone_redundancy_enabled = bool
  }))
  default = []
}

variable "retention_policy_days" {
  description = "Number of days to retain untagged manifests (Premium only)"
  type        = number
  default     = 7
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# Private Endpoint Configuration
variable "private_endpoint_enabled" {
  description = "Enable private endpoint for ACR (requires Premium SKU)"
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "Subnet ID for the private endpoint (required if private_endpoint_enabled is true)"
  type        = string
  default     = null
}

variable "private_dns_zone_id" {
  description = "Private DNS zone ID for ACR (azurecr.io). If null, a new zone will be created."
  type        = string
  default     = null
}

variable "virtual_network_id" {
  description = "Virtual network ID for DNS zone link (required if creating new private DNS zone)"
  type        = string
  default     = null
}

locals {
  common_tags = merge(var.tags, {
    Component   = "container-registry"
    Environment = "shared"
    Service     = "acr"
    ManagedBy   = "terraform"
    Project     = "Mystira"
    Shared      = "all-environments"
  })
}

# =============================================================================
# Validation: Private Endpoint Configuration
# =============================================================================
# These checks ensure valid configuration at plan time rather than apply time.

check "private_endpoint_requires_premium_sku" {
  assert {
    condition     = !var.private_endpoint_enabled || var.sku == "Premium"
    error_message = "Private endpoints require Premium SKU. Set sku = \"Premium\" when private_endpoint_enabled = true."
  }
}

check "private_endpoint_requires_subnet" {
  assert {
    condition     = !var.private_endpoint_enabled || (var.private_endpoint_subnet_id != null && var.private_endpoint_subnet_id != "")
    error_message = "private_endpoint_subnet_id is required when private_endpoint_enabled = true."
  }
}

check "network_access_validation" {
  assert {
    condition     = var.public_network_access_enabled || var.private_endpoint_enabled
    error_message = "ACR must have at least one access path: enable public_network_access_enabled OR private_endpoint_enabled."
  }
}

# Container Registry
resource "azurerm_container_registry" "shared" {
  name                          = var.acr_name
  resource_group_name           = var.resource_group_name
  location                      = var.location
  sku                           = var.sku
  admin_enabled                 = var.admin_enabled
  public_network_access_enabled = var.public_network_access_enabled
  zone_redundancy_enabled       = var.sku == "Premium" ? var.zone_redundancy_enabled : false

  # Geo-replication (Premium only)
  dynamic "georeplications" {
    for_each = var.sku == "Premium" ? var.georeplications : []
    content {
      location                = georeplications.value.location
      zone_redundancy_enabled = georeplications.value.zone_redundancy_enabled
    }
  }

  # Note: retention_policy block was removed in AzureRM 4.0
  # Retention is now managed via azurerm_container_registry_task or lifecycle policies

  tags = local.common_tags

  lifecycle {
    prevent_destroy = true
  }
}

# Private DNS Zone for ACR (optional - created if not provided)
resource "azurerm_private_dns_zone" "acr" {
  count = var.private_endpoint_enabled && var.private_dns_zone_id == null ? 1 : 0

  name                = "privatelink.azurecr.io"
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

# Link private DNS zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "acr" {
  count = var.private_endpoint_enabled && var.private_dns_zone_id == null && var.virtual_network_id != null ? 1 : 0

  name                  = "${var.acr_name}-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.acr[0].name
  virtual_network_id    = var.virtual_network_id
  registration_enabled  = false
  tags                  = local.common_tags
}

# Private Endpoint for ACR
resource "azurerm_private_endpoint" "acr" {
  count = var.private_endpoint_enabled ? 1 : 0

  name                = "${var.acr_name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.acr_name}-psc"
    private_connection_resource_id = azurerm_container_registry.shared.id
    is_manual_connection           = false
    subresource_names              = ["registry"]
  }

  private_dns_zone_group {
    name = "acr-dns-zone-group"
    private_dns_zone_ids = [
      var.private_dns_zone_id != null ? var.private_dns_zone_id : azurerm_private_dns_zone.acr[0].id
    ]
  }

  tags = local.common_tags
}

# Outputs
output "acr_id" {
  description = "Container Registry ID"
  value       = azurerm_container_registry.shared.id
}

output "acr_name" {
  description = "Container Registry name"
  value       = azurerm_container_registry.shared.name
}

output "acr_login_server" {
  description = "Container Registry login server"
  value       = azurerm_container_registry.shared.login_server
}

output "acr_admin_username" {
  description = "Container Registry admin username"
  value       = azurerm_container_registry.shared.admin_username
  sensitive   = true
}

output "acr_admin_password" {
  description = "Container Registry admin password"
  value       = azurerm_container_registry.shared.admin_password
  sensitive   = true
}

output "private_endpoint_id" {
  description = "Private endpoint ID (if enabled)"
  value       = var.private_endpoint_enabled ? azurerm_private_endpoint.acr[0].id : null
}

output "private_endpoint_ip" {
  description = "Private endpoint IP address (if enabled)"
  value       = var.private_endpoint_enabled ? azurerm_private_endpoint.acr[0].private_service_connection[0].private_ip_address : null
}

output "private_dns_zone_id" {
  description = "Private DNS zone ID (if created by module)"
  value       = var.private_endpoint_enabled && var.private_dns_zone_id == null ? azurerm_private_dns_zone.acr[0].id : var.private_dns_zone_id
}
