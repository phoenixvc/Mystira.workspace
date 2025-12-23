# Shared Communications Module - Azure
# Cross-environment Communication Services (Email, SMS)
# Shared across all environments with configuration-based separation

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

variable "resource_group_name" {
  description = "Name of the resource group for Communication Services"
  type        = string
}

variable "location" {
  description = "Azure region for resource group (Communication Services are global)"
  type        = string
  default     = "global"
}

variable "name_prefix" {
  description = "Prefix for resource names"
  type        = string
  default     = "mys-shared"
}

variable "data_location" {
  description = "Data residency location for Communication Services"
  type        = string
  default     = "Africa"

  validation {
    condition     = contains(["Africa", "Asia Pacific", "Australia", "Brazil", "Canada", "Europe", "France", "Germany", "India", "Japan", "Korea", "Norway", "Switzerland", "UAE", "UK", "United States"], var.data_location)
    error_message = "Data location must be a valid Azure Communication Services data location."
  }
}

variable "enable_email_service" {
  description = "Enable Email Communication Service"
  type        = bool
  default     = true
}

variable "email_domains" {
  description = "Custom email domains to configure"
  type = list(object({
    name                = string
    domain_management   = string # AzureManaged or CustomerManaged
    user_engagement_tracking_enabled = bool
  }))
  default = []
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  common_tags = merge(var.tags, {
    Component   = "communications"
    Environment = "shared"
    Service     = "comms"
    ManagedBy   = "terraform"
    Project     = "Mystira"
    Shared      = "all-environments"
  })
}

# Communication Service (global, shared across environments)
resource "azurerm_communication_service" "shared" {
  name                = "${var.name_prefix}-acs"
  resource_group_name = var.resource_group_name
  data_location       = var.data_location

  tags = local.common_tags
}

# Email Communication Service
resource "azurerm_email_communication_service" "shared" {
  count = var.enable_email_service ? 1 : 0

  name                = "${var.name_prefix}-ecs"
  resource_group_name = var.resource_group_name
  data_location       = var.data_location

  tags = local.common_tags
}

# Azure-managed email domain (for quick setup)
resource "azurerm_email_communication_service_domain" "azure_managed" {
  count = var.enable_email_service ? 1 : 0

  name                         = "AzureManagedDomain"
  email_service_id             = azurerm_email_communication_service.shared[0].id
  domain_management            = "AzureManaged"
  user_engagement_tracking_enabled = false

  tags = local.common_tags
}

# Custom email domains (if specified)
resource "azurerm_email_communication_service_domain" "custom" {
  for_each = { for d in var.email_domains : d.name => d }

  name                         = each.value.name
  email_service_id             = azurerm_email_communication_service.shared[0].id
  domain_management            = each.value.domain_management
  user_engagement_tracking_enabled = each.value.user_engagement_tracking_enabled

  tags = local.common_tags
}

# Outputs
output "communication_service_id" {
  description = "Communication Service ID"
  value       = azurerm_communication_service.shared.id
}

output "communication_service_name" {
  description = "Communication Service name"
  value       = azurerm_communication_service.shared.name
}

output "communication_service_primary_connection_string" {
  description = "Communication Service primary connection string"
  value       = azurerm_communication_service.shared.primary_connection_string
  sensitive   = true
}

output "communication_service_secondary_connection_string" {
  description = "Communication Service secondary connection string"
  value       = azurerm_communication_service.shared.secondary_connection_string
  sensitive   = true
}

output "email_service_id" {
  description = "Email Communication Service ID"
  value       = var.enable_email_service ? azurerm_email_communication_service.shared[0].id : null
}

output "email_service_name" {
  description = "Email Communication Service name"
  value       = var.enable_email_service ? azurerm_email_communication_service.shared[0].name : null
}

output "azure_managed_domain_id" {
  description = "Azure-managed email domain ID"
  value       = var.enable_email_service ? azurerm_email_communication_service_domain.azure_managed[0].id : null
}

output "azure_managed_domain_mail_from" {
  description = "Azure-managed email domain mail-from address"
  value       = var.enable_email_service ? azurerm_email_communication_service_domain.azure_managed[0].mail_from_sender_domain : null
}
