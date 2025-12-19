# Mystira Story-Generator Infrastructure Module - Azure
# Terraform module for deploying Mystira.StoryGenerator service infrastructure on Azure

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
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
  description = "Virtual Network ID for story-generator deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for story-generator service"
  type        = string
}

variable "shared_postgresql_server_id" {
  description = "ID of shared PostgreSQL server (from shared/postgresql module)"
  type        = string
  default     = null
}

variable "shared_redis_cache_id" {
  description = "ID of shared Redis cache (from shared/redis module)"
  type        = string
  default     = null
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace for monitoring integration"
  type        = string
  default     = null
}

variable "use_shared_postgresql" {
  description = "Whether to use shared PostgreSQL instead of creating dedicated one"
  type        = bool
  default     = false
}

variable "use_shared_redis" {
  description = "Whether to use shared Redis instead of creating dedicated one"
  type        = bool
  default     = false
}

variable "use_shared_log_analytics" {
  description = "Whether to use shared Log Analytics instead of creating dedicated one"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mys-${var.environment}-mystira-sg"
  region_code = var.region_code
  common_tags = merge(var.tags, {
    Component   = "story-generator"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
}

# Network Security Group for Story-Generator Service
resource "azurerm_network_security_group" "story_generator" {
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

# Managed Identity for Story-Generator Service
resource "azurerm_user_assigned_identity" "story_generator" {
  name                = "${local.name_prefix}-identity-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# PostgreSQL Database (if not using shared)
resource "azurerm_postgresql_flexible_server" "story_generator" {
  count                  = var.use_shared_postgresql ? 0 : 1
  name                   = "${local.name_prefix}-pg-${local.region_code}"
  location               = var.location
  resource_group_name    = var.resource_group_name
  version                = "15"
  delegated_subnet_id    = var.subnet_id
  private_dns_zone_id    = azurerm_private_dns_zone.postgres[0].id
  administrator_login    = "mystira_admin"
  administrator_password = random_password.postgres[0].result
  zone                   = "1"

  sku_name = var.environment == "prod" ? "GP_Standard_D2s_v3" : "B_Standard_B1ms"

  storage_mb                    = var.environment == "prod" ? 32768 : 32768
  backup_retention_days         = var.environment == "prod" ? 35 : 7
  geo_redundant_backup_enabled  = var.environment == "prod"

  tags = local.common_tags
}

# Private DNS Zone for PostgreSQL (if not using shared)
resource "azurerm_private_dns_zone" "postgres" {
  count               = var.use_shared_postgresql ? 0 : 1
  name                = "${local.name_prefix}-pg-${local.region_code}.postgres.database.azure.com"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Private DNS Zone Virtual Network Link (if not using shared)
resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  count                 = var.use_shared_postgresql ? 0 : 1
  name                  = "${local.name_prefix}-pg-${local.region_code}-vnet-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgres[0].name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false

  tags = local.common_tags
}

# PostgreSQL Database (if not using shared)
resource "azurerm_postgresql_flexible_server_database" "story_generator" {
  count     = var.use_shared_postgresql ? 0 : 1
  name      = "storygenerator"
  server_id = azurerm_postgresql_flexible_server.story_generator[0].id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# Random password for PostgreSQL (if not using shared)
resource "random_password" "postgres" {
  count   = var.use_shared_postgresql ? 0 : 1
  length  = 32
  special = true
}

# Redis Cache (if not using shared)
resource "azurerm_redis_cache" "story_generator" {
  count               = var.use_shared_redis ? 0 : 1
  name                = "${local.name_prefix}-cache-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  capacity            = var.environment == "prod" ? 2 : 1
  family              = var.environment == "prod" ? "C" : "C"
  sku_name            = var.environment == "prod" ? "Standard" : "Basic"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"
  subnet_id           = var.subnet_id

  redis_configuration {
    maxmemory_policy = "volatile-lru"
  }

  tags = local.common_tags
}

# Log Analytics Workspace (if not using shared)
resource "azurerm_log_analytics_workspace" "story_generator" {
  count               = var.use_shared_log_analytics ? 0 : 1
  name                = "${local.name_prefix}-log-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = local.common_tags
}

# Application Insights for Story-Generator Monitoring
resource "azurerm_application_insights" "story_generator" {
  name                = "${local.name_prefix}-ai-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = var.use_shared_log_analytics ? var.shared_log_analytics_workspace_id : azurerm_log_analytics_workspace.story_generator[0].id
  application_type    = "other"

  tags = local.common_tags
}

# Key Vault for Story-Generator Secrets
resource "azurerm_key_vault" "story_generator" {
  name                        = "mys-${var.environment}-sg-kv-${local.region_code}"
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = false
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = var.environment == "prod"
  sku_name                    = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azurerm_user_assigned_identity.story_generator.principal_id

    secret_permissions = [
      "Get",
      "List",
    ]
  }

  tags = local.common_tags
}

data "azurerm_client_config" "current" {}

# Store PostgreSQL connection string in Key Vault
# Note: When using shared PostgreSQL, connection string must be provided via environment outputs
# or the shared module's connection_strings output should be used
resource "azurerm_key_vault_secret" "postgres_connection_string" {
  count        = var.use_shared_postgresql ? 0 : 1
  name         = "postgres-connection-string"
  value        = "Host=${azurerm_postgresql_flexible_server.story_generator[0].fqdn};Port=5432;Username=${azurerm_postgresql_flexible_server.story_generator[0].administrator_login};Password=${random_password.postgres[0].result};Database=${azurerm_postgresql_flexible_server_database.story_generator[0].name};SSLMode=Require;Trust Server Certificate=true"
  key_vault_id = azurerm_key_vault.story_generator.id
}

# Store Redis connection string in Key Vault
# Note: When using shared Redis, connection string must be provided via environment outputs
# or the shared module's connection string output should be used
resource "azurerm_key_vault_secret" "redis_connection_string" {
  count        = var.use_shared_redis ? 0 : 1
  name         = "redis-connection-string"
  value        = azurerm_redis_cache.story_generator[0].primary_connection_string
  key_vault_id = azurerm_key_vault.story_generator.id
}

output "nsg_id" {
  description = "Network Security Group ID for story-generator service"
  value       = azurerm_network_security_group.story_generator.id
}

output "identity_id" {
  description = "Managed Identity ID for story-generator service"
  value       = azurerm_user_assigned_identity.story_generator.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.story_generator.principal_id
}

output "postgresql_server_id" {
  description = "PostgreSQL server ID (shared or dedicated)"
  value       = var.use_shared_postgresql ? var.shared_postgresql_server_id : azurerm_postgresql_flexible_server.story_generator[0].id
}

output "postgresql_database_name" {
  description = "PostgreSQL database name"
  value       = var.use_shared_postgresql ? "storygenerator" : azurerm_postgresql_flexible_server_database.story_generator[0].name
}

output "redis_cache_id" {
  description = "Redis cache ID (shared or dedicated)"
  value       = var.use_shared_redis ? var.shared_redis_cache_id : azurerm_redis_cache.story_generator[0].id
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID (shared or dedicated)"
  value       = var.use_shared_log_analytics ? var.shared_log_analytics_workspace_id : azurerm_log_analytics_workspace.story_generator[0].id
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.story_generator.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for story-generator secrets"
  value       = azurerm_key_vault.story_generator.id
}

