# Mystira Admin-API Module - Outputs

output "nsg_id" {
  description = "Network Security Group ID for admin-api service"
  value       = azurerm_network_security_group.admin_api.id
}

output "identity_id" {
  description = "Managed Identity ID for admin-api service"
  value       = azurerm_user_assigned_identity.admin_api.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.admin_api.principal_id
}

output "identity_client_id" {
  description = "Managed Identity Client ID (for workload identity)"
  value       = azurerm_user_assigned_identity.admin_api.client_id
}

output "application_insights_id" {
  description = "Application Insights ID for admin-api monitoring"
  value       = azurerm_application_insights.admin_api.id
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.admin_api.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for admin-api secrets"
  value       = azurerm_key_vault.admin_api.id
}

output "key_vault_uri" {
  description = "Key Vault URI for admin-api"
  value       = azurerm_key_vault.admin_api.vault_uri
}
