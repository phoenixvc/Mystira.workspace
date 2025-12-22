# Mystira Admin-API Infrastructure Module - Azure
# Terraform module for deploying Mystira.AdminAPI service infrastructure on Azure
# This is a .NET Web API that uses Microsoft Entra ID for authentication

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"  # 4.x required for .NET 9.0 support
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
  default     = "eastus"
}

variable "region_code" {
  description = "Short region code (eus, euw, etc.) - defaults to 'eus' for eastus"
  type        = string
  default     = "eus"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "vnet_id" {
  description = "Virtual Network ID for admin-api deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for admin-api service"
  type        = string
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace (from shared monitoring module)"
  type        = string
}

variable "shared_postgresql_server_id" {
  description = "ID of shared PostgreSQL server (from shared/postgresql module)"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mys-${var.environment}-admin"
  region_code = var.region_code
  common_tags = merge(var.tags, {
    Component   = "admin-api"
    Environment = var.environment
    Service     = "admin"
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

data "azurerm_client_config" "current" {}

# Network Security Group for Admin-API Service
resource "azurerm_network_security_group" "admin_api" {
  name                = "${local.name_prefix}-api-nsg-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # HTTP API endpoint
  security_rule {
    name                       = "AllowHTTP"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8080"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  # Health check endpoint
  security_rule {
    name                       = "AllowHealthCheck"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8081"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  tags = local.common_tags
}

# Managed Identity for Admin-API Service
# This identity is used for:
# - Workload identity in AKS (via federated credential)
# - Key Vault access
# - PostgreSQL Azure AD authentication
resource "azurerm_user_assigned_identity" "admin_api" {
  name                = "${local.name_prefix}-api-identity-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Application Insights for Admin-API Monitoring (uses shared Log Analytics workspace)
resource "azurerm_application_insights" "admin_api" {
  name                = "${local.name_prefix}-api-ai-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = var.shared_log_analytics_workspace_id
  application_type    = "web"

  tags = local.common_tags
}

# Key Vault for Admin-API Secrets
resource "azurerm_key_vault" "admin_api" {
  name                        = "mys-${var.environment}-adm-kv-${local.region_code}"
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = false
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = var.environment == "prod"
  sku_name                    = "standard"

  # Access policy for managed identity (read-only)
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azurerm_user_assigned_identity.admin_api.principal_id

    secret_permissions = [
      "Get",
      "List",
    ]
  }

  # Access policy for Terraform service principal (manage secrets)
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get",
      "List",
      "Set",
      "Delete",
      "Purge",
      "Recover",
    ]
  }

  tags = local.common_tags
}

# Outputs
output "nsg_id" {
  description = "Network Security Group ID for admin-api service"
  value       = azurerm_network_security_group.admin_api.id
}

output "identity_id" {
  description = "Managed Identity ID for admin-api service"
  value       = azurerm_user_assigned_identity.admin_api.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.admin_api.principal_id
}

output "identity_client_id" {
  description = "Managed Identity Client ID (for workload identity)"
  value       = azurerm_user_assigned_identity.admin_api.client_id
}

output "application_insights_id" {
  description = "Application Insights ID for admin-api monitoring"
  value       = azurerm_application_insights.admin_api.id
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.admin_api.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for admin-api secrets"
  value       = azurerm_key_vault.admin_api.id
}

output "key_vault_uri" {
  description = "Key Vault URI for admin-api"
  value       = azurerm_key_vault.admin_api.vault_uri
}
