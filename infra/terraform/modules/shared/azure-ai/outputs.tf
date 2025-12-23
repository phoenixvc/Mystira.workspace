# Azure AI Foundry Module Outputs

output "cognitive_account_id" {
  description = "Azure OpenAI cognitive account ID"
  value       = azurerm_cognitive_account.openai.id
}

output "cognitive_account_name" {
  description = "Azure OpenAI cognitive account name"
  value       = azurerm_cognitive_account.openai.name
}

output "endpoint" {
  description = "Azure OpenAI endpoint URL"
  value       = azurerm_cognitive_account.openai.endpoint
}

output "primary_access_key" {
  description = "Primary access key for Azure OpenAI"
  value       = azurerm_cognitive_account.openai.primary_access_key
  sensitive   = true
}

output "secondary_access_key" {
  description = "Secondary access key for Azure OpenAI"
  value       = azurerm_cognitive_account.openai.secondary_access_key
  sensitive   = true
}

output "identity_principal_id" {
  description = "Principal ID of the system-assigned managed identity"
  value       = azurerm_cognitive_account.openai.identity[0].principal_id
}

output "identity_tenant_id" {
  description = "Tenant ID of the system-assigned managed identity"
  value       = azurerm_cognitive_account.openai.identity[0].tenant_id
}

# Model deployment endpoints
output "model_deployments" {
  description = "Map of deployed models and their deployment names"
  value = {
    for k, v in azurerm_cognitive_deployment.models : k => {
      name    = v.name
      model   = v.model[0].name
      version = v.model[0].version
    }
  }
}

# Connection string format for applications
output "connection_config" {
  description = "Connection configuration for applications"
  value = {
    endpoint = azurerm_cognitive_account.openai.endpoint
    # API key should be retrieved from Key Vault in production
  }
}
