# =============================================================================
# Shared Infrastructure - Staging Environment
# =============================================================================
# This file instantiates all shared modules for the staging environment.
# Terragrunt generates the backend and provider configuration.
# =============================================================================

variable "environment" {
  type = string
}

variable "location" {
  type = string
}

# Note: fallback_location not used in staging (no geo-replication)
# Kept for terragrunt input consistency with prod

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

# Note: cosmos_throughput not used when serverless = true
# Kept for terragrunt input consistency with prod

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
# Shared Modules
# =============================================================================
# Note: data.azurerm_resource_group not needed in staging
# (prod uses it for location validation)

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

  environment           = var.environment
  location              = var.location
  resource_group_name   = var.resource_group_name
  sku_name              = var.postgresql_sku_name
  storage_mb            = var.postgresql_storage_mb
  backup_retention_days = var.postgresql_backup_retention
  tags                  = var.tags
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
  value     = module.cosmos_db.primary_sql_connection_string
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
