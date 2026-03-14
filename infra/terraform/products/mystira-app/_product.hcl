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
  path_parts    = split("/", try(path_relative_to_include(), ""))
  has_env_path  = length(local.path_parts) >= 2 && contains(local.path_parts, "environments")
  env_from_path = local.has_env_path ? element(local.path_parts, length(local.path_parts) - 1) : ""
  environment   = local.env_from_path != "" && local.env_from_path != "mystira-app" ? local.env_from_path : get_env("TF_VAR_environment", "dev")
}

# Dependency on shared infrastructure
dependency "shared" {
  config_path = "${get_repo_root()}/infra/terraform/shared-infra/environments/${local.environment}"

  # Mock outputs for `terragrunt plan` when shared-infra hasn't been applied
  # Note: Only include outputs that are actually used in inputs below
  mock_outputs = {
    postgresql_server_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DBforPostgreSQL/flexibleServers/mock"
    postgresql_server_fqdn                 = "mock.postgres.database.azure.com"
    redis_cache_id                         = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.Cache/Redis/mock"
    redis_connection_string                = "mock-connection-string"
    redis_hostname                         = "mock.redis.cache.windows.net"
    redis_ssl_port                         = 6380
    cosmos_db_account_id                   = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.DocumentDB/databaseAccounts/mock"
    cosmos_db_connection_string            = "mock-connection-string"
    cosmos_db_endpoint                     = "https://mock-cosmos.documents.azure.com:443/"
    storage_account_id                     = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.Storage/storageAccounts/mock"
    storage_connection_string              = "mock-connection-string"
    storage_blob_endpoint                  = "https://mockstorage.blob.core.windows.net/"
    log_analytics_workspace_id             = "/subscriptions/xxx/resourceGroups/xxx/providers/Microsoft.OperationalInsights/workspaces/mock"
    application_insights_connection_string = "mock-connection-string"
    communication_service_primary_connection_string = "endpoint=https://mock.communication.azure.com/;accesskey=mock"
  }
  mock_outputs_allowed_terraform_commands = ["init", "validate", "plan"]

  # In CI, skip real output fetching to avoid backend initialization errors
  skip_outputs = tobool(get_env("TERRAGRUNT_SKIP_OUTPUTS", "false"))
}

inputs = {
  # Inherited from shared-infra dependency
  shared_postgresql_server_id                   = dependency.shared.outputs.postgresql_server_id
  shared_postgresql_server_fqdn                 = dependency.shared.outputs.postgresql_server_fqdn
  shared_redis_hostname                         = dependency.shared.outputs.redis_hostname
  shared_cosmos_db_account_id                   = dependency.shared.outputs.cosmos_db_account_id
  shared_cosmos_db_connection_string            = dependency.shared.outputs.cosmos_db_connection_string
  shared_cosmos_db_endpoint                     = dependency.shared.outputs.cosmos_db_endpoint
  shared_storage_account_id                     = dependency.shared.outputs.storage_account_id
  shared_storage_connection_string              = dependency.shared.outputs.storage_connection_string
  shared_storage_blob_endpoint                  = dependency.shared.outputs.storage_blob_endpoint
  shared_log_analytics_workspace_id             = dependency.shared.outputs.log_analytics_workspace_id
  shared_application_insights_connection_string = dependency.shared.outputs.application_insights_connection_string
  shared_acs_connection_string                  = dependency.shared.outputs.communication_service_primary_connection_string
}
