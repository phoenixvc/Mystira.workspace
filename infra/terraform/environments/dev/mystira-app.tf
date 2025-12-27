# =============================================================================
# Mystira.App Infrastructure
# Blazor WASM PWA + ASP.NET Core API + Cosmos DB
# Converted from Bicep: https://github.com/phoenixvc/Mystira.App/infrastructure
# =============================================================================
#
# Resource Distribution (per ADR-0017):
# - Shared resources (Cosmos DB, Storage) → core-rg
# - App-specific compute (App Service, SWA, Key Vault) → app-rg
# - Communication Services → mys-shared-comms-rg-glob (cross-environment)
#
# =============================================================================

# =============================================================================
# Shared Resources (in core-rg)
# =============================================================================

# Shared Cosmos DB for Mystira.App and Admin API
module "shared_cosmos_db" {
  source = "../../modules/shared/cosmos-db"

  environment         = "dev"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name  # core-rg

  serverless        = true  # Cost-effective for dev
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

  environment         = "dev"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name  # core-rg

  storage_sku = "Standard_LRS"

  cors_allowed_origins = [
    "https://app.mystira.app",
    "https://dev.mystira.app",
    "http://localhost:5000",
    "http://localhost:5001",
    "http://localhost:5173",
    "http://127.0.0.1:5000",
    "http://127.0.0.1:5001",
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

# =============================================================================
# App-Specific Resources (in app-rg)
# =============================================================================

module "mystira_app" {
  source = "../../modules/mystira-app"

  environment         = "dev"
  location            = var.location
  fallback_location   = "eastus2"  # Static Web Apps not available in South Africa North
  resource_group_name = azurerm_resource_group.app.name  # app-rg for app-specific resources
  project_name        = "mystira"
  org                 = "mys"

  # -----------------------------------------------------------------------------
  # Cosmos DB Configuration - USE SHARED
  # -----------------------------------------------------------------------------
  skip_cosmos_creation           = true
  existing_cosmos_connection_string = module.shared_cosmos_db.primary_sql_connection_string
  shared_cosmos_endpoint         = module.shared_cosmos_db.endpoint
  shared_cosmos_database_name    = "MystiraAppDb"

  # -----------------------------------------------------------------------------
  # App Service Configuration (API Backend)
  # -----------------------------------------------------------------------------
  app_service_sku = "B1"
  dotnet_version  = "9.0"

  # Custom domain (enable after initial deployment)
  enable_api_custom_domain = false
  api_custom_domain        = "api.mystira.app"

  # -----------------------------------------------------------------------------
  # Static Web App Configuration (Blazor WASM PWA)
  # -----------------------------------------------------------------------------
  enable_static_web_app = true
  static_web_app_sku    = "Free"
  github_repository_url = "https://github.com/phoenixvc/Mystira.App"
  github_branch         = "dev"

  # Custom domain (enable after initial deployment)
  enable_app_custom_domain = false
  app_custom_domain        = "app.mystira.app"

  # -----------------------------------------------------------------------------
  # Storage Configuration - USE SHARED
  # -----------------------------------------------------------------------------
  skip_storage_creation            = true
  shared_storage_connection_string = module.shared_storage.primary_connection_string
  shared_storage_blob_endpoint     = module.shared_storage.primary_blob_endpoint

  cors_allowed_origins = [
    "https://app.mystira.app",
    "https://dev.mystira.app",
    "http://localhost:5000",
    "http://localhost:5001",
    "http://localhost:5173",
    "http://127.0.0.1:5000",
    "http://127.0.0.1:5001",
  ]

  # -----------------------------------------------------------------------------
  # Communication Services - USE SHARED (cross-environment)
  # -----------------------------------------------------------------------------
  enable_communication_services = false  # Use shared
  shared_acs_connection_string  = module.shared_comms.communication_service_primary_connection_string
  sender_email                  = "DoNotReply@mystira.app"

  # -----------------------------------------------------------------------------
  # Azure Bot (Teams Integration) - Disabled for dev
  # -----------------------------------------------------------------------------
  enable_azure_bot     = false
  bot_microsoft_app_id = ""

  # -----------------------------------------------------------------------------
  # Monitoring Configuration - USE SHARED MONITORING
  # -----------------------------------------------------------------------------
  use_shared_monitoring                         = true
  shared_log_analytics_workspace_id             = module.shared_monitoring.log_analytics_workspace_id
  shared_application_insights_id                = module.shared_monitoring.application_insights_id
  shared_application_insights_connection_string = module.shared_monitoring.application_insights_connection_string

  enable_alerts = false  # Disabled for dev to reduce noise

  # -----------------------------------------------------------------------------
  # Budget Configuration
  # -----------------------------------------------------------------------------
  enable_budget       = false  # Enable in prod
  monthly_budget      = 50
  budget_alert_emails = []

  # -----------------------------------------------------------------------------
  # Tags
  # -----------------------------------------------------------------------------
  tags = local.common_tags

  # Ensure shared resources are created first
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
  description = "Mystira.App Cosmos DB endpoint (shared)"
  value       = module.shared_cosmos_db.endpoint
}

output "mystira_app_key_vault_uri" {
  description = "Mystira.App Key Vault URI"
  value       = module.mystira_app.key_vault_uri
}

output "shared_storage_blob_endpoint" {
  description = "Shared Storage blob endpoint"
  value       = module.shared_storage.primary_blob_endpoint
}
