# Azure AI Foundry Module
# Provides shared Azure AI resources for all services
#
# This module creates:
# - Azure AI Foundry account (AIServices - current approach)
# - AI Project for workload isolation
# - Model deployments (OpenAI, Anthropic, and other catalog models)
# - Private endpoint (optional)
#
# Note: Using kind = "AIServices" (Azure AI Foundry) instead of deprecated "OpenAI"
# See: https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/cognitive_account

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "~> 0.13"
    }
  }
}

# Local variables
locals {
  # Naming: mys-shared-ai-{region_code} (shared resource pattern)
  name = "mys-shared-ai-${var.region_code}"

  common_tags = merge(var.tags, {
    Component   = "azure-ai-foundry"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })

  # Separate OpenAI models from catalog models (Anthropic, etc.)
  # Filter out disabled models using coalesce to handle missing enabled field
  # Split OpenAI models: primary (no location override) vs secondary (with location override)
  openai_deployments_primary = {
    for k, v in var.model_deployments : k => v
    if v.model_format == "OpenAI" && coalesce(v.enabled, true) && v.location == null
  }

  openai_deployments_secondary = {
    for k, v in var.model_deployments : k => v
    if v.model_format == "OpenAI" && coalesce(v.enabled, true) && v.location != null
  }

  # Catalog models require different deployment approach via AzAPI
  # Split by region: primary (same as account) vs secondary (different region like uksouth)
  catalog_deployments_primary = {
    for k, v in var.model_deployments : k => v
    if v.model_format != "OpenAI" && (v.location == null || v.location == var.location) && coalesce(v.enabled, true)
  }

  catalog_deployments_secondary = {
    for k, v in var.model_deployments : k => v
    if v.model_format != "OpenAI" && v.location != null && v.location != var.location && coalesce(v.enabled, true)
  }

  # Get unique secondary regions needed for catalog models
  secondary_regions = distinct([
    for k, v in local.catalog_deployments_secondary : v.location
  ])

  # Get unique secondary regions needed for OpenAI models (Standard SKU)
  openai_secondary_regions = distinct([
    for k, v in local.openai_deployments_secondary : v.location
  ])

  # Check if we need UK South account (for catalog models)
  needs_uksouth = contains(local.secondary_regions, "uksouth")

  # Check if we need East US account (for Standard SKU OpenAI models like DALL-E)
  needs_eastus = contains(local.openai_secondary_regions, "eastus")

  # Check if we need Sweden Central account (for GPT-5.1 models)
  needs_swedencentral = contains(local.openai_secondary_regions, "swedencentral")

  # Check if we need North Central US account (for Whisper/TTS models)
  needs_northcentralus = contains(local.openai_secondary_regions, "northcentralus")
}

# =============================================================================
# Azure AI Foundry (AIServices Account)
# =============================================================================
# Using kind = "AIServices" instead of deprecated "OpenAI"
# This provides access to:
# - OpenAI models (GPT-4, GPT-4o, etc.)
# - Azure AI model catalog (Anthropic Claude, Cohere, Mistral, Meta, DeepSeek, etc.)
# - Project management for workload isolation
# - Multi-region model deployment capability

resource "azurerm_cognitive_account" "ai_foundry" {
  name                  = local.name
  location              = var.location
  resource_group_name   = var.resource_group_name
  kind                  = "AIServices" # Azure AI Foundry (not legacy "OpenAI")
  sku_name              = var.sku_name
  custom_subdomain_name = local.name

  # Enable project management for hierarchical organization
  # This allows creating AI projects as child resources
  # Note: Requires azurerm provider >= 4.x
  # If not available, use azapi_resource below instead

  # Network rules
  dynamic "network_acls" {
    for_each = var.enable_network_rules ? [1] : []
    content {
      default_action = var.network_default_action
      ip_rules       = var.allowed_ip_ranges
    }
  }

  # Identity for managed identity access
  identity {
    type = "SystemAssigned"
  }

  local_auth_enabled                 = !var.disable_local_auth
  public_network_access_enabled      = var.public_network_access_enabled
  outbound_network_access_restricted = false

  tags = local.common_tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags that might be added by Azure
      tags["hidden-link:*"],
    ]
  }
}

# =============================================================================
# Enable Project Management (using AzAPI)
# =============================================================================
# The allowProjectManagement property is not available in azurerm provider
# We use azapi_update_resource to PATCH the account with this setting

