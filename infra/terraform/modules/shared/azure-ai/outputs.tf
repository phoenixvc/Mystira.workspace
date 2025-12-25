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
  description = "Map of deployed catalog models (Anthropic, etc.) - primary region"
  value = {
    for k, v in azapi_resource.catalog_models : k => {
      name     = v.name
      format   = var.model_deployments[k].model_format
      model    = var.model_deployments[k].model_name
      region   = "primary"
      endpoint = azurerm_cognitive_account.ai_foundry.endpoint
    }
  }
}

output "catalog_model_deployments_uksouth" {
  description = "Map of deployed catalog models in UK South region"
  value = {
    for k, v in azapi_resource.catalog_models_uksouth : k => {
      name     = v.name
      format   = var.model_deployments[k].model_format
      model    = var.model_deployments[k].model_name
      region   = "uksouth"
      endpoint = azurerm_cognitive_account.ai_foundry_uksouth[0].endpoint
    }
  }
}

output "openai_model_deployments_eastus" {
  description = "Map of deployed OpenAI models in East US (Standard SKU)"
  value = {
    for k, v in azurerm_cognitive_deployment.openai_models_eastus : k => {
      name     = v.name
      model    = v.model[0].name
      version  = v.model[0].version
      format   = "OpenAI"
      region   = "eastus"
      endpoint = azurerm_cognitive_account.ai_foundry_eastus[0].endpoint
    }
  }
}

output "model_deployments" {
  description = "Combined map of all deployed models across all regions"
  value = merge(
    # OpenAI models (primary region)
    {
      for k, v in azurerm_cognitive_deployment.openai_models : k => {
        name     = v.name
        model    = v.model[0].name
        version  = v.model[0].version
        format   = "OpenAI"
        region   = var.location
        endpoint = azurerm_cognitive_account.ai_foundry.endpoint
      }
    },
    # OpenAI models (East US - DALL-E)
    {
      for k, v in azurerm_cognitive_deployment.openai_models_eastus : k => {
        name     = v.name
        model    = v.model[0].name
        version  = v.model[0].version
        format   = "OpenAI"
        region   = "eastus"
        endpoint = azurerm_cognitive_account.ai_foundry_eastus[0].endpoint
      }
    },
    # OpenAI models (Sweden Central - GPT-5.1)
    {
      for k, v in azurerm_cognitive_deployment.openai_models_swedencentral : k => {
        name     = v.name
        model    = v.model[0].name
        version  = v.model[0].version
        format   = "OpenAI"
        region   = "swedencentral"
        endpoint = azurerm_cognitive_account.ai_foundry_swedencentral[0].endpoint
      }
    },
    # OpenAI models (North Central US - Whisper/TTS)
    {
      for k, v in azurerm_cognitive_deployment.openai_models_northcentralus : k => {
        name     = v.name
        model    = v.model[0].name
        version  = v.model[0].version
        format   = "OpenAI"
        region   = "northcentralus"
        endpoint = azurerm_cognitive_account.ai_foundry_northcentralus[0].endpoint
      }
    },
    # Catalog models (primary region)
    {
      for k, v in azapi_resource.catalog_models : k => {
        name     = v.name
        model    = var.model_deployments[k].model_name
        version  = var.model_deployments[k].model_version
        format   = var.model_deployments[k].model_format
        region   = var.location
        endpoint = azurerm_cognitive_account.ai_foundry.endpoint
      }
    },
    # Catalog models (UK South region)
    {
      for k, v in azapi_resource.catalog_models_uksouth : k => {
        name     = v.name
        model    = var.model_deployments[k].model_name
        version  = var.model_deployments[k].model_version
        format   = var.model_deployments[k].model_format
        region   = "uksouth"
        endpoint = azurerm_cognitive_account.ai_foundry_uksouth[0].endpoint
      }
    }
  )
}

# =============================================================================
# UK South Account (Secondary Region)
# =============================================================================

output "uksouth_account_id" {
  description = "UK South AI Foundry account ID (if created)"
  value       = local.needs_uksouth ? azurerm_cognitive_account.ai_foundry_uksouth[0].id : null
}

output "uksouth_endpoint" {
  description = "UK South AI Foundry endpoint URL (if created)"
  value       = local.needs_uksouth ? azurerm_cognitive_account.ai_foundry_uksouth[0].endpoint : null
}

output "uksouth_primary_access_key" {
  description = "UK South primary access key (if created)"
  value       = local.needs_uksouth ? azurerm_cognitive_account.ai_foundry_uksouth[0].primary_access_key : null
  sensitive   = true
}

# =============================================================================
# Connection Configuration
# =============================================================================

output "connection_config" {
  description = "Connection configuration for applications (primary region)"
  value = {
    endpoint   = azurerm_cognitive_account.ai_foundry.endpoint
    account_id = azurerm_cognitive_account.ai_foundry.id
    region     = var.location
    # API key should be retrieved from Key Vault in production
  }
}

output "connection_config_uksouth" {
  description = "Connection configuration for UK South models"
  value = local.needs_uksouth ? {
    endpoint   = azurerm_cognitive_account.ai_foundry_uksouth[0].endpoint
    account_id = azurerm_cognitive_account.ai_foundry_uksouth[0].id
    region     = "uksouth"
  } : null
}

