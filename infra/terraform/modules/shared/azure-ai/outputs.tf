# Azure AI Foundry Module Outputs

# =============================================================================
# AI Foundry Account
# =============================================================================

output "cognitive_account_id" {
  description = "Azure AI Foundry cognitive account ID"
  value       = azurerm_cognitive_account.ai_foundry.id
}

output "cognitive_account_name" {
  description = "Azure AI Foundry cognitive account name"
  value       = azurerm_cognitive_account.ai_foundry.name
}

output "endpoint" {
  description = "Azure AI Foundry endpoint URL"
  value       = azurerm_cognitive_account.ai_foundry.endpoint
}

output "primary_access_key" {
  description = "Primary access key for Azure AI Foundry"
  value       = azurerm_cognitive_account.ai_foundry.primary_access_key
  sensitive   = true
}

output "secondary_access_key" {
  description = "Secondary access key for Azure AI Foundry"
  value       = azurerm_cognitive_account.ai_foundry.secondary_access_key
  sensitive   = true
}

output "identity_principal_id" {
  description = "Principal ID of the system-assigned managed identity"
  value       = azurerm_cognitive_account.ai_foundry.identity[0].principal_id
}

output "identity_tenant_id" {
  description = "Tenant ID of the system-assigned managed identity"
  value       = azurerm_cognitive_account.ai_foundry.identity[0].tenant_id
}

# =============================================================================
# AI Foundry Project
# =============================================================================

output "project_id" {
  description = "Azure AI Foundry project ID (if enabled)"
  value       = var.enable_project ? azapi_resource.ai_project[0].id : null
}

output "project_name" {
  description = "Azure AI Foundry project name (if enabled)"
  value       = var.enable_project ? azapi_resource.ai_project[0].name : null
}

output "project_identity_principal_id" {
  description = "Principal ID of the project's managed identity (if enabled)"
  value       = var.enable_project ? azapi_resource.ai_project[0].identity[0].principal_id : null
}

# =============================================================================
# Model Deployments
# =============================================================================

output "openai_model_deployments" {
  description = "Map of deployed OpenAI models and their deployment names"
  value = {
    for k, v in azurerm_cognitive_deployment.openai_models : k => {
      name    = v.name
      model   = v.model[0].name
      version = v.model[0].version
      format  = "OpenAI"
    }
  }
}

output "catalog_model_deployments" {
  description = "Map of deployed catalog models (Anthropic, etc.) and their deployment names"
  value = {
    for k, v in azapi_resource.catalog_models : k => {
      name   = v.name
      format = var.model_deployments[k].model_format
      model  = var.model_deployments[k].model_name
    }
  }
}

output "model_deployments" {
  description = "Combined map of all deployed models"
  value = merge(
    {
      for k, v in azurerm_cognitive_deployment.openai_models : k => {
        name    = v.name
        model   = v.model[0].name
        version = v.model[0].version
        format  = "OpenAI"
      }
    },
    {
      for k, v in azapi_resource.catalog_models : k => {
        name    = v.name
        model   = var.model_deployments[k].model_name
        version = var.model_deployments[k].model_version
        format  = var.model_deployments[k].model_format
      }
    }
  )
}

# =============================================================================
# Connection Configuration
# =============================================================================

output "connection_config" {
  description = "Connection configuration for applications"
  value = {
    endpoint   = azurerm_cognitive_account.ai_foundry.endpoint
    account_id = azurerm_cognitive_account.ai_foundry.id
    # API key should be retrieved from Key Vault in production
  }
}

# =============================================================================
# Private Endpoint
# =============================================================================

output "private_endpoint_id" {
  description = "Private endpoint ID (if enabled)"
  value       = var.enable_private_endpoint ? azurerm_private_endpoint.ai_foundry[0].id : null
}

output "private_endpoint_ip" {
  description = "Private endpoint IP address (if enabled)"
  value       = var.enable_private_endpoint ? azurerm_private_endpoint.ai_foundry[0].private_service_connection[0].private_ip_address : null
}
