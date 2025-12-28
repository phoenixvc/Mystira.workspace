# =============================================================================
# Mystira.App Infrastructure - Staging
# Blazor WASM PWA + ASP.NET Core API + Cosmos DB
# =============================================================================

# =============================================================================
# Shared Resources for Mystira.App (in core-rg)
# =============================================================================

# Shared Cosmos DB for Mystira.App
module "shared_cosmos_db" {
  source = "../../modules/shared/cosmos-db"

  environment         = "staging"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name

  serverless        = true
  consistency_level = "Session"

  databases = {
    "MystiraAppDb" = {
      containers = [
        { name = "UserProfiles", partition_key = "/id" },
        { name = "Accounts", partition_key = "/id" },
        { name = "Scenarios", partition_key = "/id" },
        { name = "GameSessions", partition_key = "/accountId" },
        { name = "ContentBundles", partition_key = "/id" },
        { name = "PendingSignups", partition_key = "/email" },
        { name = "CompassTrackings", partition_key = "/id" },
      ]
    }
  }

  tags = local.common_tags
}

# Shared Storage for Mystira.App media and content
module "shared_storage" {
  source = "../../modules/shared/storage"

  environment         = "staging"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name

  storage_sku = "Standard_LRS"

  cors_allowed_origins = [
    "https://staging.mystira.app",
    "https://staging.app.mystira.app",
    "https://mystira.app",
  ]

  containers = {
    "mystira-app-media" = { access_type = "blob" }
    "avatars"           = { access_type = "blob" }
    "audio"             = { access_type = "blob" }
    "content-bundles"   = { access_type = "blob" }
    "archives"          = { access_type = "private" }
    "uploads"           = { access_type = "private" }
  }

  enable_tiering = true

  tags = local.common_tags
}

# Shared Communication Services
resource "azurerm_resource_group" "shared_comms" {
  name     = "mys-staging-comms-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "communication-services" })
}

module "shared_comms" {
  source = "../../modules/shared/communications"

  resource_group_name = azurerm_resource_group.shared_comms.name
  location            = azurerm_resource_group.shared_comms.location

  tags = local.common_tags
}

# =============================================================================
# Mystira.App Module (in app-rg)
# =============================================================================

module "mystira_app" {
  source = "../../modules/mystira-app"

  environment         = "staging"
  location            = var.location
  fallback_location   = "eastus2"  # Static Web Apps not available in South Africa North
  resource_group_name = azurerm_resource_group.app.name
  project_name        = "mystira"
  org                 = "mys"

  # Cosmos DB - USE SHARED
  skip_cosmos_creation              = true
  existing_cosmos_connection_string = module.shared_cosmos_db.primary_sql_connection_string
  shared_cosmos_endpoint            = module.shared_cosmos_db.endpoint
  shared_cosmos_database_name       = "MystiraAppDb"

  # App Service Configuration
  app_service_sku = "B1"
  dotnet_version  = "9.0"

  # Custom domain for staging API
  enable_api_custom_domain = true
  api_custom_domain        = "staging.api.mystira.app"

  # Static Web App Configuration
  enable_static_web_app = true
  static_web_app_sku    = "Free"
  github_repository_url = "https://github.com/phoenixvc/Mystira.App"
  github_branch         = "main"

  # Custom domain for staging
  enable_app_custom_domain = true
  app_custom_domain        = "staging.app.mystira.app"

  # Storage - USE SHARED
  skip_storage_creation            = true
  shared_storage_connection_string = module.shared_storage.primary_connection_string
  shared_storage_blob_endpoint     = module.shared_storage.primary_blob_endpoint

  cors_allowed_origins = [
    "https://staging.mystira.app",
    "https://staging.app.mystira.app",
    "https://mystira.app",
  ]

  # Communication Services - USE SHARED
  enable_communication_services = false
  shared_acs_connection_string  = module.shared_comms.communication_service_primary_connection_string
  sender_email                  = "DoNotReply@mystira.app"

  # Azure Bot - Disabled for staging
  enable_azure_bot     = false
  bot_microsoft_app_id = ""

  # Monitoring - USE SHARED
  use_shared_monitoring                         = true
  shared_log_analytics_workspace_id             = module.shared_monitoring.log_analytics_workspace_id
  shared_application_insights_id                = module.shared_monitoring.application_insights_id
  shared_application_insights_connection_string = module.shared_monitoring.application_insights_connection_string

  enable_alerts = true

  # Budget
  enable_budget       = false
  monthly_budget      = 100
  budget_alert_emails = []

  tags = local.common_tags

  depends_on = [
    module.shared_monitoring,
    module.shared_cosmos_db,
    module.shared_storage,
    module.shared_comms
  ]
}

# =============================================================================
# Outputs
# =============================================================================

output "mystira_app_api_url" {
  description = "Mystira.App API URL"
  value       = module.mystira_app.app_service_url
}

output "mystira_app_web_url" {
  description = "Mystira.App Web URL (Static Web App)"
  value       = module.mystira_app.static_web_app_url
}

output "mystira_app_cosmos_endpoint" {
  description = "Mystira.App Cosmos DB endpoint"
  value       = module.shared_cosmos_db.endpoint
}

output "mystira_app_key_vault_uri" {
  description = "Mystira.App Key Vault URI"
  value       = module.mystira_app.key_vault_uri
}

output "mystira_app_swa_default_hostname" {
  description = "Mystira.App SWA default hostname (for Front Door)"
  value       = module.mystira_app.static_web_app_default_hostname
}

output "mystira_app_api_default_hostname" {
  description = "Mystira.App API default hostname (for Front Door)"
  value       = module.mystira_app.app_service_default_hostname
}