# =============================================================================
# East US Account (Secondary Region - Standard SKU)
# =============================================================================

output "eastus_account_id" {
  description = "East US AI Foundry account ID (if created)"
  value       = local.needs_eastus ? azurerm_cognitive_account.ai_foundry_eastus[0].id : null
}

output "eastus_endpoint" {
  description = "East US AI Foundry endpoint URL (if created)"
  value       = local.needs_eastus ? azurerm_cognitive_account.ai_foundry_eastus[0].endpoint : null
}

output "eastus_primary_access_key" {
  description = "East US primary access key (if created)"
  value       = local.needs_eastus ? azurerm_cognitive_account.ai_foundry_eastus[0].primary_access_key : null
  sensitive   = true
}

output "connection_config_eastus" {
  description = "Connection configuration for East US models (DALL-E)"
  value = local.needs_eastus ? {
    endpoint   = azurerm_cognitive_account.ai_foundry_eastus[0].endpoint
    account_id = azurerm_cognitive_account.ai_foundry_eastus[0].id
    region     = "eastus"
  } : null
}

# =============================================================================
# Sweden Central Account (Secondary Region - GPT-5.1)
# =============================================================================

output "swedencentral_account_id" {
  description = "Sweden Central AI Foundry account ID (if created)"
  value       = local.needs_swedencentral ? azurerm_cognitive_account.ai_foundry_swedencentral[0].id : null
}

output "swedencentral_endpoint" {
  description = "Sweden Central AI Foundry endpoint URL (if created)"
  value       = local.needs_swedencentral ? azurerm_cognitive_account.ai_foundry_swedencentral[0].endpoint : null
}

output "connection_config_swedencentral" {
  description = "Connection configuration for Sweden Central models (GPT-5.1)"
  value = local.needs_swedencentral ? {
    endpoint   = azurerm_cognitive_account.ai_foundry_swedencentral[0].endpoint
    account_id = azurerm_cognitive_account.ai_foundry_swedencentral[0].id
    region     = "swedencentral"
  } : null
}

# =============================================================================
# North Central US Account (Secondary Region - Audio Models)
# =============================================================================

output "northcentralus_account_id" {
  description = "North Central US AI Foundry account ID (if created)"
  value       = local.needs_northcentralus ? azurerm_cognitive_account.ai_foundry_northcentralus[0].id : null
}

output "northcentralus_endpoint" {
  description = "North Central US AI Foundry endpoint URL (if created)"
  value       = local.needs_northcentralus ? azurerm_cognitive_account.ai_foundry_northcentralus[0].endpoint : null
}

output "connection_config_northcentralus" {
  description = "Connection configuration for North Central US models (Whisper, TTS)"
  value = local.needs_northcentralus ? {
    endpoint   = azurerm_cognitive_account.ai_foundry_northcentralus[0].endpoint
    account_id = azurerm_cognitive_account.ai_foundry_northcentralus[0].id
    region     = "northcentralus"
  } : null
}

# =============================================================================
# Deployment Health
# =============================================================================

output "deployment_health" {
  description = "Deployment health summary for monitoring"
  value = {
    primary_region   = var.location
    primary_endpoint = azurerm_cognitive_account.ai_foundry.endpoint
    openai_model_count_primary        = length(azurerm_cognitive_deployment.openai_models)
    openai_model_count_eastus         = length(azurerm_cognitive_deployment.openai_models_eastus)
    openai_model_count_swedencentral  = length(azurerm_cognitive_deployment.openai_models_swedencentral)
    openai_model_count_northcentralus = length(azurerm_cognitive_deployment.openai_models_northcentralus)
    catalog_model_count_primary       = length(azapi_resource.catalog_models)
    catalog_model_count_uksouth       = length(azapi_resource.catalog_models_uksouth)
    total_model_count = (
      length(azurerm_cognitive_deployment.openai_models) +
      length(azurerm_cognitive_deployment.openai_models_eastus) +
      length(azurerm_cognitive_deployment.openai_models_swedencentral) +
      length(azurerm_cognitive_deployment.openai_models_northcentralus) +
      length(azapi_resource.catalog_models) +
      length(azapi_resource.catalog_models_uksouth)
    )
    uksouth_enabled        = local.needs_uksouth
    uksouth_endpoint       = local.needs_uksouth ? azurerm_cognitive_account.ai_foundry_uksouth[0].endpoint : null
    eastus_enabled         = local.needs_eastus
    eastus_endpoint        = local.needs_eastus ? azurerm_cognitive_account.ai_foundry_eastus[0].endpoint : null
    swedencentral_enabled  = local.needs_swedencentral
    swedencentral_endpoint = local.needs_swedencentral ? azurerm_cognitive_account.ai_foundry_swedencentral[0].endpoint : null
    northcentralus_enabled  = local.needs_northcentralus
    northcentralus_endpoint = local.needs_northcentralus ? azurerm_cognitive_account.ai_foundry_northcentralus[0].endpoint : null
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
