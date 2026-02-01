# =============================================================================
# Shared Infrastructure - Production Environment
# =============================================================================
# This file instantiates all shared modules for the production environment.
# Terragrunt generates the backend and provider configuration.
# =============================================================================

variable "environment" {
  type = string
}

variable "location" {
  type = string
}

variable "fallback_location" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# PostgreSQL variables
variable "postgresql_sku_name" {
  type = string
}

variable "postgresql_storage_mb" {
  type = number
}

variable "postgresql_backup_retention" {
  type = number
}

variable "postgresql_geo_redundant_backup" {
  type    = bool
  default = false
}

# Redis variables
variable "redis_sku" {
  type = string
}

variable "redis_family" {
  type = string
}

variable "redis_capacity" {
  type = number
}

# Cosmos DB variables
variable "cosmos_serverless" {
  type = bool
}

variable "cosmos_throughput" {
  type    = number
  default = 400
}

variable "cosmos_multi_region" {
  type    = bool
  default = false
}

# Azure AI variables
variable "azure_ai_sku" {
  type = string
}

# Storage variables
variable "storage_sku" {
  type = string
}

# Service Bus variables
variable "servicebus_sku" {
  type = string
}

# Container Registry variables
variable "acr_sku" {
  type = string
}

# Monitoring variables
variable "log_retention_days" {
  type = number
}

# =============================================================================
# Data Sources
# =============================================================================

# Used for location validation and resource group attributes
data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

# =============================================================================
# Validation
# =============================================================================

check "resource_group_location_matches" {
  assert {
    condition     = data.azurerm_resource_group.main.location == var.location
    error_message = "Resource group location (${data.azurerm_resource_group.main.location}) does not match var.location (${var.location})."
  }
}

# =============================================================================
# Shared Modules
# =============================================================================

module "monitoring" {
  source = "../../../modules/shared/monitoring"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  log_retention_days  = var.log_retention_days
  tags                = var.tags
}

module "postgresql" {
  source = "../../../modules/shared/postgresql"

  environment                  = var.environment
  location                     = var.location
  resource_group_name          = var.resource_group_name
  sku_name                     = var.postgresql_sku_name
  storage_mb                   = var.postgresql_storage_mb
  backup_retention_days        = var.postgresql_backup_retention
  geo_redundant_backup_enabled = var.postgresql_geo_redundant_backup
  tags                         = var.tags
}

module "redis" {
  source = "../../../modules/shared/redis"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  sku_name            = var.redis_sku
  family              = var.redis_family
  capacity            = var.redis_capacity
  tags                = var.tags
}

module "cosmos_db" {
  source = "../../../modules/shared/cosmos-db"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  serverless          = var.cosmos_serverless
  throughput          = var.cosmos_throughput
  multi_region        = var.cosmos_multi_region
  fallback_location   = var.fallback_location
  tags                = var.tags
}

module "storage" {
  source = "../../../modules/shared/storage"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  storage_sku         = var.storage_sku
  tags                = var.tags
}

module "azure_ai" {
  source = "../../../modules/shared/azure-ai"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  sku_name            = var.azure_ai_sku
  tags                = var.tags
}

module "servicebus" {
  source = "../../../modules/shared/servicebus"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.servicebus_sku
  tags                = var.tags
}

module "container_registry" {
  source = "../../../modules/shared/container-registry"

  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = var.acr_sku
  tags                = var.tags
}

module "dns" {
  source = "../../../modules/dns"

  environment         = var.environment
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# =============================================================================
# Outputs - Exposed for other products to consume
# =============================================================================

output "postgresql_server_id" {
  value = module.postgresql.server_id
}

output "postgresql_server_fqdn" {
  value = module.postgresql.server_fqdn
}

output "redis_cache_id" {
  value = module.redis.cache_id
}

output "redis_connection_string" {
  value     = module.redis.primary_connection_string
  sensitive = true
}

output "cosmos_db_account_id" {
  value = module.cosmos_db.account_id
}

output "cosmos_db_connection_string" {
  value     = module.cosmos_db.primary_connection_string
  sensitive = true
}

output "storage_account_id" {
  value = module.storage.account_id
}

output "storage_connection_string" {
  value     = module.storage.primary_connection_string
  sensitive = true
}

output "azure_ai_endpoint" {
  value = module.azure_ai.endpoint
}

output "servicebus_namespace_id" {
  value = module.servicebus.namespace_id
}

output "servicebus_connection_string" {
  value     = module.servicebus.default_primary_connection_string
  sensitive = true
}

output "acr_login_server" {
  value = module.container_registry.acr_login_server
}

output "dns_zone_id" {
  value = module.dns.dns_zone_id
}

output "log_analytics_workspace_id" {
  value = module.monitoring.log_analytics_workspace_id
}

output "application_insights_connection_string" {
  value     = module.monitoring.application_insights_connection_string
  sensitive = true
}
