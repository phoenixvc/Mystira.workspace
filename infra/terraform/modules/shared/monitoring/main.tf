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

variable "alert_email_addresses" {
  description = "List of email addresses to receive alerts"
  type        = list(string)
  default     = []
}

locals {
  name_prefix = "mys-${var.environment}-core"
  common_tags = merge(var.tags, {
    Component   = "shared-monitoring"
    Environment = var.environment
    Service     = "core"
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })

  retention_days = var.retention_in_days != null ? var.retention_in_days : (
    var.environment == "prod" ? 90 : 30
  )
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "shared" {
  name                = "${local.name_prefix}-log"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku
  retention_in_days   = local.retention_days

  tags = local.common_tags
}

# Application Insights for workspace-level monitoring
resource "azurerm_application_insights" "shared" {
  name                = "${local.name_prefix}-ai"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.shared.id
  application_type    = "other"

  tags = local.common_tags
}

# Scheduled Query Rule Alert: High Data Ingestion
# Uses log-based query against Usage table for accurate ingestion monitoring
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "high_ingestion" {
  name                = "${local.name_prefix}-high-ingestion"
  resource_group_name = var.resource_group_name
  location            = var.location
  description         = "Alert when data ingestion is unusually high"
  severity            = 2
  enabled             = var.environment == "prod" # Only enable in production

  evaluation_frequency = "PT5M"
  window_duration      = "PT1H"
  scopes               = [azurerm_log_analytics_workspace.shared.id]

  criteria {
    query = <<-QUERY
      Usage
      | where TimeGenerated > ago(1h)
      | summarize IngestionMB = sum(Quantity) by bin(TimeGenerated, 1h)
      | where IngestionMB > 1000
    QUERY

    time_aggregation_method = "Count"
    threshold               = 0
    operator                = "GreaterThan"

    failing_periods {
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
    }
  }

  action {
    action_groups = [azurerm_monitor_action_group.default.id]
  }

  tags = local.common_tags
}

# Action Group for Alerts
resource "azurerm_monitor_action_group" "default" {
  name                = "${local.name_prefix}-alerts"
  resource_group_name = var.resource_group_name
  short_name          = "MysAlerts"
  enabled             = true

  dynamic "email_receiver" {
    for_each = var.alert_email_addresses
    content {
      name          = "email-${email_receiver.key}"
      email_address = email_receiver.value
    }
  }

  tags = local.common_tags
}

# Scheduled Query Rule Alert: High Error Rate
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "high_error_rate" {
  name                = "${local.name_prefix}-high-error-rate"
  resource_group_name = var.resource_group_name
  location            = var.location
  description         = "Alert when error rate exceeds threshold"
  severity            = 1
  enabled             = var.environment == "prod"

  evaluation_frequency = "PT5M"
  window_duration      = "PT15M"
  scopes               = [azurerm_application_insights.shared.id]

  criteria {
    query = <<-QUERY
      requests
      | where timestamp > ago(15m)
      | summarize
          Total = count(),
          Failed = countif(success == false)
        by bin(timestamp, 5m)
      | extend ErrorRate = (Failed * 100.0) / Total
      | where ErrorRate > 5
    QUERY

    time_aggregation_method = "Count"
    threshold               = 0
    operator                = "GreaterThan"

    failing_periods {
      minimum_failing_periods_to_trigger_alert = 2
      number_of_evaluation_periods             = 3
    }
  }

  action {
    action_groups = [azurerm_monitor_action_group.default.id]
  }

  tags = local.common_tags
}

# Scheduled Query Rule Alert: Slow Response Times
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "slow_response" {
  name                = "${local.name_prefix}-slow-response"
  resource_group_name = var.resource_group_name
  location            = var.location
  description         = "Alert when response times are slow (P95 > 2s)"
  severity            = 2
  enabled             = var.environment == "prod"

  evaluation_frequency = "PT5M"
  window_duration      = "PT15M"
  scopes               = [azurerm_application_insights.shared.id]

  criteria {
    query = <<-QUERY
      requests
      | where timestamp > ago(15m)
      | summarize P95 = percentile(duration, 95) by bin(timestamp, 5m)
      | where P95 > 2000
    QUERY

    time_aggregation_method = "Count"
    threshold               = 0
    operator                = "GreaterThan"

    failing_periods {
      minimum_failing_periods_to_trigger_alert = 2
      number_of_evaluation_periods             = 3
    }
  }

  action {
    action_groups = [azurerm_monitor_action_group.default.id]
  }

  tags = local.common_tags
}

# Scheduled Query Rule Alert: Unhandled Exceptions
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "exceptions" {
  name                = "${local.name_prefix}-exceptions"
  resource_group_name = var.resource_group_name
  location            = var.location
  description         = "Alert when unhandled exceptions exceed threshold"
  severity            = 2
  enabled             = var.environment == "prod"

  evaluation_frequency = "PT5M"
  window_duration      = "PT15M"
  scopes               = [azurerm_application_insights.shared.id]

  criteria {
    query = <<-QUERY
      exceptions
      | where timestamp > ago(15m)
      | summarize ExceptionCount = count() by bin(timestamp, 5m)
      | where ExceptionCount > 10
    QUERY

    time_aggregation_method = "Count"
    threshold               = 0
    operator                = "GreaterThan"

    failing_periods {
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 3
    }
  }

  action {
    action_groups = [azurerm_monitor_action_group.default.id]
  }

  tags = local.common_tags
}

# Scheduled Query Rule Alert: Dependency Failures
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "dependency_failures" {
  name                = "${local.name_prefix}-dependency-failures"
  resource_group_name = var.resource_group_name
  location            = var.location
  description         = "Alert when external dependency calls fail"
  severity            = 2
  enabled             = var.environment == "prod"

  evaluation_frequency = "PT5M"
  window_duration      = "PT15M"
  scopes               = [azurerm_application_insights.shared.id]

  criteria {
    query = <<-QUERY
      dependencies
      | where timestamp > ago(15m)
      | summarize
          Total = count(),
          Failed = countif(success == false)
        by bin(timestamp, 5m), target
      | extend FailureRate = (Failed * 100.0) / Total
      | where FailureRate > 10
    QUERY

    time_aggregation_method = "Count"
    threshold               = 0
    operator                = "GreaterThan"

    failing_periods {
      minimum_failing_periods_to_trigger_alert = 2
      number_of_evaluation_periods             = 3
    }
  }

  action {
    action_groups = [azurerm_monitor_action_group.default.id]
  }

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

