# =============================================================================
# Shared Infrastructure - Production Environment
# =============================================================================

include "root" {
  path           = find_in_parent_folders()
  merge_strategy = "deep"
}

include "product" {
  path           = find_in_parent_folders("_product.hcl")
  merge_strategy = "deep"
}

terraform {
  source = "${get_terragrunt_dir()}/."
}

# Production-specific inputs
inputs = {
  # PostgreSQL Configuration (General Purpose for prod)
  postgresql_sku_name             = "GP_Standard_D4s_v3"
  postgresql_storage_mb           = 131072
  postgresql_backup_retention     = 35
  postgresql_geo_redundant_backup = true

  # Redis Configuration
  redis_sku      = "Premium"
  redis_family   = "P"
  redis_capacity = 1

  # Cosmos DB Configuration
  cosmos_serverless   = false
  cosmos_throughput   = 1000
  cosmos_multi_region = true

  # Azure AI Configuration
  azure_ai_sku = "S0"

  # Storage Configuration (Read-access geo-redundant)
  storage_sku = "Standard_RAGRS"

  # Service Bus Configuration
  servicebus_sku = "Premium"

  # Container Registry
  acr_sku = "Premium"

  # Monitoring
  log_retention_days = 90
}
