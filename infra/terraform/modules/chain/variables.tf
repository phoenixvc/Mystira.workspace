# Mystira Chain Infrastructure Module - Variables
# Variable definitions for the chain module
# See main.tf for resource definitions and outputs.tf for outputs

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

variable "chain_node_count" {
  description = "Number of chain nodes to deploy"
  type        = number
  default     = 3

  validation {
    condition     = var.chain_node_count >= 1 && var.chain_node_count <= 10
    error_message = "Chain node count must be between 1 and 10."
  }
}

variable "chain_vm_size" {
  description = "Azure VM size for chain nodes"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "chain_storage_size_gb" {
  description = "Storage size in GB for chain data (minimum 100 GB for Premium file shares)"
  type        = number
  default     = 100

  validation {
    condition     = var.chain_storage_size_gb >= 100
    error_message = "Premium file shares require a minimum quota of 100 GB."
  }
}

variable "vnet_id" {
  description = "Virtual Network ID for chain deployment"
  type        = string
  default     = null
}

variable "subnet_id" {
  description = "Subnet ID for chain nodes"
  type        = string
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
