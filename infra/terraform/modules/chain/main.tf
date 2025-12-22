# Mystira Chain Infrastructure Module - Azure
# Terraform module for deploying Mystira.Chain blockchain infrastructure on Azure

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"  # 4.x required for .NET 9.0 support
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
  description = "Storage size in GB for chain data (minimum 100 GB for Premium file shares)"
  type        = number
  default     = 100

  validation {
    condition     = var.chain_storage_size_gb >= 100
    error_message = "Premium file shares require a minimum quota of 100 GB."
  }
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

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace (from shared monitoring module)"
  type        = string
}

locals {
  name_prefix = "mys-${var.environment}-chain"
  region_code = var.region_code
  # Key Vault names must be 3-24 chars, alphanumeric and dashes only
  kv_name = "mys-${var.environment}-chain-kv-${local.region_code}"
  common_tags = merge(var.tags, {
    Component   = "chain"
    Environment = var.environment
    Service     = "chain"
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
  name                          = replace("mys${var.environment}mystirachainstg${local.region_code}", "-", "")
  resource_group_name           = var.resource_group_name
  location                      = var.location
  account_tier                  = "Premium"
  account_replication_type      = var.environment == "prod" ? "ZRS" : "LRS"
  account_kind                  = "FileStorage"
  https_traffic_only_enabled    = true
  min_tls_version               = "TLS1_2"
  public_network_access_enabled = true

  tags = local.common_tags
}

# Wait for storage account to be fully provisioned before creating file shares
resource "time_sleep" "wait_for_storage" {
  depends_on      = [azurerm_storage_account.chain]
  create_duration = "60s"
}

# Azure File Share for Chain Data Persistence
# Using data plane API (az storage share create) instead of ARM API to work around InvalidHeaderValue bug
resource "terraform_data" "chain_data_share" {
  count = var.chain_node_count

  triggers_replace = {
    resource_group  = var.resource_group_name
    storage_account = azurerm_storage_account.chain.name
    share_name      = "chain-data-${count.index}"
    quota           = var.chain_storage_size_gb
  }

  provisioner "local-exec" {
    command     = <<-EOT
      # Get storage account key and use data plane API
      STORAGE_KEY=$(az storage account keys list \
        --resource-group "${var.resource_group_name}" \
        --account-name "${azurerm_storage_account.chain.name}" \
        --query '[0].value' -o tsv)

      az storage share create \
        --name "chain-data-${count.index}" \
        --account-name "${azurerm_storage_account.chain.name}" \
        --account-key "$STORAGE_KEY" \
        --quota ${var.chain_storage_size_gb} \
        --output none
    EOT
    interpreter = ["/bin/bash", "-c"]
  }

  provisioner "local-exec" {
    when        = destroy
    command     = <<-EOT
      # Get storage account key for deletion
      STORAGE_KEY=$(az storage account keys list \
        --resource-group "${self.triggers_replace.resource_group}" \
        --account-name "${self.triggers_replace.storage_account}" \
        --query '[0].value' -o tsv 2>/dev/null) || true

      if [ -n "$STORAGE_KEY" ]; then
        az storage share delete \
          --name "${self.triggers_replace.share_name}" \
          --account-name "${self.triggers_replace.storage_account}" \
          --account-key "$STORAGE_KEY" \
          --output none || true
      fi
    EOT
    interpreter = ["/bin/bash", "-c"]
  }

  depends_on = [time_sleep.wait_for_storage]
}

# Application Insights for Chain (uses shared Log Analytics workspace)
resource "azurerm_application_insights" "chain" {
  name                = "${local.name_prefix}-ai-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = var.shared_log_analytics_workspace_id
  application_type    = "other"

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

output "application_insights_id" {
  description = "Application Insights ID for chain monitoring"
  value       = azurerm_application_insights.chain.id
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.chain.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for chain secrets"
  value       = azurerm_key_vault.chain.id
}
