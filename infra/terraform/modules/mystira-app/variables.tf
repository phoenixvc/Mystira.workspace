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
  description = ".NET version for App Service"
  type        = string
  default     = "9.0"
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

variable "log_retention_days" {
  description = "Log retention in days"
  type        = number
  default     = 30
}

variable "daily_quota_gb" {
  description = "Daily quota for Application Insights in GB"
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
