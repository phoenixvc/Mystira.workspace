# =============================================================================
# Chain - Production Environment
# =============================================================================
# Blockchain integration service
# =============================================================================

variable "environment" {
  type = string
}

variable "location" {
  type = string
}

variable "fallback_location" {
  type    = string
  default = "eastus2"
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# Shared infrastructure inputs (optional)
variable "shared_log_analytics_workspace_id" {
  type    = string
  default = null
}

# =============================================================================
# Chain Module
# =============================================================================

module "chain" {
  source = "../../../../modules/chain"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  shared_log_analytics_workspace_id = var.shared_log_analytics_workspace_id
}

# =============================================================================
# Outputs
# =============================================================================

output "key_vault_uri" {
  description = "Key Vault URI for chain secrets"
  value       = module.chain.key_vault_uri
}

output "identity_client_id" {
  description = "Managed Identity Client ID"
  value       = module.chain.identity_client_id
}
