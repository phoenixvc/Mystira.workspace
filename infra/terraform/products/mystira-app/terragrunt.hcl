# =============================================================================
# Mystira.App - Product Configuration
# =============================================================================
# Main consumer-facing application:
#   - Blazor WASM PWA (Static Web App)
#   - .NET API (App Service)
#   - Cosmos DB (uses shared or dedicated)
#   - Communication Services (email)
#
# Dependencies:
#   - shared-infra (PostgreSQL, Redis, Storage, Monitoring)
# =============================================================================

locals {
  # Extract environment from the path (e.g., "environments/dev" -> "dev")
  # This works when running from products/mystira-app/environments/{env}/
  # Only derive from path if we're in an environments subdirectory
  path_parts    = split("/", path_relative_to_include())
  has_env_path  = length(local.path_parts) >= 2 && contains(local.path_parts, "environments")
  env_from_path = local.has_env_path ? element(local.path_parts, length(local.path_parts) - 1) : ""
  environment   = local.env_from_path != "" && local.env_from_path != "mystira-app" ? local.env_from_path : get_env("TF_VAR_environment", "dev")
}

include "root" {
  path = find_in_parent_folders()
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "${get_parent_terragrunt_dir()}/shared-infra/environments/${local.environment}"

  # Mock outputs for `terragrunt plan` when shared-infra hasn't been applied
  # Note: Only include outputs that are actually used in inputs below
  mock_outputs = {
    postgresql_server_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/mock"
    postgresql_server_fqdn                 = "mock.postgres.database.azure.com"
    redis_cache_id                         = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.Cache/Redis/mock"
    redis_connection_string                = "mock-connection-string"
    cosmos_db_account_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DocumentDB/databaseAccounts/mock"
    cosmos_db_connection_string            = "mock-connection-string"
    storage_account_id                     = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.Storage/storageAccounts/mock"
    storage_connection_string              = "mock-connection-string"
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
  shared_cosmos_db_account_id                   = dependency.shared.outputs.cosmos_db_account_id
  shared_cosmos_db_connection_string            = dependency.shared.outputs.cosmos_db_connection_string
  shared_storage_account_id                     = dependency.shared.outputs.storage_account_id
  shared_storage_connection_string              = dependency.shared.outputs.storage_connection_string
  shared_log_analytics_workspace_id             = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string
}
