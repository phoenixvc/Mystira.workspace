# =============================================================================
# Admin Services - Production Environment
# =============================================================================

locals {
  environment = "prod"
}

include "root" {
  path = find_in_parent_folders()
}

include "product" {
  path = find_in_parent_folders("terragrunt.hcl", "${get_terragrunt_dir()}/../../terragrunt.hcl")
}

terraform {
  source = "${get_terragrunt_dir()}/."
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "${get_parent_terragrunt_dir()}/shared-infra/environments/${local.environment}"

  mock_outputs = {
    postgresql_server_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/mock"
    log_analytics_workspace_id             = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.OperationalInsights/workspaces/mock"
    application_insights_connection_string = "InstrumentationKey=mock;IngestionEndpoint=https://mock.in.applicationinsights.azure.com/"
  }
  mock_outputs_allowed_terraform_commands = ["validate", "plan"]
}

# Production-specific configuration
inputs = {
  # Shared infrastructure dependencies
  shared_postgresql_server_id                   = dependency.shared.outputs.postgresql_server_id
  shared_log_analytics_workspace_id             = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string

  # Admin UI (Static Web App)
  admin_ui_sku = "Standard"

  # Custom domains
  enable_custom_domain = true

  # Admin API scaling
  admin_api_min_replicas = 2
  admin_api_max_replicas = 5
}