resource "azapi_update_resource" "enable_project_management" {
  count = var.enable_project ? 1 : 0

  type        = "Microsoft.CognitiveServices/accounts@2025-06-01"
  resource_id = azurerm_cognitive_account.ai_foundry.id

  # Must wait for OpenAI model deployments to complete first
  # Otherwise, PATCH will fail with RequestConflict error because
  # the Cognitive Services account is in a non-terminal provisioning state
  depends_on = [azurerm_cognitive_deployment.openai_models]

  body = {
    properties = {
      allowProjectManagement = true
    }
  }
}

# =============================================================================
# AI Foundry Project (using AzAPI for preview feature support)
# =============================================================================
# Projects provide workload isolation within AI Foundry
# Each project can have its own RBAC, deployments, and configurations

resource "azapi_resource" "ai_project" {
  count = var.enable_project ? 1 : 0

  type      = "Microsoft.CognitiveServices/accounts/projects@2025-06-01"
  name      = "${local.name}-project"
  parent_id = azurerm_cognitive_account.ai_foundry.id
  location  = var.location

  # Must wait for allowProjectManagement to be enabled
  depends_on = [azapi_update_resource.enable_project_management]

  identity {
    type = "SystemAssigned"
  }

  body = {
    properties = {
      displayName = "${var.environment} AI Project"
      description = "Mystira ${var.environment} AI workloads"
    }
  }

  tags = local.common_tags
}

# =============================================================================
# OpenAI Model Deployments
# =============================================================================
# Standard OpenAI model deployments using azurerm provider

resource "azurerm_cognitive_deployment" "openai_models" {
  for_each = local.openai_deployments_primary

  name                 = each.key
  cognitive_account_id = azurerm_cognitive_account.ai_foundry.id

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }

  # Responsible AI policy (optional)
  rai_policy_name = each.value.rai_policy_name

  # Longer timeouts for model deployments which can take time
  timeouts {
    create = "30m"
    update = "30m"
    delete = "15m"
  }

  lifecycle {
    # Prevent recreation when capacity changes (use update instead)
    # Model versions may be updated automatically by Azure
    ignore_changes = [
      model[0].version
    ]
    # Allow replacement if needed, don't block on failures
    create_before_destroy = false
  }
}

# =============================================================================
# Azure AI Model Catalog Deployments (Primary Region)
# =============================================================================
# Catalog models in the same region as the primary AI Foundry account
# These are models like Anthropic Claude that are available in the primary region

resource "azapi_resource" "catalog_models" {
  for_each = local.catalog_deployments_primary

  type      = "Microsoft.CognitiveServices/accounts/deployments@2024-10-01"
  name      = each.key
  parent_id = azurerm_cognitive_account.ai_foundry.id

  # Wait for project management to be enabled
  depends_on = [azapi_update_resource.enable_project_management]

  body = {
    properties = {
      model = {
        format  = each.value.model_format
        name    = each.value.model_name
        version = each.value.model_version
      }
      raiPolicyName = each.value.rai_policy_name
    }
    sku = {
      name     = each.value.sku_name
      capacity = each.value.capacity
    }
  }

  # Catalog models may take longer to provision
  timeouts {
    create = "30m"
    update = "30m"
    delete = "15m"
  }

  lifecycle {
    ignore_changes = [
      # Model versions may be updated automatically by Azure
      body["properties"]["model"]["version"]
    ]
  }
}

# =============================================================================
# Secondary Region AI Foundry Account (UK South)
# =============================================================================
# Required for catalog models not available in primary region (SAN)
# Models like Cohere, Mistral, DeepSeek, AI21 are only available in select regions

resource "azurerm_cognitive_account" "ai_foundry_uksouth" {
  count = local.needs_uksouth ? 1 : 0

  name                  = "mys-shared-ai-uks"
  location              = "uksouth"
  resource_group_name   = var.resource_group_name
  kind                  = "AIServices"
  sku_name              = var.sku_name
  custom_subdomain_name = "mys-shared-ai-uks"

  # Network rules (inherit from primary)
  dynamic "network_acls" {
    for_each = var.enable_network_rules ? [1] : []
    content {
      default_action = var.network_default_action
      ip_rules       = var.allowed_ip_ranges
    }
  }

  identity {
    type = "SystemAssigned"
  }

  local_auth_enabled                 = !var.disable_local_auth
  public_network_access_enabled      = var.public_network_access_enabled
  outbound_network_access_restricted = false

  tags = merge(local.common_tags, {
    Region = "uksouth"
    Role   = "secondary-catalog-models"
  })

  lifecycle {
    ignore_changes = [
      tags["hidden-link:*"],
    ]
  }
}

