# =============================================================================
# Publisher - Dev Environment
# =============================================================================
# Publishing service for content distribution
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
# Publisher Module
# =============================================================================

module "publisher" {
  source = "../../../../modules/publisher"

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
  description = "Key Vault URI for publisher secrets"
  value       = module.publisher.key_vault_uri
}

output "identity_client_id" {
  description = "Managed Identity Client ID"
  value       = module.publisher.identity_client_id
}

output "servicebus_queue_name" {
  description = "Service Bus queue name"
  value       = module.publisher.servicebus_queue_name
}
