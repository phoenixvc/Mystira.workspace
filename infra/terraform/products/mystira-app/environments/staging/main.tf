# =============================================================================
# Mystira App - Staging Environment
# =============================================================================
# Consumer-facing Blazor WASM PWA with .NET API backend
# =============================================================================

variable "environment" {
  type        = string
  description = "Deployment environment name (e.g., 'staging'). Used for resource naming and tagging. Passed from Terragrunt environment configuration."
}

variable "location" {
  type        = string
  description = "Primary Azure region for resource deployment (e.g., 'eastus2'). Passed from Terragrunt root configuration."
}

variable "resource_group_name" {
  type        = string
  description = "Name of the Azure resource group where Mystira App resources will be deployed. Created by shared-infra layer."
}

variable "tags" {
  type        = map(string)
  description = "Common resource tags applied to all Mystira App resources. Includes environment, product, and cost-center metadata from Terragrunt."
}

# Shared infrastructure inputs
variable "shared_postgresql_server_id" {
  type        = string
  description = "Azure resource ID of the shared PostgreSQL Flexible Server from shared-infra. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.DBforPostgreSQL/flexibleServers/{name}"
}

variable "shared_postgresql_server_fqdn" {
  type        = string
  description = "Fully qualified domain name of the shared PostgreSQL server (e.g., 'mystira-staging-pg.postgres.database.azure.com'). Used for database connection strings."
}

variable "shared_cosmos_db_connection_string" {
  type        = string
  sensitive   = true
  description = "Primary connection string for the shared Cosmos DB account from shared-infra. Contains account endpoint and key for NoSQL API access."
}

variable "shared_storage_connection_string" {
  type        = string
  sensitive   = true
  description = "Connection string for the shared Azure Storage account from shared-infra. Used for blob storage, queues, and static assets."
}

variable "shared_log_analytics_workspace_id" {
  type        = string
  description = "Azure resource ID of the shared Log Analytics workspace from shared-infra. Used as the diagnostic settings destination for monitoring and alerting."
}

variable "shared_application_insights_connection_string" {
  type        = string
  sensitive   = true
  description = "Connection string for the shared Application Insights instance from shared-infra. Contains instrumentation key and ingestion endpoint for APM telemetry."
}

# =============================================================================
# Mystira App Module
# =============================================================================

module "mystira_app" {
  source = "../../../../modules/mystira-app"

  environment         = var.environment
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  # Pass shared infrastructure references
  shared_postgresql_server_id                = var.shared_postgresql_server_id
  shared_postgresql_server_fqdn              = var.shared_postgresql_server_fqdn
  existing_cosmos_connection_string          = var.shared_cosmos_db_connection_string
  shared_storage_connection_string           = var.shared_storage_connection_string
  shared_log_analytics_workspace_id          = var.shared_log_analytics_workspace_id
  shared_application_insights_connection_string = var.shared_application_insights_connection_string
}

# =============================================================================
# Outputs
# =============================================================================

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = module.mystira_app.static_web_app_url
}

output "api_url" {
  description = "API service URL"
  value       = module.mystira_app.app_service_url
}
