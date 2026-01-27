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

include "root" {
  path = find_in_parent_folders()
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "../../shared-infra/environments/${get_env("TF_VAR_environment", "dev")}"

  mock_outputs = {
    postgresql_server_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/mock"
    log_analytics_workspace_id             = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.OperationalInsights/workspaces/mock"
    application_insights_connection_string = "mock-connection-string"
  }
  mock_outputs_allowed_terraform_commands = ["validate", "plan"]
}

inputs = {
  # Inherited from shared-infra dependency
  shared_postgresql_server_id              = dependency.shared.outputs.postgresql_server_id
  shared_log_analytics_workspace_id        = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string
}
