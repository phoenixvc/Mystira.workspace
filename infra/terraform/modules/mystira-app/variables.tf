# =============================================================================
# Mystira.App Infrastructure Module - Variables
# Converted from Bicep: https://github.com/phoenixvc/Mystira.App/infrastructure
# =============================================================================

# -----------------------------------------------------------------------------
# General Settings
# -----------------------------------------------------------------------------

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "southafricanorth"
}

variable "fallback_location" {
  description = "Fallback region for resources not available in primary location (e.g., Static Web Apps)"
  type        = string
  default     = "eastus2"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "project_name" {
  description = "Project name for resource naming"
  type        = string
  default     = "mystira"
}

variable "org" {
  description = "Organization prefix for resource naming"
  type        = string
  default     = "mys"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# -----------------------------------------------------------------------------
# Cosmos DB Settings
# -----------------------------------------------------------------------------

variable "cosmos_db_serverless" {
  description = "Use serverless mode for Cosmos DB (recommended for dev/staging)"
  type        = bool
  default     = true
}

variable "cosmos_db_consistency_level" {
  description = "Consistency level for Cosmos DB"
  type        = string
  default     = "Session"
  validation {
    condition     = contains(["Strong", "BoundedStaleness", "Session", "ConsistentPrefix", "Eventual"], var.cosmos_db_consistency_level)
    error_message = "Must be one of: Strong, BoundedStaleness, Session, ConsistentPrefix, Eventual"
  }
}

variable "skip_cosmos_creation" {
  description = "Skip Cosmos DB creation (use existing)"
  type        = bool
  default     = false
}

variable "existing_cosmos_connection_string" {
  description = "Connection string for existing Cosmos DB (when skip_cosmos_creation is true)"
  type        = string
  default     = ""
  sensitive   = true
}

# -----------------------------------------------------------------------------
# App Service Settings
# -----------------------------------------------------------------------------

variable "app_service_sku" {
  description = "SKU for App Service Plan"
  type        = string
  default     = "B1"
}

variable "dotnet_version" {
  description = ".NET version for App Service (AzureRM 4.x supports: 3.1, 5.0, 6.0, 7.0, 8.0, 9.0)"
  type        = string
  default     = "9.0"

  validation {
    condition     = contains(["3.1", "5.0", "6.0", "7.0", "8.0", "9.0"], var.dotnet_version)
    error_message = "dotnet_version must be one of: 3.1, 5.0, 6.0, 7.0, 8.0, 9.0"
  }
}

variable "api_custom_domain" {
  description = "Custom domain for API (e.g., api.mystira.app)"
  type        = string
  default     = ""
}

variable "enable_api_custom_domain" {
  description = "Enable custom domain for API"
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Static Web App Settings
# -----------------------------------------------------------------------------

variable "enable_static_web_app" {
  description = "Deploy Static Web App for Blazor WASM PWA"
  type        = bool
  default     = true
}

variable "static_web_app_sku" {
  description = "SKU for Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "github_repository_url" {
  description = "GitHub repository URL for Static Web App deployment"
  type        = string
  default     = ""
}

variable "github_branch" {
  description = "GitHub branch for Static Web App deployment"
  type        = string
  default     = "dev"
}

variable "app_custom_domain" {
  description = "Custom domain for Static Web App (e.g., app.mystira.app)"
  type        = string
  default     = ""
}

variable "enable_app_custom_domain" {
  description = "Enable custom domain for Static Web App"
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Storage Settings
# -----------------------------------------------------------------------------

variable "storage_sku" {
  description = "SKU for Storage Account"
  type        = string
  default     = "Standard_LRS"
}

variable "skip_storage_creation" {
  description = "Skip Storage Account creation (use existing)"
  type        = bool
  default     = false
}

variable "cors_allowed_origins" {
  description = "CORS allowed origins for storage"
  type        = list(string)
  default     = []
}

variable "enable_storage_tiering" {
  description = "Enable automatic blob storage tiering with lifecycle management"
  type        = bool
  default     = true
}

variable "storage_tier_to_cool_days" {
  description = "Days after last modification to move blobs to cool tier"
  type        = number
  default     = 30
}

variable "storage_tier_to_archive_days" {
  description = "Days after last modification to move blobs to archive tier"
  type        = number
  default     = 90
}

variable "storage_delete_after_days" {
  description = "Days after last modification to delete blobs (0 = never delete)"
  type        = number
  default     = 0
}

# -----------------------------------------------------------------------------
# Communication Services Settings
# -----------------------------------------------------------------------------

variable "enable_communication_services" {
  description = "Deploy Azure Communication Services"
  type        = bool
  default     = true
}

variable "sender_email" {
  description = "Sender email address for Communication Services"
  type        = string
  default     = "DoNotReply@mystira.app"
}

# -----------------------------------------------------------------------------
# Azure Bot Settings
# -----------------------------------------------------------------------------

variable "enable_azure_bot" {
  description = "Deploy Azure Bot for Teams integration"
  type        = bool
  default     = false
}

variable "bot_microsoft_app_id" {
  description = "Microsoft App ID for Azure Bot"
  type        = string
  default     = ""
}

# -----------------------------------------------------------------------------
# Monitoring Settings
# -----------------------------------------------------------------------------

variable "use_shared_monitoring" {
  description = "Use shared Log Analytics and Application Insights instead of creating new ones"
  type        = bool
  default     = false
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace (when use_shared_monitoring is true)"
  type        = string
  default     = ""
}

variable "shared_application_insights_id" {
  description = "ID of shared Application Insights (when use_shared_monitoring is true)"
  type        = string
  default     = ""
}

variable "shared_application_insights_connection_string" {
  description = "Connection string of shared Application Insights (when use_shared_monitoring is true)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "log_retention_days" {
  description = "Log retention in days (only used when not using shared monitoring)"
  type        = number
  default     = 30
}

variable "daily_quota_gb" {
  description = "Daily quota for Application Insights in GB (only used when not using shared monitoring)"
  type        = number
  default     = 1
}

variable "enable_alerts" {
  description = "Enable metric alerts and availability tests"
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Budget Settings
# -----------------------------------------------------------------------------

variable "enable_budget" {
  description = "Enable budget tracking"
  type        = bool
  default     = false
}

variable "monthly_budget" {
  description = "Monthly budget limit in USD"
  type        = number
  default     = 50
}

variable "budget_alert_emails" {
  description = "Email addresses for budget alerts"
  type        = list(string)
  default     = []
}

# -----------------------------------------------------------------------------
# Key Vault Settings
# -----------------------------------------------------------------------------

variable "key_vault_sku" {
  description = "SKU for Key Vault"
  type        = string
  default     = "standard"
}

# -----------------------------------------------------------------------------
# JWT/Auth Settings (stored in Key Vault)
# -----------------------------------------------------------------------------

variable "jwt_issuer" {
  description = "JWT issuer"
  type        = string
  default     = ""
}

variable "jwt_audience" {
  description = "JWT audience"
  type        = string
  default     = ""
}

variable "jwt_private_key" {
  description = "JWT RSA private key (stored in Key Vault)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "jwt_public_key" {
  description = "JWT RSA public key (stored in Key Vault)"
  type        = string
  default     = ""
  sensitive   = true
}

# -----------------------------------------------------------------------------
# Integration Settings
# -----------------------------------------------------------------------------

variable "discord_bot_token" {
  description = "Discord bot token (optional)"
  type        = string
  default     = ""
  sensitive   = true
}

variable "enable_whatsapp" {
  description = "Enable WhatsApp integration"
  type        = bool
  default     = false
}

variable "whatsapp_callback_token" {
  description = "WhatsApp webhook callback token"
  type        = string
  default     = ""
  sensitive   = true
}

# -----------------------------------------------------------------------------
# PostgreSQL Settings (for hybrid data strategy)
# -----------------------------------------------------------------------------

variable "use_shared_postgresql" {
  description = "Use shared PostgreSQL server instead of creating dedicated one"
  type        = bool
  default     = true
}

variable "shared_postgresql_server_id" {
  description = "ID of shared PostgreSQL server (required if use_shared_postgresql = true)"
  type        = string
  default     = ""
}

variable "shared_postgresql_server_fqdn" {
  description = "FQDN of shared PostgreSQL server (required if use_shared_postgresql = true)"
  type        = string
  default     = ""
}

variable "shared_postgresql_admin_login" {
  description = "Admin login for shared PostgreSQL server"
  type        = string
  default     = ""
}

variable "shared_postgresql_admin_password" {
  description = "Admin password for shared PostgreSQL server"
  type        = string
  default     = ""
  sensitive   = true
}

variable "postgresql_database_name" {
  description = "PostgreSQL database name for Mystira.App"
  type        = string
  default     = "mystiraapp"
}

variable "enable_postgresql" {
  description = "Enable PostgreSQL for hybrid data strategy (Phase 1+)"
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Redis Cache Settings
# -----------------------------------------------------------------------------

variable "enable_redis" {
  description = "Enable Redis cache for session and data caching"
  type        = bool
  default     = false
}

variable "redis_sku" {
  description = "Redis cache SKU (Basic, Standard, Premium)"
  type        = string
  default     = "Basic"
}

variable "redis_capacity" {
  description = "Redis cache capacity (0-6 for Basic/Standard, 1-4 for Premium)"
  type        = number
  default     = 0
}

variable "redis_family" {
  description = "Redis cache family (C for Basic/Standard, P for Premium)"
  type        = string
  default     = "C"
}

# -----------------------------------------------------------------------------
# Data Migration Settings
# -----------------------------------------------------------------------------

variable "data_migration_phase" {
  description = "Current data migration phase (0=CosmosOnly, 1=DualWriteCosmosRead, 2=DualWritePostgresRead, 3=PostgresOnly)"
  type        = number
  default     = 0

  validation {
    condition     = var.data_migration_phase >= 0 && var.data_migration_phase <= 3
    error_message = "data_migration_phase must be between 0 and 3."
  }
}
