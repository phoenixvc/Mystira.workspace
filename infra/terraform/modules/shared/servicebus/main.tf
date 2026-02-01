# Shared Service Bus Infrastructure Module - Azure
# Terraform module for deploying shared Service Bus messaging infrastructure
# Used by multiple services: publisher, admin-api, app, future services

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "southafricanorth"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "sku" {
  description = "Service Bus SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Standard"
}

variable "capacity" {
  description = "Premium tier capacity (1, 2, 4, 8, 16). Only for Premium SKU."
  type        = number
  default     = 1
}

variable "zone_redundant" {
  description = "Enable zone redundancy (Premium only)"
  type        = bool
  default     = false
}

variable "queues" {
  description = "Map of queues to create"
  type = map(object({
    max_delivery_count                   = optional(number, 10)
    default_message_ttl                  = optional(string, "P14D")
    dead_lettering_on_message_expiration = optional(bool, true)
    partitioning_enabled                 = optional(bool, false)
    max_size_in_megabytes                = optional(number, 1024)
  }))
  default = {}
}

variable "topics" {
  description = "Map of topics to create"
  type = map(object({
    default_message_ttl   = optional(string, "P14D")
    partitioning_enabled  = optional(bool, false)
    max_size_in_megabytes = optional(number, 1024)
  }))
  default = {}
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# Private Endpoint Configuration
variable "private_endpoint_enabled" {
  description = "Enable private endpoint for Service Bus (requires Premium SKU)"
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "Subnet ID for the private endpoint (required if private_endpoint_enabled is true)"
  type        = string
  default     = null
}

variable "private_dns_zone_id" {
  description = "Private DNS zone ID for Service Bus (servicebus.windows.net). If null, a new zone will be created."
  type        = string
  default     = null
}

variable "virtual_network_id" {
  description = "Virtual network ID for DNS zone link (required if creating new private DNS zone)"
  type        = string
  default     = null
}

variable "public_network_access_enabled" {
  description = "Allow public network access (set to false when using private endpoints)"
  type        = bool
  default     = false
}

locals {
  name_prefix = "mys-${var.environment}-core"
  region_code = "san"
  common_tags = merge(var.tags, {
    Component   = "shared-servicebus"
    Environment = var.environment
    Service     = "core"
    ManagedBy   = "terraform"
    Project     = "Mystira"
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

check "private_endpoint_dns_requires_vnet" {
  assert {
    condition     = !var.private_endpoint_enabled || var.private_dns_zone_id != null || var.virtual_network_id != null
    error_message = "virtual_network_id is required when private_endpoint_enabled = true and private_dns_zone_id = null (to link the created private DNS zone)."
  }
}

check "network_access_validation" {
  assert {
    condition     = var.public_network_access_enabled || var.private_endpoint_enabled
    error_message = "Service Bus namespace must have at least one access path: enable public_network_access_enabled OR private_endpoint_enabled."
  }
}

# Service Bus Namespace
resource "azurerm_servicebus_namespace" "shared" {
  name                = replace("${local.name_prefix}-sb-${local.region_code}", "-", "")
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  capacity            = var.sku == "Premium" ? var.capacity : null

  # Premium-only features
  premium_messaging_partitions = var.sku == "Premium" ? 1 : null
  # Note: zone_redundant was removed in AzureRM 4.0 - zone redundancy is now automatic for Premium

  # Network access control
  public_network_access_enabled = var.public_network_access_enabled

  tags = local.common_tags

  lifecycle {
    prevent_destroy = true
  }
}

# Dynamic queues based on variable
resource "azurerm_servicebus_queue" "queues" {
  for_each = var.queues

  name         = each.key
  namespace_id = azurerm_servicebus_namespace.shared.id

  max_delivery_count                   = each.value.max_delivery_count
  default_message_ttl                  = each.value.default_message_ttl
  dead_lettering_on_message_expiration = each.value.dead_lettering_on_message_expiration
  partitioning_enabled                 = var.sku != "Premium" ? each.value.partitioning_enabled : false
  max_size_in_megabytes                = each.value.max_size_in_megabytes
}

# Dynamic topics based on variable
resource "azurerm_servicebus_topic" "topics" {
  for_each = var.topics

  name         = each.key
  namespace_id = azurerm_servicebus_namespace.shared.id

  default_message_ttl   = each.value.default_message_ttl
  partitioning_enabled  = var.sku != "Premium" ? each.value.partitioning_enabled : false
  max_size_in_megabytes = each.value.max_size_in_megabytes
}

# Private DNS Zone for Service Bus (optional - created if not provided)
resource "azurerm_private_dns_zone" "servicebus" {
  count = var.private_endpoint_enabled && var.private_dns_zone_id == null ? 1 : 0

  name                = "privatelink.servicebus.windows.net"
  resource_group_name = var.resource_group_name
  tags                = local.common_tags
}

# Link private DNS zone to VNet
resource "azurerm_private_dns_zone_virtual_network_link" "servicebus" {
  count = var.private_endpoint_enabled && var.private_dns_zone_id == null && var.virtual_network_id != null ? 1 : 0

  name                  = "${azurerm_servicebus_namespace.shared.name}-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.servicebus[0].name
  virtual_network_id    = var.virtual_network_id
  registration_enabled  = false
  tags                  = local.common_tags
}

# Private Endpoint for Service Bus
resource "azurerm_private_endpoint" "servicebus" {
  count = var.private_endpoint_enabled ? 1 : 0

  name                = "${azurerm_servicebus_namespace.shared.name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${azurerm_servicebus_namespace.shared.name}-psc"
    private_connection_resource_id = azurerm_servicebus_namespace.shared.id
    is_manual_connection           = false
    subresource_names              = ["namespace"]
  }

  private_dns_zone_group {
    name = "servicebus-dns-zone-group"
    private_dns_zone_ids = [
      var.private_dns_zone_id != null ? var.private_dns_zone_id : azurerm_private_dns_zone.servicebus[0].id
    ]
  }

  tags = local.common_tags
}

# Outputs
output "namespace_id" {
  description = "Service Bus namespace ID"
  value       = azurerm_servicebus_namespace.shared.id
}

output "namespace_name" {
  description = "Service Bus namespace name"
  value       = azurerm_servicebus_namespace.shared.name
}

output "namespace_endpoint" {
  description = "Service Bus namespace endpoint"
  value       = azurerm_servicebus_namespace.shared.endpoint
}

output "default_primary_connection_string" {
  description = "Primary connection string for the namespace"
  value       = azurerm_servicebus_namespace.shared.default_primary_connection_string
  sensitive   = true
}

output "default_secondary_connection_string" {
  description = "Secondary connection string for the namespace"
  value       = azurerm_servicebus_namespace.shared.default_secondary_connection_string
  sensitive   = true
}

output "default_primary_key" {
  description = "Primary key for the namespace"
  value       = azurerm_servicebus_namespace.shared.default_primary_key
  sensitive   = true
}

output "queue_ids" {
  description = "Map of queue names to their IDs"
  value       = { for k, v in azurerm_servicebus_queue.queues : k => v.id }
}

output "topic_ids" {
  description = "Map of topic names to their IDs"
  value       = { for k, v in azurerm_servicebus_topic.topics : k => v.id }
}

output "private_endpoint_id" {
  description = "Private endpoint ID (if enabled)"
  value       = var.private_endpoint_enabled ? azurerm_private_endpoint.servicebus[0].id : null
}

output "private_endpoint_ip" {
  description = "Private endpoint IP address (if enabled)"
  value       = var.private_endpoint_enabled ? azurerm_private_endpoint.servicebus[0].private_service_connection[0].private_ip_address : null
}

output "private_dns_zone_id" {
  description = "Private DNS zone ID (if created by module)"
  value       = var.private_endpoint_enabled && var.private_dns_zone_id == null ? azurerm_private_dns_zone.servicebus[0].id : var.private_dns_zone_id
}