# =============================================================================
# Azure AI Model Catalog Deployments (UK South Region)
# =============================================================================
# Catalog models only available in UK South (Cohere, Mistral, DeepSeek, AI21)

# Deploy catalog models one at a time using time_sleep to avoid 409 conflicts
# The cognitive account can only handle one deployment operation at a time

resource "time_sleep" "wait_for_uksouth_account" {
  count = local.needs_uksouth ? 1 : 0

  depends_on      = [azurerm_cognitive_account.ai_foundry_uksouth]
  create_duration = "30s"
}

resource "azapi_resource" "catalog_models_uksouth" {
  for_each = local.catalog_deployments_secondary

  type      = "Microsoft.CognitiveServices/accounts/deployments@2024-10-01"
  name      = each.key
  parent_id = azurerm_cognitive_account.ai_foundry_uksouth[0].id

  # Wait for account to be fully ready
  depends_on = [time_sleep.wait_for_uksouth_account]

  body = {
    properties = {
      model = {
        format  = each.value.model_format
        name    = each.value.model_name
        version = each.value.model_version
      }
      raiPolicyName = each.value.rai_policy_name
    }
    sku = {
      name     = each.value.sku_name
      capacity = each.value.capacity
    }
  }

  timeouts {
    create = "30m"
    update = "30m"
    delete = "15m"
  }

  lifecycle {
    ignore_changes = [
      body["properties"]["model"]["version"]
    ]
    # Continue creating other models even if one fails
    create_before_destroy = false
  }
}

# =============================================================================
# Secondary Region AI Foundry Account (East US)
# =============================================================================
# Required for OpenAI models that need Standard SKU (DALL-E, Whisper, TTS)
# Standard SKU is not available in South Africa North

resource "azurerm_cognitive_account" "ai_foundry_eastus" {
  count = local.needs_eastus ? 1 : 0

  name                  = "mys-shared-ai-eus"
  location              = "eastus"
  resource_group_name   = var.resource_group_name
  kind                  = "AIServices"
  sku_name              = var.sku_name
  custom_subdomain_name = "mys-shared-ai-eus"

  # Network rules (inherit from primary)
  dynamic "network_acls" {
    for_each = var.enable_network_rules ? [1] : []
    content {
      default_action = var.network_default_action
      ip_rules       = var.allowed_ip_ranges
    }
  }

  identity {
    type = "SystemAssigned"
  }

  local_auth_enabled                 = !var.disable_local_auth
  public_network_access_enabled      = var.public_network_access_enabled
  outbound_network_access_restricted = false

  tags = merge(local.common_tags, {
    Region = "eastus"
    Role   = "secondary-standard-sku-models"
  })

  lifecycle {
    ignore_changes = [
      tags["hidden-link:*"],
    ]
  }
}

# =============================================================================
# OpenAI Model Deployments (East US Region)
# =============================================================================
# OpenAI models that require Standard SKU (DALL-E, Whisper, TTS)

resource "azurerm_cognitive_deployment" "openai_models_eastus" {
  for_each = {
    for k, v in local.openai_deployments_secondary : k => v
    if v.location == "eastus"
  }

  name                 = each.key
  cognitive_account_id = azurerm_cognitive_account.ai_foundry_eastus[0].id

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }

  rai_policy_name = each.value.rai_policy_name

  timeouts {
    create = "30m"
    update = "30m"
    delete = "15m"
  }

  lifecycle {
    ignore_changes = [
      model[0].version
    ]
    create_before_destroy = false
  }
}

# =============================================================================
# Secondary Region AI Foundry Account (Sweden Central)
# =============================================================================
# Required for GPT-5.1 models that need GlobalStandard in specific regions

resource "azurerm_cognitive_account" "ai_foundry_swedencentral" {
  count = local.needs_swedencentral ? 1 : 0

  name                  = "mys-shared-ai-swc"
  location              = "swedencentral"
  resource_group_name   = var.resource_group_name
  kind                  = "AIServices"
  sku_name              = var.sku_name
  custom_subdomain_name = "mys-shared-ai-swc"

  dynamic "network_acls" {
    for_each = var.enable_network_rules ? [1] : []
    content {
      default_action = var.network_default_action
      ip_rules       = var.allowed_ip_ranges
    }
  }

  identity {
    type = "SystemAssigned"
  }

  local_auth_enabled                 = !var.disable_local_auth
  public_network_access_enabled      = var.public_network_access_enabled
  outbound_network_access_restricted = false

  tags = merge(local.common_tags, {
    Region = "swedencentral"
    Role   = "secondary-gpt5-models"
  })

  lifecycle {
    ignore_changes = [
      tags["hidden-link:*"],
    ]
  }
}

