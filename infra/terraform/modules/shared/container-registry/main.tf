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
  description = "Allow public network access"
  type        = bool
  default     = true
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
