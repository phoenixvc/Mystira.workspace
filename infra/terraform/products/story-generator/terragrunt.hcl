# =============================================================================
# Story Generator - Product Configuration
# =============================================================================
# AI-powered story generation service:
#   - Blazor WASM frontend (Static Web App)
#   - .NET API (Kubernetes/AKS)
#   - PostgreSQL (uses shared)
#   - Redis (uses shared)
#
# Dependencies:
#   - shared-infra (PostgreSQL, Redis, Azure AI, Monitoring)
# =============================================================================

locals {
  # Extract environment from the path (e.g., "environments/dev" -> "dev")
  env_from_path = element(split("/", path_relative_to_include()), length(split("/", path_relative_to_include())) - 1)
  environment   = local.env_from_path != "" ? local.env_from_path : get_env("TF_VAR_environment", "dev")
}

include "root" {
  path = find_in_parent_folders()
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "${get_parent_terragrunt_dir()}/shared-infra/environments/${local.environment}"

  mock_outputs = {
    postgresql_server_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/mock"
    postgresql_server_fqdn                 = "mock.postgres.database.azure.com"
    redis_cache_id                         = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.Cache/Redis/mock"
    redis_connection_string                = "mock-connection-string"
    azure_ai_endpoint                      = "https://mock.cognitiveservices.azure.com"
    log_analytics_workspace_id             = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.OperationalInsights/workspaces/mock"
    application_insights_connection_string = "mock-connection-string"
  }
  mock_outputs_allowed_terraform_commands = ["validate", "plan"]
}

inputs = {
  # Inherited from shared-infra dependency
  shared_postgresql_server_id                   = dependency.shared.outputs.postgresql_server_id
  shared_postgresql_server_fqdn                 = dependency.shared.outputs.postgresql_server_fqdn
  shared_redis_cache_id                         = dependency.shared.outputs.redis_cache_id
  shared_redis_connection_string                = dependency.shared.outputs.redis_connection_string
  shared_azure_ai_endpoint                      = dependency.shared.outputs.azure_ai_endpoint
  shared_log_analytics_workspace_id             = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string
}
