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

  tags = local.common_tags
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