# =============================================================================
# OpenAI Model Deployments (Sweden Central Region)
# =============================================================================
# GPT-5.1 models requiring GlobalStandard in Sweden Central

resource "azurerm_cognitive_deployment" "openai_models_swedencentral" {
  for_each = {
    for k, v in local.openai_deployments_secondary : k => v
    if v.location == "swedencentral"
  }

  name                 = each.key
  cognitive_account_id = azurerm_cognitive_account.ai_foundry_swedencentral[0].id

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }

  rai_policy_name = each.value.rai_policy_name

  timeouts {
    create = "30m"
    update = "30m"
    delete = "15m"
  }

  lifecycle {
    ignore_changes = [
      model[0].version
    ]
    create_before_destroy = false
  }
}

# =============================================================================
# Secondary Region AI Foundry Account (North Central US)
# =============================================================================
# Required for Whisper and TTS models

resource "azurerm_cognitive_account" "ai_foundry_northcentralus" {
  count = local.needs_northcentralus ? 1 : 0

  name                  = "mys-shared-ai-ncus"
  location              = "northcentralus"
  resource_group_name   = var.resource_group_name
  kind                  = "AIServices"
  sku_name              = var.sku_name
  custom_subdomain_name = "mys-shared-ai-ncus"

  dynamic "network_acls" {
    for_each = var.enable_network_rules ? [1] : []
    content {
      default_action = var.network_default_action
      ip_rules       = var.allowed_ip_ranges
    }
  }

  identity {
    type = "SystemAssigned"
  }

  local_auth_enabled                 = !var.disable_local_auth
  public_network_access_enabled      = var.public_network_access_enabled
  outbound_network_access_restricted = false

  tags = merge(local.common_tags, {
    Region = "northcentralus"
    Role   = "secondary-audio-models"
  })

  lifecycle {
    ignore_changes = [
      tags["hidden-link:*"],
    ]
  }
}

# =============================================================================
# OpenAI Model Deployments (North Central US Region)
# =============================================================================
# Whisper and TTS models

resource "azurerm_cognitive_deployment" "openai_models_northcentralus" {
  for_each = {
    for k, v in local.openai_deployments_secondary : k => v
    if v.location == "northcentralus"
  }

  name                 = each.key
  cognitive_account_id = azurerm_cognitive_account.ai_foundry_northcentralus[0].id

  model {
    format  = "OpenAI"
    name    = each.value.model_name
    version = each.value.model_version
  }

  sku {
    name     = each.value.sku_name
    capacity = each.value.capacity
  }

  rai_policy_name = each.value.rai_policy_name

  timeouts {
    create = "30m"
    update = "30m"
    delete = "15m"
  }

  lifecycle {
    ignore_changes = [
      model[0].version
    ]
    create_before_destroy = false
  }
}

# =============================================================================
# Private Endpoint (Optional)
# =============================================================================

resource "azurerm_private_endpoint" "ai_foundry" {
  count = var.enable_private_endpoint ? 1 : 0

  name                = "${local.name}-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${local.name}-psc"
    private_connection_resource_id = azurerm_cognitive_account.ai_foundry.id
    is_manual_connection           = false
    subresource_names              = ["account"]
  }

  tags = local.common_tags
}

# =============================================================================
# Private DNS Zone (Optional)
# =============================================================================
# Note: AIServices uses cognitiveservices DNS zone

resource "azurerm_private_dns_zone" "ai_foundry" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                = "privatelink.cognitiveservices.azure.com"
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "ai_foundry" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                  = "${local.name}-dns-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.ai_foundry[0].name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false

  tags = local.common_tags
}

resource "azurerm_private_dns_a_record" "ai_foundry" {
  count = var.enable_private_endpoint && var.create_private_dns_zone ? 1 : 0

  name                = azurerm_cognitive_account.ai_foundry.name
  zone_name           = azurerm_private_dns_zone.ai_foundry[0].name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [azurerm_private_endpoint.ai_foundry[0].private_service_connection[0].private_ip_address]
}
