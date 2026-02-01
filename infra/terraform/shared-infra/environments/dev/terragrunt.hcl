# =============================================================================
# Shared Infrastructure - Dev Environment
# =============================================================================

include "root" {
  path = find_in_parent_folders()
}

include "product" {
  path = find_in_parent_folders("terragrunt.hcl", "${get_terragrunt_dir()}/../../terragrunt.hcl")
}

# Terraform source - use the modules directly
terraform {
  source = "${get_terragrunt_dir()}/."
}

# Dev-specific inputs
inputs = {
  # PostgreSQL Configuration
  postgresql_sku_name = "B_Standard_B1ms" # Burstable for dev
  postgresql_storage_mb = 32768
  postgresql_backup_retention = 7

  # Redis Configuration
  redis_sku = "Basic"
  redis_family = "C"
  redis_capacity = 0

  # Cosmos DB Configuration
  cosmos_serverless = true # Serverless for dev

  # Azure AI Configuration
  azure_ai_sku = "S0"

  # Storage Configuration
  storage_sku = "Standard_LRS" # Locally redundant for dev

  # Service Bus Configuration
  servicebus_sku = "Standard"

  # Container Registry
  acr_sku = "Standard"

  # Monitoring
  log_retention_days = 30
}
