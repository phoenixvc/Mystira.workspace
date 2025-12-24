# Azure AI Search Module Outputs

output "id" {
  description = "Azure AI Search service ID"
  value       = azurerm_search_service.main.id
}

output "name" {
  description = "Azure AI Search service name"
  value       = azurerm_search_service.main.name
}

output "endpoint" {
  description = "Azure AI Search endpoint URL"
  value       = "https://${azurerm_search_service.main.name}.search.windows.net"
}

output "primary_key" {
  description = "Primary admin key for Azure AI Search"
  value       = azurerm_search_service.main.primary_key
  sensitive   = true
}

output "secondary_key" {
  description = "Secondary admin key for Azure AI Search"
  value       = azurerm_search_service.main.secondary_key
  sensitive   = true
}

output "query_keys" {
  description = "Query keys for Azure AI Search (read-only access)"
  value       = azurerm_search_service.main.query_keys
  sensitive   = true
}

output "identity_principal_id" {
  description = "Principal ID of the system-assigned managed identity"
  value       = azurerm_search_service.main.identity[0].principal_id
}

output "identity_tenant_id" {
  description = "Tenant ID of the system-assigned managed identity"
  value       = azurerm_search_service.main.identity[0].tenant_id
}

# Connection configuration for applications
output "connection_config" {
  description = "Connection configuration for applications"
  value = {
    endpoint     = "https://${azurerm_search_service.main.name}.search.windows.net"
    service_name = azurerm_search_service.main.name
  }
}

# Private endpoint outputs
output "private_endpoint_id" {
  description = "Private endpoint ID (if enabled)"
  value       = var.enable_private_endpoint ? azurerm_private_endpoint.search[0].id : null
}

output "private_endpoint_ip" {
  description = "Private endpoint IP address (if enabled)"
  value       = var.enable_private_endpoint ? azurerm_private_endpoint.search[0].private_service_connection[0].private_ip_address : null
}
