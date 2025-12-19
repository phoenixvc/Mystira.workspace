variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod."
  }
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
