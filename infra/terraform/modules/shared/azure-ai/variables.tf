# Azure AI Foundry Module Variables

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
}

variable "region_code" {
  description = "Short region code for resource naming (e.g., 'san' for South Africa North)"
  type        = string
  default     = "san"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "sku_name" {
  description = "SKU for Azure AI Foundry (S0 is standard)"
  type        = string
  default     = "S0"
}

# =============================================================================
# Project Configuration
# =============================================================================

variable "enable_project" {
  description = "Enable AI Foundry project for workload isolation"
  type        = bool
  default     = true
}

# =============================================================================
# Model Deployments
# =============================================================================
# Supports both OpenAI models and Azure AI Model Catalog models (Anthropic, etc.)
# model_format: "OpenAI" for GPT models, "Anthropic" for Claude, etc.

variable "model_deployments" {
  description = "Map of model deployments to create"
  type = map(object({
    model_name      = string
    model_version   = string
    model_format    = optional(string, "OpenAI") # OpenAI, Anthropic, Cohere, Meta, Mistral, etc.
    sku_name        = optional(string, "Standard")
    capacity        = optional(number, 10)
    rai_policy_name = optional(string, null) # Responsible AI policy name
  }))
  default = {
    # ==========================================================================
    # OpenAI Models (GPT Series)
    # ==========================================================================
    "gpt-4o" = {
      model_name    = "gpt-4o"
      model_version = "2024-08-06"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }
    "gpt-4o-mini" = {
      model_name    = "gpt-4o-mini"
      model_version = "2024-07-18"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }
    # GPT-4.1 series (newer models)
    "gpt-4-1" = {
      model_name    = "gpt-4.1"
      model_version = "2024-04-01-preview" # Check Azure for latest version
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }
    # GPT-5 series (if available in your region)
    # Note: These models may have limited regional availability
    "gpt-5-nano" = {
      model_name    = "gpt-5-nano"
      model_version = "2024-12-01-preview" # Check Azure for latest version
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20 # Higher capacity for smaller model
    }
    "gpt-5-1" = {
      model_name    = "gpt-5.1"
      model_version = "2024-12-01-preview" # Check Azure for latest version
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }

    # ==========================================================================
    # Anthropic Models (Claude Series)
    # ==========================================================================
    # Note: Claude models require Azure AI Model Catalog access and may have
    # allocation constraints. Deployment may fail if quota is not available.
    # Check Azure portal for model availability in your region.
    "claude-sonnet-4-5" = {
      model_name    = "claude-sonnet-4-5"
      model_version = "1" # Anthropic versioning
      model_format  = "Anthropic"
      sku_name      = "Standard" # Pay-as-you-go
      capacity      = 1          # Anthropic models typically have lower capacity limits
    }
  }
}

# =============================================================================
# Network Configuration
# =============================================================================

variable "enable_network_rules" {
  description = "Enable network access rules"
  type        = bool
  default     = false
}

variable "network_default_action" {
  description = "Default action for network rules (Allow or Deny)"
  type        = string
  default     = "Allow"
}

variable "allowed_ip_ranges" {
  description = "List of allowed IP ranges"
  type        = list(string)
  default     = []
}

variable "public_network_access_enabled" {
  description = "Enable public network access"
  type        = bool
  default     = true
}

variable "disable_local_auth" {
  description = "Disable API key authentication (use managed identity only)"
  type        = bool
  default     = false
}

# =============================================================================
# Private Endpoint Configuration
# =============================================================================

variable "enable_private_endpoint" {
  description = "Enable private endpoint"
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "Subnet ID for private endpoint"
  type        = string
  default     = ""
}

variable "create_private_dns_zone" {
  description = "Create private DNS zone for private endpoint"
  type        = bool
  default     = true
}

variable "vnet_id" {
  description = "VNet ID for private DNS zone link"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
