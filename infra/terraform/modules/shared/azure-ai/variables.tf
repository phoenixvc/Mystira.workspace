# Azure AI Foundry Module Variables
# Updated: December 2025 - Implements ADR-0020 AI Model Selection Strategy

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
# Model Deployments (ADR-0020: 32 Models)
# =============================================================================
# Supports both OpenAI models and Azure AI Model Catalog models
# model_format: "OpenAI" for GPT models, "Anthropic" for Claude, etc.
#
# Supported model formats:
#   - OpenAI: GPT, DALL-E, Whisper, TTS, embedding models, o-series
#   - Anthropic: Claude models (haiku, sonnet, opus)
#   - Cohere: Rerank, Embed, Command models
#   - Meta: Llama models
#   - Mistral: Mistral, Codestral, Pixtral models
#   - DeepSeek: DeepSeek-V3, DeepSeek-R1, DeepSeek-Coder models
#   - AI21: Jamba models
#   - xAI: Grok models

variable "model_deployments" {
  description = "Map of model deployments to create"
  type = map(object({
    model_name      = string
    model_version   = string
    model_format    = optional(string, "OpenAI") # OpenAI, Anthropic, Cohere, Meta, Mistral, DeepSeek, AI21, xAI
    sku_name        = optional(string, "Standard")
    capacity        = optional(number, 10)
    rai_policy_name = optional(string, null)   # Responsible AI policy name
    location        = optional(string, null)   # Override region for models not available in primary region
    enabled         = optional(bool, true)     # Set to false to skip deploying this model
  }))

  validation {
    condition = alltrue([
      for k, v in var.model_deployments :
      contains(["OpenAI", "Anthropic", "Cohere", "Meta", "Mistral", "DeepSeek", "AI21", "xAI"], v.model_format)
    ])
    error_message = "model_format must be one of: OpenAI, Anthropic, Cohere, Meta, Mistral, DeepSeek, AI21, xAI"
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
    # See ADR-0020 for model selection rationale
    # ==========================================================================

    # GPT-4o - Flagship multimodal model
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
    # GPT-4.1 Series - Enhanced reasoning models (1M context)
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
      location      = "swedencentral"
    }
    "gpt-5.1-codex" = {
      model_name    = "gpt-5.1-codex"
      model_version = "2025-11-13"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 20
      location      = "swedencentral"
    }
    # GPT-5.2 - Latest and smartest (December 2025, 400K context)
    # NOTE: GlobalStandard not available in South Africa North, using Sweden Central
    "gpt-5.2" = {
      model_name    = "gpt-5.2"
      model_version = "2025-12-11"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
      location      = "swedencentral"
    }

    # ==========================================================================
    # Reasoning Models (o-series) - Chain of Thought
    # Advanced reasoning with explicit thinking process
    # ==========================================================================
    "o3" = {
      model_name    = "o3"
      model_version = "2025-04-16"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }
    "o3-mini" = {
      model_name    = "o3-mini"
      model_version = "2025-01-31"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }
    "o4-mini" = {
      model_name    = "o4-mini"
      model_version = "2025-04-16"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }

    # ==========================================================================
    # Embedding Models (for RAG / Vector Search)
    # ==========================================================================
    "text-embedding-3-large" = {
      model_name    = "text-embedding-3-large"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 120
    }
    "text-embedding-3-small" = {
      model_name    = "text-embedding-3-small"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 120
    }

    # ==========================================================================
    # Image Generation (DALL-E, gpt-image-1)
    # ==========================================================================
    "dall-e-3" = {
      model_name    = "dall-e-3"
      model_version = "3.0"
      model_format  = "OpenAI"
      sku_name      = "Standard"
      capacity      = 1
      location      = "eastus"
    }
    # gpt-image-1 - Not yet available in Azure OpenAI
    "gpt-image-1" = {
      model_name    = "gpt-image-1"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "Standard"
      capacity      = 1
      location      = "eastus"
      enabled       = false # Not available in Azure OpenAI yet
    }

    # ==========================================================================
    # Audio Models (Whisper & TTS)
    # ==========================================================================
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
    # Claude 4.5 with hybrid reasoning (Auto/Fast/Thinking modes)
    # NOTE: Requires marketplace subscription - deploy via script
    # ==========================================================================
    "claude-haiku-4-5" = {
      model_name    = "claude-3-5-haiku"
      model_version = "20241022"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false # Deploy via deploy-claude-models.sh
    }
    "claude-sonnet-4-5" = {
      model_name    = "claude-sonnet-4-5"
      model_version = "20250514"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false # Deploy via deploy-claude-models.sh
    }
    "claude-opus-4-5" = {
      model_name    = "claude-opus-4-5"
      model_version = "20251124"
      model_format  = "Anthropic"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false # Deploy via deploy-claude-models.sh
    }

    # ==========================================================================
    # Cohere Models (via Azure AI Model Catalog)
    # ==========================================================================
    "cohere-rerank-v4" = {
      model_name    = "Cohere-rerank-v4.0-pro"
      model_version = "1"
      model_format  = "Cohere"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }
    "cohere-embed-v4" = {
      model_name    = "embed-v-4-0"
      model_version = "1"
      model_format  = "Cohere"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }

    # ==========================================================================
    # Mistral Models (via Azure AI Model Catalog)
    # ==========================================================================
    "codestral" = {
      model_name    = "Codestral"
      model_version = "2501"
      model_format  = "Mistral"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }

    # ==========================================================================
    # DeepSeek Models (via Azure AI Model Catalog)
    # Cost-effective reasoning alternatives
    # ==========================================================================
    "deepseek-v3.1" = {
      model_name    = "DeepSeek-V3.1"
      model_version = "1"
      model_format  = "DeepSeek"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }
    "deepseek-r1" = {
      model_name    = "DeepSeek-R1"
      model_version = "1"
      model_format  = "DeepSeek"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }
    "deepseek-coder-v2" = {
      model_name    = "DeepSeek-Coder-V2-236B"
      model_version = "1"
      model_format  = "DeepSeek"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }

    # ==========================================================================
    # AI21 Jamba Models (via Azure AI Model Catalog)
    # Long context (256K tokens)
    # ==========================================================================
    "jamba-1.5-large" = {
      model_name    = "AI21-Jamba-1.5-Large"
      model_version = "1"
      model_format  = "AI21"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }
    "jamba-1.5-mini" = {
      model_name    = "AI21-Jamba-1.5-Mini"
      model_version = "1"
      model_format  = "AI21"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }

    # ==========================================================================
    # xAI Grok Models (via Azure AI Model Catalog)
    # Alternative reasoning models
    # ==========================================================================
    "grok-3" = {
      model_name    = "grok-3"
      model_version = "1"
      model_format  = "xAI"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
    }

    # ==========================================================================
    # Meta Llama Models (via Azure AI Model Catalog)
    # Latest open-source models
    # ==========================================================================
    "llama-4-maverick" = {
      model_name    = "Llama-4-Maverick-17B-128E-Instruct-FP8"
      model_version = "1"
      model_format  = "Meta"
      sku_name      = "GlobalStandard"
      capacity      = 1
      location      = "uksouth"
      enabled       = false
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
