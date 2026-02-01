# Mystira Publisher Infrastructure Module - Outputs
# Output definitions for the publisher module
# See main.tf for resource definitions and variables.tf for input variables

output "nsg_id" {
  description = "Network Security Group ID for publisher service"
  value       = azurerm_network_security_group.publisher.id
}

output "identity_id" {
  description = "Managed Identity ID for publisher service"
  value       = azurerm_user_assigned_identity.publisher.id
}

output "identity_principal_id" {
  description = "Managed Identity Principal ID"
  value       = azurerm_user_assigned_identity.publisher.principal_id
}

output "servicebus_namespace" {
  description = "Service Bus namespace for publisher events"
  value       = var.use_shared_servicebus ? null : azurerm_servicebus_namespace.publisher[0].name
}

output "servicebus_queue_name" {
  description = "Service Bus queue name for publisher events"
  value       = var.use_shared_servicebus ? var.shared_servicebus_queue_name : azurerm_servicebus_queue.publisher_events[0].name
}

output "application_insights_id" {
  description = "Application Insights ID for publisher monitoring"
  value       = azurerm_application_insights.publisher.id
}

output "app_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.publisher.connection_string
  sensitive   = true
}

output "key_vault_id" {
  description = "Key Vault ID for publisher secrets"
  value       = azurerm_key_vault.publisher.id
}

output "key_vault_uri" {
  description = "Key Vault URI for publisher secrets"
  value       = azurerm_key_vault.publisher.vault_uri
}

output "identity_client_id" {
  description = "Managed Identity Client ID (for workload identity configuration)"
  value       = azurerm_user_assigned_identity.publisher.client_id
}
