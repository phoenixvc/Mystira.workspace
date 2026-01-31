# Mystira Publisher Infrastructure Module - Variables
# Variable definitions for the publisher module
# See main.tf for resource definitions and outputs.tf for output values

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

variable "publisher_replica_count" {
  description = "Number of publisher service replicas"
  type        = number
  default     = 2
}

variable "vnet_id" {
  description = "Virtual Network ID for publisher deployment"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID for publisher service"
  type        = string
}

variable "chain_rpc_endpoint" {
  description = "RPC endpoint for Mystira Chain"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace (from shared monitoring module)"
  type        = string
}

variable "use_shared_servicebus" {
  description = "Use shared Service Bus namespace instead of creating one"
  type        = bool
  default     = false
}

variable "shared_servicebus_namespace_id" {
  description = "ID of shared Service Bus namespace (required when use_shared_servicebus = true)"
  type        = string
  default     = null
}

variable "shared_servicebus_queue_name" {
  description = "Name of the publisher queue in shared Service Bus (required when use_shared_servicebus = true)"
  type        = string
  default     = "publisher-events"
}
