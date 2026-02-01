# =============================================================================
# Shared Infrastructure - Staging Environment
# =============================================================================

include "root" {
  path = find_in_parent_folders()
}

include "product" {
  path = find_in_parent_folders("terragrunt.hcl", "${get_terragrunt_dir()}/../../terragrunt.hcl")
}

terraform {
  source = "${get_terragrunt_dir()}/."
}

# Staging-specific inputs
inputs = {
  # PostgreSQL Configuration
  postgresql_sku_name = "GP_Standard_D2s_v3" # General Purpose for staging
  postgresql_storage_mb = 65536
  postgresql_backup_retention = 14

  # Redis Configuration
  redis_sku = "Standard"
  redis_family = "C"
  redis_capacity = 1

  # Cosmos DB Configuration
  cosmos_serverless = false
  cosmos_throughput = 400

  # Azure AI Configuration
  azure_ai_sku = "S0"

  # Storage Configuration
  storage_sku = "Standard_GRS" # Geo-redundant for staging

  # Service Bus Configuration
  servicebus_sku = "Standard"

  # Container Registry
  acr_sku = "Standard"

  # Monitoring
  log_retention_days = 60
}
