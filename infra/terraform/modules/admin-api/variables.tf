# Mystira Admin-API Module - Variables
# See main.tf for module documentation

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
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

variable "vnet_id" {
  description = "Virtual Network ID for admin-api deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for admin-api service"
  type        = string
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace (from shared monitoring module)"
  type        = string
}

variable "shared_postgresql_server_id" {
  description = "ID of shared PostgreSQL server (from shared/postgresql module)"
  type        = string
  default     = null
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
