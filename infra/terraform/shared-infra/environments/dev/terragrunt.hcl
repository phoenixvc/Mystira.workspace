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
