# =============================================================================
# Chain - Dev Environment
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
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# Shared infrastructure inputs
variable "shared_cosmos_db_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_servicebus_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_log_analytics_workspace_id" {
  type = string
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
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

  cosmos_db_connection_string            = var.shared_cosmos_db_connection_string
  servicebus_connection_string           = var.shared_servicebus_connection_string
  log_analytics_workspace_id             = var.shared_log_analytics_workspace_id
  application_insights_connection_string = var.shared_application_insights_connection_string
}

# =============================================================================
# Outputs
# =============================================================================

output "chain_api_url" {
  value = module.chain.api_url
}
