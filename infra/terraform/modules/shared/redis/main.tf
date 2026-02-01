# Shared Redis Infrastructure Module - Azure
# Terraform module for deploying shared Redis cache infrastructure

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0" # 4.x required for .NET 9.0 support
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "eastus"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for Redis cache (required for Premium tier)"
  type        = string
  default     = null
}

variable "capacity" {
  description = "Redis cache capacity (0-6 for Basic/Standard C family, 1-5 for Premium P family)"
  type        = number
  default     = 1

  validation {
    condition     = var.capacity >= 0 && var.capacity <= 6
    error_message = "Capacity must be between 0 and 6 for Basic/Standard (C family) or 1-5 for Premium (P family)."
  }
}

variable "family" {
  description = "Redis cache family (C for Basic/Standard, P for Premium)"
  type        = string
  default     = "C"

  validation {
    condition     = contains(["C", "P"], var.family)
    error_message = "Family must be one of: C (for Basic/Standard), P (for Premium)."
  }
}

variable "sku_name" {
  description = "Redis cache SKU name (Basic, Standard, Premium)"
  type        = string
  default     = "Standard"

  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.sku_name)
    error_message = "SKU name must be one of: Basic, Standard, Premium."
  }
}

variable "non_ssl_port_enabled" {
  description = "Enable non-SSL port"
  type        = bool
  default     = false
}

variable "minimum_tls_version" {
  description = "Minimum TLS version"
  type        = string
  default     = "1.2"
}

variable "maxmemory_policy" {
  description = "Redis maxmemory policy"
  type        = string
  default     = "volatile-lru"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mys-${var.environment}-core"
  common_tags = merge(var.tags, {
    Component   = "shared-redis"
    Environment = var.environment
    Service     = "core"
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

# Redis Cache
# Note: subnet_id can only be used with Premium SKU
resource "azurerm_redis_cache" "shared" {
  name                 = "${local.name_prefix}-cache"
  location             = var.location
  resource_group_name  = var.resource_group_name
  capacity             = var.capacity
  family               = var.family
  sku_name             = var.sku_name
  non_ssl_port_enabled = var.non_ssl_port_enabled
  minimum_tls_version  = var.minimum_tls_version
  subnet_id            = var.sku_name == "Premium" ? var.subnet_id : null
  shard_count          = var.sku_name == "Premium" && var.capacity > 1 ? var.capacity : null

  redis_configuration {
    maxmemory_policy = var.maxmemory_policy
  }

  tags = local.common_tags

  lifecycle {
    prevent_destroy = true
  }
}

# Redis Firewall Rules (allow Azure services)
resource "azurerm_redis_firewall_rule" "allow_azure_services" {
  count               = var.sku_name == "Premium" ? 1 : 0
  name                = "AllowAzureServices"
  redis_cache_name    = azurerm_redis_cache.shared.name
  resource_group_name = var.resource_group_name
  start_ip            = "0.0.0.0"
  end_ip              = "0.0.0.0"
}

output "cache_id" {
  description = "Redis cache ID"
  value       = azurerm_redis_cache.shared.id
}

output "cache_name" {
  description = "Redis cache name"
  value       = azurerm_redis_cache.shared.name
}

output "hostname" {
  description = "Redis cache hostname"
  value       = azurerm_redis_cache.shared.hostname
}

output "ssl_port" {
  description = "Redis cache SSL port"
  value       = azurerm_redis_cache.shared.ssl_port
}

output "port" {
  description = "Redis cache port"
  value       = azurerm_redis_cache.shared.port
}

output "primary_access_key" {
  description = "Redis cache primary access key"
  value       = azurerm_redis_cache.shared.primary_access_key
  sensitive   = true
}

output "secondary_access_key" {
  description = "Redis cache secondary access key"
  value       = azurerm_redis_cache.shared.secondary_access_key
  sensitive   = true
}

output "primary_connection_string" {
  description = "Redis cache primary connection string"
  value       = azurerm_redis_cache.shared.primary_connection_string
  sensitive   = true
}

output "secondary_connection_string" {
  description = "Redis cache secondary connection string"
  value       = azurerm_redis_cache.shared.secondary_connection_string
  sensitive   = true
}

