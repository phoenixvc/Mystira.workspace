# =============================================================================
# Admin Services - Product Configuration
# =============================================================================
# Administrative services for platform management:
#   - Admin UI (React Static Web App)
#   - Admin API (.NET API on Kubernetes/AKS)
#
# Dependencies:
#   - shared-infra (PostgreSQL, Monitoring)
# =============================================================================

locals {
  # Extract environment from the path (e.g., "environments/dev" -> "dev")
  env_from_path = try(element(split("/", path_relative_to_include()), length(split("/", path_relative_to_include())) - 1), "")
  environment   = local.env_from_path != "" ? local.env_from_path : get_env("TF_VAR_environment", "dev")
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "${get_repo_root()}/infra/terraform/shared-infra/environments/${local.environment}"

  mock_outputs = {
    postgresql_server_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/mock"
    log_analytics_workspace_id             = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.OperationalInsights/workspaces/mock"
    application_insights_connection_string = "mock-connection-string"
  }
  mock_outputs_allowed_terraform_commands = ["init", "validate", "plan"]

  # In CI, skip real output fetching to avoid backend initialization errors
  skip_outputs = tobool(get_env("TERRAGRUNT_SKIP_OUTPUTS", "false"))
}

inputs = {
  # Inherited from shared-infra dependency
  shared_postgresql_server_id                   = dependency.shared.outputs.postgresql_server_id
  shared_log_analytics_workspace_id             = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string
}
