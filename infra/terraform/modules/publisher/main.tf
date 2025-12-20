# Mystira Publisher Infrastructure Module - Azure
# Terraform module for deploying Mystira.Publisher service infrastructure on Azure

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
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

variable "publisher_replica_count" {
  description = "Number of publisher service replicas"
  type        = number
  default     = 2
}

variable "vnet_id" {
  description = "Virtual Network ID for publisher deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for publisher service"
  type        = string
}

variable "chain_rpc_endpoint" {
  description = "RPC endpoint for Mystira Chain"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mys-${var.environment}-mystira-pub"
  region_code = var.region_code
  common_tags = merge(var.tags, {
    Component   = "publisher"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

# Network Security Group for Publisher Service
resource "azurerm_network_security_group" "publisher" {
  name                = "${local.name_prefix}-nsg-${local.region_code}"
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
    destination_port_range     = "3000"
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
    destination_port_range     = "3001"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  tags = local.common_tags
}

# Managed Identity for Publisher Service
resource "azurerm_user_assigned_identity" "publisher" {
  name                = "${local.name_prefix}-identity"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Service Bus Namespace for Publisher Events
resource "azurerm_servicebus_namespace" "publisher" {
  name                = replace("${local.name_prefix}-queue-${local.region_code}", "-", "")
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.environment == "prod" ? "Premium" : "Standard"

  tags = local.common_tags
}

# Service Bus Queue for Publisher Events
resource "azurerm_servicebus_queue" "publisher_events" {
  name         = "publisher-events"
  namespace_id = azurerm_servicebus_namespace.publisher.id

  max_delivery_count                   = 3
  default_message_ttl                  = "P1D"
  dead_lettering_on_message_expiration = true
  partitioning_enabled                 = var.environment == "prod"
}

# Dead Letter Queue handled automatically by Service Bus

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "publisher" {
  name                = "${local.name_prefix}-logs"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

# Application Insights for Publisher Monitoring
resource "azurerm_application_insights" "publisher" {
  name                = "${local.name_prefix}-ai-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.publisher.id
  application_type    = "Node.JS"

  tags = local.common_tags
}

# Key Vault for Publisher Secrets
resource "azurerm_key_vault" "publisher" {
  name                        = "mys-${var.environment}-pub-kv-${local.region_code}"
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
    object_id = azurerm_user_assigned_identity.publisher.principal_id

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

data "azurerm_client_config" "current" {}

# Store Chain RPC Endpoint in Key Vault
resource "azurerm_key_vault_secret" "chain_rpc_endpoint" {
  name         = "chain-rpc-endpoint"
  value        = var.chain_rpc_endpoint
  key_vault_id = azurerm_key_vault.publisher.id
}

# Redis Cache for Publisher (optional caching layer)
resource "azurerm_redis_cache" "publisher" {
  count               = var.environment == "prod" ? 1 : 0
  name                = "${local.name_prefix}-cache-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  capacity            = 1
  family              = "C"
  sku_name            = "Standard"
  non_ssl_port_enabled = false
  minimum_tls_version = "1.2"

  redis_configuration {
    maxmemory_policy = "volatile-lru"
  }

  tags = local.common_tags
}

output "nsg_id" {
  description = "Network Security Group ID for publisher service"
  value       = azurerm_network_security_group.publisher.id
}

output "identity_id" {
  description = "Managed Identity ID for publisher service"
  value       = azurerm_user_assigned_identity.publisher.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.publisher.principal_id
}

output "servicebus_namespace" {
  description = "Service Bus namespace for publisher events"
  value       = azurerm_servicebus_namespace.publisher.name
}

output "servicebus_queue_name" {
  description = "Service Bus queue name for publisher events"
  value       = azurerm_servicebus_queue.publisher_events.name
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = azurerm_log_analytics_workspace.publisher.id
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.publisher.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for publisher secrets"
  value       = azurerm_key_vault.publisher.id
}
