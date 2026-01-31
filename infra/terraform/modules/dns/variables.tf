# Mystira DNS Infrastructure Module - Variables
# Module: infra/terraform/modules/dns
# This file contains all input variable definitions for the DNS module.

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "domain_name" {
  description = "Base domain name (e.g., mystira.app)"
  type        = string
  default     = "mystira.app"
}

variable "publisher_ip" {
  description = "IP address for publisher service ingress"
  type        = string
  default     = ""
}

variable "chain_ip" {
  description = "IP address for chain service ingress"
  type        = string
  default     = ""
}

# Front Door Configuration
variable "use_front_door" {
  description = "Use Azure Front Door (CNAME) instead of direct Load Balancer (A record)"
  type        = bool
  default     = false
}

variable "front_door_publisher_endpoint" {
  description = "Front Door publisher endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_chain_endpoint" {
  description = "Front Door chain endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_publisher_validation_token" {
  description = "Front Door publisher custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "front_door_chain_validation_token" {
  description = "Front Door chain custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

# Mystira.App Front Door Configuration
variable "front_door_mystira_app_swa_endpoint" {
  description = "Front Door Mystira.App SWA endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_mystira_app_api_endpoint" {
  description = "Front Door Mystira.App API endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_mystira_app_swa_validation_token" {
  description = "Front Door Mystira.App SWA custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "front_door_mystira_app_api_validation_token" {
  description = "Front Door Mystira.App API custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

# Admin Services Front Door Configuration
variable "front_door_admin_api_endpoint" {
  description = "Front Door Admin API endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_admin_ui_endpoint" {
  description = "Front Door Admin UI endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_admin_api_validation_token" {
  description = "Front Door Admin API custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "front_door_admin_ui_validation_token" {
  description = "Front Door Admin UI custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

# Story Generator Front Door Configuration
variable "front_door_story_api_endpoint" {
  description = "Front Door Story Generator API endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_story_swa_endpoint" {
  description = "Front Door Story Generator SWA endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_story_api_validation_token" {
  description = "Front Door Story Generator API custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "front_door_story_swa_validation_token" {
  description = "Front Door Story Generator SWA custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
