# Mystira Chain Infrastructure Module - Outputs
# Output definitions for the chain module
# See main.tf for resource definitions and variables.tf for input variables

output "nsg_id" {
  description = "Network Security Group ID for chain nodes"
  value       = azurerm_network_security_group.chain.id
}

output "identity_id" {
  description = "Managed Identity ID for chain nodes"
  value       = azurerm_user_assigned_identity.chain.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.chain.principal_id
}

output "storage_account_name" {
  description = "Storage account name for chain data"
  value       = azurerm_storage_account.chain.name
}

output "application_insights_id" {
  description = "Application Insights ID for chain monitoring"
  value       = azurerm_application_insights.chain.id
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.chain.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for chain secrets"
  value       = azurerm_key_vault.chain.id
}

output "key_vault_uri" {
  description = "Key Vault URI for chain secrets"
  value       = azurerm_key_vault.chain.vault_uri
}

output "identity_client_id" {
  description = "Managed Identity Client ID (for workload identity configuration)"
  value       = azurerm_user_assigned_identity.chain.client_id
}
