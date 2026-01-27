# =============================================================================
# Publisher - Product Configuration
# =============================================================================
# Publishing service for content distribution:
#   - Publisher API (.NET on Kubernetes/AKS)
#   - Event processing via Service Bus
#
# Dependencies:
#   - shared-infra (Service Bus, Monitoring)
# =============================================================================

include "root" {
  path = find_in_parent_folders()
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "../../shared-infra/environments/${get_env("TF_VAR_environment", "dev")}"

  mock_outputs = {
    servicebus_namespace_id               = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.ServiceBus/namespaces/mock"
    servicebus_connection_string          = "mock-connection-string"
    log_analytics_workspace_id            = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.OperationalInsights/workspaces/mock"
    application_insights_connection_string = "mock-connection-string"
  }
  mock_outputs_allowed_terraform_commands = ["validate", "plan"]
}

inputs = {
  # Inherited from shared-infra dependency
  shared_servicebus_namespace_id          = dependency.shared.outputs.servicebus_namespace_id
  shared_servicebus_connection_string     = dependency.shared.outputs.servicebus_connection_string
  shared_log_analytics_workspace_id       = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string
}
