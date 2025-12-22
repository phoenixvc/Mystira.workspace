# =============================================================================
# Mystira.App Infrastructure
# Blazor WASM PWA + ASP.NET Core API + Cosmos DB
# Converted from Bicep: https://github.com/phoenixvc/Mystira.App/infrastructure
# =============================================================================

# Resource Group for Mystira.App (separate from core services)
resource "azurerm_resource_group" "mystira_app" {
  name     = "mys-dev-mystira-rg-san"
  location = var.location

  tags = {
    Environment = "dev"
    Project     = "Mystira"
    Service     = "mystira-app"
    ManagedBy   = "terraform"
  }
}

module "mystira_app" {
  source = "../../modules/mystira-app"

  environment         = "dev"
  location            = var.location
  fallback_location   = "eastus2"  # Static Web Apps not available in South Africa North
  resource_group_name = azurerm_resource_group.mystira_app.name
  project_name        = "mystira"
  org                 = "mys"

  # -----------------------------------------------------------------------------
  # Cosmos DB Configuration
  # -----------------------------------------------------------------------------
  cosmos_db_serverless        = true  # Cost-effective for dev
  cosmos_db_consistency_level = "Session"
  skip_cosmos_creation        = false  # Set to true if importing existing

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
  # Storage Configuration
  # -----------------------------------------------------------------------------
  storage_sku          = "Standard_LRS"
  skip_storage_creation = false  # Set to true if importing existing

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
  # Communication Services
  # -----------------------------------------------------------------------------
  enable_communication_services = true
  sender_email                  = "DoNotReply@mystira.app"

  # -----------------------------------------------------------------------------
  # Azure Bot (Teams Integration) - Disabled for dev
  # -----------------------------------------------------------------------------
  enable_azure_bot     = false
  bot_microsoft_app_id = ""

  # -----------------------------------------------------------------------------
  # Monitoring Configuration
  # -----------------------------------------------------------------------------
  log_retention_days = 30
  daily_quota_gb     = 1
  enable_alerts      = false  # Disabled for dev to reduce noise

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
  value       = module.mystira_app.cosmos_db_endpoint
}

output "mystira_app_key_vault_uri" {
  description = "Mystira.App Key Vault URI"
  value       = module.mystira_app.key_vault_uri
}
