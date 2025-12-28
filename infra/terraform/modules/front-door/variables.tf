variable "environment" {
  description = "Deployment environment (dev, staging, prod, nonprod)"
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod", "nonprod"], var.environment)
    error_message = "Environment must be dev, staging, prod, or nonprod."
  }
}

# =============================================================================
# Secondary Environment Support (for shared non-prod Front Door)
# =============================================================================
# When enable_secondary_environment is true, this Front Door instance will
# also handle domains/backends for a second environment (e.g., dev + staging)

variable "enable_secondary_environment" {
  description = "Enable secondary environment support (e.g., dev Front Door also handles staging)"
  type        = bool
  default     = false
}

variable "secondary_environment" {
  description = "Secondary environment name (e.g., staging when primary is dev)"
  type        = string
  default     = ""
}

# Secondary Publisher/Chain
variable "secondary_publisher_backend_address" {
  description = "Backend address for Publisher service in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_chain_backend_address" {
  description = "Backend address for Chain service in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_publisher" {
  description = "Custom domain for Publisher in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_chain" {
  description = "Custom domain for Chain in secondary environment"
  type        = string
  default     = ""
}

# Secondary Admin Services
variable "secondary_admin_api_backend_address" {
  description = "Backend address for Admin API in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_admin_ui_backend_address" {
  description = "Backend address for Admin UI in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_admin_api" {
  description = "Custom domain for Admin API in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_admin_ui" {
  description = "Custom domain for Admin UI in secondary environment"
  type        = string
  default     = ""
}

# Secondary Story Generator
variable "secondary_story_generator_api_backend_address" {
  description = "Backend address for Story Generator API in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_story_generator_swa_backend_address" {
  description = "Backend address for Story Generator SWA in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_story_generator_api" {
  description = "Custom domain for Story Generator API in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_story_generator_swa" {
  description = "Custom domain for Story Generator SWA in secondary environment"
  type        = string
  default     = ""
}

# Secondary Mystira.App
variable "secondary_mystira_app_api_backend_address" {
  description = "Backend address for Mystira.App API in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_mystira_app_swa_backend_address" {
  description = "Backend address for Mystira.App SWA in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_mystira_app_api" {
  description = "Custom domain for Mystira.App API in secondary environment"
  type        = string
  default     = ""
}

variable "secondary_custom_domain_mystira_app_swa" {
  description = "Custom domain for Mystira.App SWA in secondary environment"
  type        = string
  default     = ""
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "eastus"
}

variable "project_name" {
  description = "Project name (e.g., mystira)"
  type        = string
  default     = "mystira"
}

variable "publisher_backend_address" {
  description = "Backend address for Publisher service (e.g., dev.publisher.mystira.app)"
  type        = string
}

variable "chain_backend_address" {
  description = "Backend address for Chain service (e.g., dev.chain.mystira.app)"
  type        = string
}

variable "custom_domain_publisher" {
  description = "Custom domain for Publisher (e.g., dev.publisher.mystira.app)"
  type        = string
}

variable "custom_domain_chain" {
  description = "Custom domain for Chain (e.g., dev.chain.mystira.app)"
  type        = string
}

# Admin services (optional)
variable "enable_admin_services" {
  description = "Enable admin-api and admin-ui endpoints in Front Door"
  type        = bool
  default     = false
}

variable "admin_api_backend_address" {
  description = "Backend address for Admin API service (e.g., dev.admin-api.mystira.app)"
  type        = string
  default     = ""
}

variable "admin_ui_backend_address" {
  description = "Backend address for Admin UI service (e.g., dev.admin.mystira.app)"
  type        = string
  default     = ""
}

variable "custom_domain_admin_api" {
  description = "Custom domain for Admin API (e.g., dev.admin-api.mystira.app)"
  type        = string
  default     = ""
}

variable "custom_domain_admin_ui" {
  description = "Custom domain for Admin UI (e.g., dev.admin.mystira.app)"
  type        = string
  default     = ""
}

# Story Generator services (optional)
variable "enable_story_generator" {
  description = "Enable story-generator API and SWA endpoints in Front Door"
  type        = bool
  default     = false
}

variable "story_generator_api_backend_address" {
  description = "Backend address for Story Generator API (e.g., dev.story-api.mystira.app)"
  type        = string
  default     = ""
}

variable "story_generator_swa_backend_address" {
  description = "Backend address for Story Generator SWA (Blazor WASM frontend)"
  type        = string
  default     = ""
}

variable "custom_domain_story_generator_api" {
  description = "Custom domain for Story Generator API (e.g., dev.story-api.mystira.app)"
  type        = string
  default     = ""
}

variable "custom_domain_story_generator_swa" {
  description = "Custom domain for Story Generator SWA (e.g., dev.story.mystira.app)"
  type        = string
  default     = ""
}

# Mystira.App services (optional)
variable "enable_mystira_app" {
  description = "Enable Mystira.App API and SWA endpoints in Front Door"
  type        = bool
  default     = false
}

variable "mystira_app_api_backend_address" {
  description = "Backend address for Mystira.App API (e.g., mys-dev-app-api-san.azurewebsites.net)"
  type        = string
  default     = ""
}

variable "mystira_app_swa_backend_address" {
  description = "Backend address for Mystira.App SWA (Blazor WASM PWA)"
  type        = string
  default     = ""
}

variable "custom_domain_mystira_app_api" {
  description = "Custom domain for Mystira.App API (e.g., api.mystira.app or dev.api.mystira.app)"
  type        = string
  default     = ""
}

variable "custom_domain_mystira_app_swa" {
  description = "Custom domain for Mystira.App SWA (e.g., app.mystira.app or dev.mystira.app)"
  type        = string
  default     = ""
}

variable "enable_waf" {
  description = "Enable Web Application Firewall"
  type        = bool
  default     = true
}

variable "waf_mode" {
  description = "WAF mode: Detection or Prevention"
  type        = string
  default     = "Prevention"
  validation {
    condition     = contains(["Detection", "Prevention"], var.waf_mode)
    error_message = "WAF mode must be Detection or Prevention."
  }
}

variable "enable_caching" {
  description = "Enable caching for static content"
  type        = bool
  default     = true
}

variable "cache_duration_seconds" {
  description = "Cache duration in seconds for static content"
  type        = number
  default     = 3600 # 1 hour
}

variable "rate_limit_threshold" {
  description = "Number of requests per minute before rate limiting"
  type        = number
  default     = 100
}

variable "health_probe_path" {
  description = "Health probe path for backend services"
  type        = string
  default     = "/health"
}

variable "health_probe_interval" {
  description = "Health probe interval in seconds"
  type        = number
  default     = 30
}

variable "session_affinity_enabled" {
  description = "Enable session affinity (sticky sessions)"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
