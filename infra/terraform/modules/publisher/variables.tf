# Mystira Publisher Infrastructure Module - Variables
# Variable definitions for the publisher module
# See main.tf for resource definitions and outputs.tf for output values

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "eastus"
}

variable "region_code" {
  description = "Short region code (eus, euw, etc.) - defaults to 'eus' for eastus"
  type        = string
  default     = "eus"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "chain_rpc_endpoint" {
  description = "RPC endpoint for Mystira Chain"
  type        = string
  sensitive   = true
  default     = null
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace (from shared monitoring module)"
  type        = string
  default     = null
}

variable "use_shared_servicebus" {
  description = "Use shared Service Bus namespace instead of creating one"
  type        = bool
  default     = false
}

variable "shared_servicebus_queue_name" {
  description = "Name of the publisher queue in shared Service Bus (required when use_shared_servicebus = true)"
  type        = string
  default     = "publisher-events"
}
