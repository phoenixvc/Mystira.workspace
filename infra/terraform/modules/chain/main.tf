# Mystira Chain Infrastructure Module - Azure
# Terraform module for deploying Mystira.Chain blockchain infrastructure on Azure

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.45"
    }
    azapi = {
      source  = "azure/azapi"
      version = "~> 1.10"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.9"
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

variable "chain_node_count" {
  description = "Number of chain nodes to deploy"
  type        = number
  default     = 3
}

variable "chain_vm_size" {
  description = "Azure VM size for chain nodes"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "chain_storage_size_gb" {
  description = "Storage size in GB for chain data"
  type        = number
  default     = 100
}

variable "vnet_id" {
  description = "Virtual Network ID for chain deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for chain nodes"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mys-${var.environment}-mystira-chain"
  region_code = var.region_code
  # Key Vault names must be 3-24 chars, alphanumeric and dashes only
  kv_name     = "mys-${var.environment}-chn-kv-${local.region_code}"
  common_tags = merge(var.tags, {
    Component   = "chain"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

# Network Security Group for Chain Nodes
resource "azurerm_network_security_group" "chain" {
  name                = "${local.name_prefix}-nsg-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name

  # P2P communication between chain nodes
  security_rule {
    name                       = "AllowChainP2P"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "30303"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  # RPC endpoint (internal only)
  security_rule {
    name                       = "AllowRPC"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8545"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  # WebSocket endpoint (internal only)
  security_rule {
    name                       = "AllowWebSocket"
    priority                   = 120
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8546"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  tags = local.common_tags
}

# Managed Identity for Chain Nodes
resource "azurerm_user_assigned_identity" "chain" {
  name                = "${local.name_prefix}-identity-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Storage Account for Chain Data
resource "azurerm_storage_account" "chain" {
  name                     = replace("mys${var.environment}mystirachainstg${local.region_code}", "-", "")
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Premium"
  account_replication_type = var.environment == "prod" ? "ZRS" : "LRS"
  account_kind             = "FileStorage"

  tags = local.common_tags
}

# Wait for storage account to be fully provisioned before creating file shares
resource "time_sleep" "wait_for_storage" {
  depends_on      = [azurerm_storage_account.chain]
  create_duration = "30s"
}

# Azure File Share for Chain Data Persistence
# Using azapi provider to work around azurerm provider bug with Premium FileStorage
resource "azapi_resource" "chain_data" {
  count     = var.chain_node_count
  type      = "Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01"
  name      = "chain-data-${count.index}"
  parent_id = "${azurerm_storage_account.chain.id}/fileServices/default"

  body = {
    properties = {
      shareQuota = var.chain_storage_size_gb
    }
  }

  depends_on = [time_sleep.wait_for_storage]
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "chain" {
  name                = "${local.name_prefix}-log-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

# Key Vault for Chain Secrets
resource "azurerm_key_vault" "chain" {
  name                        = local.kv_name
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = var.environment == "prod"
  sku_name                    = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azurerm_user_assigned_identity.chain.principal_id

    secret_permissions = [
      "Get",
      "List",
    ]
  }

  tags = local.common_tags
}

data "azurerm_client_config" "current" {}

output "nsg_id" {
  description = "Network Security Group ID for chain nodes"
  value       = azurerm_network_security_group.chain.id
}

output "identity_id" {
  description = "Managed Identity ID for chain nodes"
  value       = azurerm_user_assigned_identity.chain.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.chain.principal_id
}

output "storage_account_name" {
  description = "Storage account name for chain data"
  value       = azurerm_storage_account.chain.name
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = azurerm_log_analytics_workspace.chain.id
}

output "key_vault_id" {
  description = "Key Vault ID for chain secrets"
  value       = azurerm_key_vault.chain.id
}
