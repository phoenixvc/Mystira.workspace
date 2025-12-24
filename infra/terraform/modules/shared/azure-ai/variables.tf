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
    rai_policy_name = optional(string, null)   # Responsible AI policy name
    location        = optional(string, null)   # Override region for models not available in primary region
  }))
  default = {
    # ==========================================================================
    # OpenAI Models (GPT Series) - Flagship & Latest
    # ==========================================================================

    # GPT-4o - Flagship multimodal model (request quota increase if exceeded)
    "gpt-4o" = {
      model_name    = "gpt-4o"
      model_version = "2024-11-20"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }

    # GPT-4o-mini - Cost-effective, widely available
    "gpt-4o-mini" = {
      model_name    = "gpt-4o-mini"
      model_version = "2024-07-18"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }

    # GPT-4.1 Series - Enhanced reasoning models
    "gpt-4.1" = {
      model_name    = "gpt-4.1"
      model_version = "2025-04-14"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }
    "gpt-4.1-nano" = {
      model_name    = "gpt-4.1-nano"
      model_version = "2025-04-14"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }
    "gpt-4.1-mini" = {
      model_name    = "gpt-4.1-mini"
      model_version = "2025-04-14"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }

    # GPT-5 Series - Next generation models
    "gpt-5-nano" = {
      model_name    = "gpt-5-nano"
      model_version = "2025-05-01"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }
    "gpt-5.1" = {
      model_name    = "gpt-5.1"
      model_version = "2025-06-01"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }
    "gpt-5.1-codex" = {
      model_name    = "gpt-5.1-codex"
      model_version = "2025-06-01"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }

    # ==========================================================================
    # Embedding Models (for RAG / Vector Search)
    # ==========================================================================
    # Used to convert text to vectors before sending to AI Search
    # Reduces token usage by ~20x compared to sending raw text
    # Note: Must use GlobalStandard - Standard not available in SAN
    "text-embedding-3-large" = {
      model_name    = "text-embedding-3-large"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard" # Required for SAN region
      capacity      = 120
    }
    "text-embedding-3-small" = {
      model_name    = "text-embedding-3-small"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard" # Required for SAN region
      capacity      = 120
    }

    # ==========================================================================
    # Anthropic Claude Models (via Azure AI Model Catalog)
    # ==========================================================================
    # Note: Claude models are deployed via Azure AI Model Catalog (Serverless API)
    # They require marketplace subscription and use pay-as-you-go billing
    # Deploy via: az ml serverless-endpoint create or Azure AI Foundry portal
    "claude-haiku-4-5" = {
      model_name    = "claude-3-5-haiku"
      model_version = "20241022"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - capacity is token-based
    }
    "claude-sonnet-4-5" = {
      model_name    = "claude-sonnet-4-5"
      model_version = "20250514"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - capacity is token-based
    }
    "claude-opus-4-5" = {
      model_name    = "claude-opus-4-5"
      model_version = "20250514"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - capacity is token-based
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
