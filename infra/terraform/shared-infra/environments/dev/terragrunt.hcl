# =============================================================================
# Shared Infrastructure - Dev Environment
# =============================================================================

include "root" {
  path           = find_in_parent_folders()
  merge_strategy = "deep"
}

include "product" {
  path           = find_in_parent_folders("_product.hcl")
  merge_strategy = "deep"
}

# Use // so Terragrunt caches the full infra/terraform tree,
# ensuring relative module paths (../../../modules/) resolve in .terragrunt-cache
terraform {
  source = "${get_repo_root()}/infra/terraform//${path_relative_to_include("root")}"
}

# Dev-specific inputs
inputs = {
  # PostgreSQL Configuration (Burstable for dev)
  postgresql_sku_name         = "B_Standard_B1ms"
  postgresql_storage_mb       = 32768
  postgresql_backup_retention = 7

  # Redis Configuration
  redis_sku      = "Basic"
  redis_family   = "C"
  redis_capacity = 0

  # Cosmos DB Configuration (Serverless for dev)
  cosmos_serverless = true

  # Azure AI Configuration
  azure_ai_sku = "S0"

  # Storage Configuration (Locally redundant for dev)
  storage_sku = "Standard_LRS"

  # Service Bus Configuration
  servicebus_sku = "Standard"

  # Container Registry
  acr_sku = "Standard"

  # Monitoring
  log_retention_days = 30
}
