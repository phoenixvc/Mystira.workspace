# Shared PostgreSQL Infrastructure Module - Azure
# Terraform module for deploying shared PostgreSQL database infrastructure

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

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "vnet_id" {
  description = "Virtual Network ID for PostgreSQL deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for PostgreSQL server"
  type        = string
}

variable "admin_login" {
  description = "PostgreSQL administrator login"
  type        = string
  default     = "mystira_admin"
}

variable "admin_password" {
  description = "PostgreSQL administrator password (if not provided, will be generated)"
  type        = string
  sensitive   = true
  default     = null
}

variable "postgres_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "15"
}

variable "sku_name" {
  description = "PostgreSQL SKU name"
  type        = string
  default     = null
}

variable "storage_mb" {
  description = "Storage size in MB"
  type        = number
  default     = 32768
}

variable "backup_retention_days" {
  description = "Backup retention in days"
  type        = number
  default     = 7
}

variable "geo_redundant_backup_enabled" {
  description = "Enable geo-redundant backup"
  type        = bool
  default     = false
}

variable "databases" {
  description = "List of database names to create"
  type        = list(string)
  default     = []
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mystira-shared-pg-${var.environment}"
  common_tags = merge(var.tags, {
    Component   = "shared-postgresql"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
  
  sku_name_final = var.sku_name != null ? var.sku_name : (
    var.environment == "prod" ? "GP_Standard_D4s_v3" : "B_Standard_B1ms"
  )
}

# Private DNS Zone for PostgreSQL
resource "azurerm_private_dns_zone" "postgres" {
  name                = "${local.name_prefix}.postgres.database.azure.com"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# Private DNS Zone Virtual Network Link
resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  name                  = "${local.name_prefix}-vnet-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgres.name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false

  tags = local.common_tags
}

# PostgreSQL Flexible Server
resource "azurerm_postgresql_flexible_server" "shared" {
  name                   = "${local.name_prefix}-server"
  location               = var.location
  resource_group_name    = var.resource_group_name
  version                = var.postgres_version
  delegated_subnet_id    = var.subnet_id
  private_dns_zone_id    = azurerm_private_dns_zone.postgres.id
  administrator_login    = var.admin_login
  administrator_password = var.admin_password != null ? var.admin_password : random_password.postgres[0].result
  zone                   = "1"

  sku_name   = local.sku_name_final
  storage_mb = var.storage_mb

  backup_retention_days        = var.backup_retention_days
  geo_redundant_backup_enabled = var.geo_redundant_backup_enabled

  tags = local.common_tags
}

# Random password for PostgreSQL (if not provided)
resource "random_password" "postgres" {
  count   = var.admin_password == null ? 1 : 0
  length  = 32
  special = true
}

# PostgreSQL Databases
resource "azurerm_postgresql_flexible_server_database" "databases" {
  for_each  = toset(var.databases)
  name      = each.value
  server_id = azurerm_postgresql_flexible_server.shared.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# PostgreSQL Firewall Rules (allow Azure services)
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.shared.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

output "server_id" {
  description = "PostgreSQL server ID"
  value       = azurerm_postgresql_flexible_server.shared.id
}

output "server_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = azurerm_postgresql_flexible_server.shared.fqdn
}

output "admin_login" {
  description = "PostgreSQL administrator login"
  value       = azurerm_postgresql_flexible_server.shared.administrator_login
}

output "admin_password" {
  description = "PostgreSQL administrator password"
  value       = var.admin_password != null ? var.admin_password : random_password.postgres[0].result
  sensitive   = true
}

output "private_dns_zone_id" {
  description = "Private DNS Zone ID"
  value       = azurerm_private_dns_zone.postgres.id
}

output "connection_string_template" {
  description = "PostgreSQL connection string template (use with database name)"
  value       = "Host=${azurerm_postgresql_flexible_server.shared.fqdn};Port=5432;Username=${azurerm_postgresql_flexible_server.shared.administrator_login};Password=${var.admin_password != null ? var.admin_password : random_password.postgres[0].result};Database={0};SSLMode=Require;Trust Server Certificate=true"
  sensitive   = true
}

output "connection_strings" {
  description = "Map of database names to connection strings (Npgsql format for .NET)"
  value = {
    for db in var.databases : db => "Host=${azurerm_postgresql_flexible_server.shared.fqdn};Port=5432;Username=${azurerm_postgresql_flexible_server.shared.administrator_login};Password=${var.admin_password != null ? var.admin_password : random_password.postgres[0].result};Database=${db};SSLMode=Require;Trust Server Certificate=true"
  }
  sensitive = true
}

