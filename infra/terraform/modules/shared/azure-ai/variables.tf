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
#
# Supported model formats:
#   - OpenAI: GPT, DALL-E, Whisper, TTS, embedding models
#   - Anthropic: Claude models (haiku, sonnet, opus)
#   - Cohere: Rerank, Embed, Command models
#   - Meta: Llama models
#   - Mistral: Mistral, Codestral, Pixtral models
#   - DeepSeek: DeepSeek-V3, DeepSeek-Coder models
#   - AI21: Jamba models

variable "model_deployments" {
  description = "Map of model deployments to create"
  type = map(object({
    model_name      = string
    model_version   = string
    model_format    = optional(string, "OpenAI") # OpenAI, Anthropic, Cohere, Meta, Mistral, DeepSeek, AI21
    sku_name        = optional(string, "Standard")
    capacity        = optional(number, 10)
    rai_policy_name = optional(string, null)   # Responsible AI policy name
    location        = optional(string, null)   # Override region for models not available in primary region
    enabled         = optional(bool, true)     # Set to false to skip deploying this model
  }))

  validation {
    condition = alltrue([
      for k, v in var.model_deployments :
      contains(["OpenAI", "Anthropic", "Cohere", "Meta", "Mistral", "DeepSeek", "AI21"], v.model_format)
    ])
    error_message = "model_format must be one of: OpenAI, Anthropic, Cohere, Meta, Mistral, DeepSeek, AI21"
  }

  validation {
    condition = alltrue([
      for k, v in var.model_deployments :
      contains(["Standard", "GlobalStandard", "ProvisionedManaged"], v.sku_name)
    ])
    error_message = "sku_name must be one of: Standard, GlobalStandard, ProvisionedManaged"
  }
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

    # ==========================================================================
    # GPT-4.1 Series - Enhanced reasoning models
    # ==========================================================================
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

    # ==========================================================================
    # GPT-5 Series - Latest flagship models
    # ==========================================================================
    # Note: gpt-5.1 and gpt-5.1-codex require registration for access
    # Available via GlobalStandard in East US2/Sweden Central
    "gpt-5-nano" = {
      model_name    = "gpt-5-nano"
      model_version = "2025-08-07"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
    }
    "gpt-5.1" = {
      model_name    = "gpt-5.1"
      model_version = "2025-11-13"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
      location      = "swedencentral" # Not available in SAN
    }
    "gpt-5.1-codex" = {
      model_name    = "gpt-5.1-codex"
      model_version = "2025-11-13"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
      location      = "swedencentral" # Not available in SAN
    }

    # ==========================================================================
    # Reasoning Models (o-series) - Chain of Thought
    # ==========================================================================
    # Advanced reasoning with explicit thinking process
    # Best for complex analysis, planning, and multi-step problems
    "o3-mini" = {
      model_name    = "o3-mini"
      model_version = "2025-01-31"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
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
    # Image Generation (DALL-E)
    # ==========================================================================
    # For story illustrations, visual content creation
    # Deployed to East US (Standard SKU not available in South Africa North)
    "dall-e-3" = {
      model_name    = "dall-e-3"
      model_version = "3.0"
      model_format  = "OpenAI"
      sku_name      = "Standard"
      capacity      = 1
      location      = "eastus"
    }

    # ==========================================================================
    # Audio Models (Whisper & TTS)
    # ==========================================================================
    # Speech-to-text for voice input, text-to-speech for narration
    # Deployed to North Central US (only region with Whisper/TTS support)
    # NOTE: Disabled by default - enable when audio features needed
    "whisper" = {
      model_name    = "whisper"
      model_version = "001"
      model_format  = "OpenAI"
      sku_name      = "Standard"
      capacity      = 1
      location      = "northcentralus"
      enabled       = false # Enable when audio features needed
    }
    "tts" = {
      model_name    = "tts"
      model_version = "001"
      model_format  = "OpenAI"
      sku_name      = "Standard"
      capacity      = 1
      location      = "northcentralus"
      enabled       = false # Enable when audio features needed
    }
    "tts-hd" = {
      model_name    = "tts-hd"
      model_version = "001"
      model_format  = "OpenAI"
      sku_name      = "Standard"
      capacity      = 1
      location      = "northcentralus"
      enabled       = false # Enable when audio features needed
    }

    # ==========================================================================
    # Anthropic Claude Models (via Azure AI Model Catalog)
    # ==========================================================================
    # Note: Claude models are deployed via Azure AI Model Catalog (Serverless API)
    # They require marketplace subscription and use pay-as-you-go billing
    # Deploy via: az ml serverless-endpoint create or Azure AI Foundry portal
    # NOTE: Disabled - requires marketplace subscription first
    "claude-haiku-4-5" = {
      model_name    = "claude-3-5-haiku"
      model_version = "20241022"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - capacity is token-based
      enabled       = false # Requires marketplace subscription
    }
    "claude-sonnet-4-5" = {
      model_name    = "claude-sonnet-4-5"
      model_version = "20250514"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - capacity is token-based
      enabled       = false # Requires marketplace subscription
    }
    "claude-opus-4-5" = {
      model_name    = "claude-opus-4-5"
      model_version = "20250514"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - capacity is token-based
      enabled       = false # Requires marketplace subscription
    }

    # ==========================================================================
    # Cohere Models (via Azure AI Model Catalog)
    # ==========================================================================
    # Specialized models for RAG enhancement
    # Rerank: Improves search relevance by 10-30% for complex queries
    # Embed: Multi-language support including African languages
    # NOTE: Disabled by default - enable individually after core deployment
    "cohere-rerank-v4" = {
      model_name    = "Cohere-rerank-v4.0-pro"
      model_version = "1"
      model_format  = "Cohere"
      sku_name      = "GlobalStandard"
      capacity      = 1  # Serverless - pay per query
      location      = "uksouth" # Not available in SAN
      enabled       = false # Enable after core deployment succeeds
    }
    "cohere-embed-v4" = {
      model_name    = "embed-v-4-0"
      model_version = "1"
      model_format  = "Cohere"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth" # Not available in SAN
      enabled       = false # Enable after core deployment succeeds
    }

    # ==========================================================================
    # Mistral Models (via Azure AI Model Catalog)
    # ==========================================================================
    # Codestral: Dedicated code model, 256K context, 80+ languages
    # Much cheaper than gpt-5.1-codex for code tasks
    # NOTE: Disabled by default - enable individually after core deployment
    "codestral" = {
      model_name    = "Codestral"
      model_version = "2501"
      model_format  = "Mistral"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth" # Not available in SAN
      enabled       = false # Enable after core deployment succeeds
    }

    # ==========================================================================
    # DeepSeek Models (via Azure AI Model Catalog)
    # ==========================================================================
    # DeepSeek-V3: Advanced reasoning and agent performance
    # Strong benchmarks on code generation and understanding
    # NOTE: Disabled by default - enable individually after core deployment
    "deepseek-v3" = {
      model_name    = "DeepSeek-V3.2"
      model_version = "1"
      model_format  = "DeepSeek"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth" # Not available in SAN
      enabled       = false # Enable after core deployment succeeds
    }

    # ==========================================================================
    # AI21 Jamba Models (via Azure AI Model Catalog)
    # ==========================================================================
    # Jamba: Hybrid Mamba-Transformer architecture with long context
    # Linear scaling with context - efficient for long documents
    # Use for full story manuscript analysis, cross-chapter consistency
    # NOTE: Disabled by default - enable individually after core deployment
    "jamba-1.5-large" = {
      model_name    = "AI21-Jamba-1.5-Large"
      model_version = "1"
      model_format  = "AI21"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth" # Not available in SAN
      enabled       = false # Enable after core deployment succeeds
    }
    "jamba-1.5-mini" = {
      model_name    = "AI21-Jamba-1.5-Mini"
      model_version = "1"
      model_format  = "AI21"
      sku_name      = "GlobalStandard"
      capacity      = 1  # 10x cheaper than large for simpler long-context tasks
      location      = "uksouth" # Not available in SAN
      enabled       = false # Enable after core deployment succeeds
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
