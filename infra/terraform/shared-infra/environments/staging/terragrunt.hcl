# =============================================================================
# Shared Infrastructure - Staging Environment
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

# Staging-specific inputs
inputs = {
  # PostgreSQL Configuration (General Purpose for staging)
  postgresql_sku_name         = "GP_Standard_D2s_v3"
  postgresql_storage_mb       = 65536
  postgresql_backup_retention = 14

  # Redis Configuration
  redis_sku      = "Standard"
  redis_family   = "C"
  redis_capacity = 1

  # Cosmos DB Configuration
  cosmos_serverless = false
  cosmos_throughput = 400

  # Azure AI Configuration
  azure_ai_sku = "S0"

  # Storage Configuration (Geo-redundant for staging)
  storage_sku = "Standard_GRS"

  # Service Bus Configuration
  servicebus_sku = "Standard"

  # Container Registry
  acr_sku = "Standard"

  # Monitoring
  log_retention_days = 60
}
