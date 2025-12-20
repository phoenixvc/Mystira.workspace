# Shared Monitoring Infrastructure Module - Azure
# Terraform module for deploying shared monitoring infrastructure (Log Analytics + Application Insights)

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "eastus"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "sku" {
  description = "Log Analytics SKU (PerGB2018, CapacityReservation, Free, PerNode, Premium, Standard, Standalone)"
  type        = string
  default     = "PerGB2018"
}

variable "retention_in_days" {
  description = "Log Analytics retention in days"
  type        = number
  default     = null
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  name_prefix = "mystira-shared-mon-${var.environment}"
  common_tags = merge(var.tags, {
    Component   = "shared-monitoring"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })
  
  retention_days = var.retention_in_days != null ? var.retention_in_days : (
    var.environment == "prod" ? 90 : 30
  )
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "shared" {
  name                = "${local.name_prefix}-logs"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  retention_in_days   = local.retention_days

  tags = local.common_tags
}

# Application Insights for workspace-level monitoring
resource "azurerm_application_insights" "shared" {
  name                = "${local.name_prefix}-insights"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.shared.id
  application_type    = "other"

  tags = local.common_tags
}

# Metric Alert: High Data Ingestion
# Note: Log Analytics workspaces don't have a "Usage" metric.
# Using "IngestionVolumeMB" to monitor data ingestion volume instead.
resource "azurerm_monitor_metric_alert" "high_ingestion" {
  name                = "${local.name_prefix}-high-ingestion"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_log_analytics_workspace.shared.id]
  description         = "Alert when data ingestion is unusually high"
  severity            = 2
  enabled             = var.environment == "prod" # Only enable in production
  frequency           = "PT5M"
  window_size         = "PT1H"

  criteria {
    metric_namespace = "Microsoft.OperationalInsights/workspaces"
    metric_name      = "IngestionVolumeMB"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 1000 # 1 GB per hour threshold
  }

  action {
    action_group_id = azurerm_monitor_action_group.default.id
  }

  tags = local.common_tags
}

# Action Group for Alerts
resource "azurerm_monitor_action_group" "default" {
  name                = "${local.name_prefix}-alerts"
  resource_group_name = var.resource_group_name
  short_name          = "MysAlerts"
  enabled             = true

  # Email notification (configure as needed)
  # email_receiver {
  #   name          = "sendtoadmin"
  #   email_address = "admin@example.com"
  # }

  tags = local.common_tags
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = azurerm_log_analytics_workspace.shared.id
}

output "log_analytics_workspace_name" {
  description = "Log Analytics Workspace name"
  value       = azurerm_log_analytics_workspace.shared.name
}

output "log_analytics_workspace_primary_shared_key" {
  description = "Log Analytics Workspace primary shared key"
  value       = azurerm_log_analytics_workspace.shared.primary_shared_key
  sensitive   = true
}

output "application_insights_id" {
  description = "Application Insights ID"
  value       = azurerm_application_insights.shared.id
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = azurerm_application_insights.shared.connection_string
  sensitive   = true
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = azurerm_application_insights.shared.instrumentation_key
  sensitive   = true
}

