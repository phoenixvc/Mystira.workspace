# =============================================================================
# Mystira.App Infrastructure Module
# Converted from Bicep: https://github.com/phoenixvc/Mystira.App/infrastructure
# =============================================================================
#
# Resources deployed:
# - Cosmos DB (serverless) with 7 containers
# - App Service Plan + App Service (API backend)
# - Static Web App (Blazor WASM PWA)
# - Storage Account with blob container
# - Key Vault for secrets
# - Application Insights + Log Analytics
# - Communication Services (email)
# - Azure Bot (optional, for Teams)
#
# =============================================================================

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"  # 4.x required for .NET 9.0 support
    }
  }
}

# -----------------------------------------------------------------------------
# Data Sources
# -----------------------------------------------------------------------------

data "azurerm_client_config" "current" {}

# -----------------------------------------------------------------------------
# Local Variables
# -----------------------------------------------------------------------------

locals {
  # Naming convention: [org]-[env]-[project]-[type]-[region]
  region_code = lookup({
    "southafricanorth" = "san"
    "eastus2"          = "eus2"
    "westeurope"       = "weu"
    "northeurope"      = "neu"
  }, var.location, substr(var.location, 0, 4))

  fallback_region_code = lookup({
    "southafricanorth" = "san"
    "eastus2"          = "eus2"
    "westeurope"       = "weu"
    "northeurope"      = "neu"
  }, var.fallback_location, substr(var.fallback_location, 0, 4))

  name_prefix = "${var.org}-${var.environment}-${var.project_name}"

  # Storage account names can't have dashes and max 24 chars
  storage_account_name = replace("${var.org}${var.environment}${var.project_name}st${local.region_code}", "-", "")

  # Key Vault names max 24 chars
  key_vault_name = "${var.org}-${var.environment}-app-kv-${local.region_code}"

  common_tags = merge(var.tags, {
    Environment = var.environment
    Project     = var.project_name
    ManagedBy   = "terraform"
    Source      = "mystira-app-module"
  })

  # Cosmos DB containers configuration
  # Partition keys match DbContext ToJsonProperty mappings in Mystira.App
  cosmos_containers = [
    { name = "UserProfiles", partition_key = "/id" },        # Id → "id"
    { name = "Accounts", partition_key = "/id" },            # Id → "id"
    { name = "Scenarios", partition_key = "/id" },           # Id → "id"
    { name = "GameSessions", partition_key = "/accountId" }, # AccountId → "accountId"
    { name = "ContentBundles", partition_key = "/id" },      # Id → "id"
    { name = "PendingSignups", partition_key = "/email" },   # Email → "email"
    { name = "CompassTrackings", partition_key = "/id" },    # Axis → "id" (mapped)
  ]

  # Resolved monitoring resource references (shared or created)
  log_analytics_workspace_id       = var.use_shared_monitoring ? var.shared_log_analytics_workspace_id : azurerm_log_analytics_workspace.main[0].id
  application_insights_connection_string = var.use_shared_monitoring ? var.shared_application_insights_connection_string : azurerm_application_insights.main[0].connection_string
}

# =============================================================================
# Log Analytics Workspace (only created if not using shared monitoring)
# =============================================================================

resource "azurerm_log_analytics_workspace" "main" {
  count = var.use_shared_monitoring ? 0 : 1

  name                = "${local.name_prefix}-law-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.log_retention_days

  tags = local.common_tags
}

# =============================================================================
# Application Insights (only created if not using shared monitoring)
# =============================================================================

resource "azurerm_application_insights" "main" {
  count = var.use_shared_monitoring ? 0 : 1

  name                 = "${local.name_prefix}-ai-${local.region_code}"
  location             = var.location
  resource_group_name  = var.resource_group_name
  workspace_id         = azurerm_log_analytics_workspace.main[0].id
  application_type     = "web"
  daily_data_cap_in_gb = var.daily_quota_gb

  tags = local.common_tags
}

# =============================================================================
# Key Vault
# =============================================================================

resource "azurerm_key_vault" "main" {
  name                        = local.key_vault_name
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = false
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = var.environment == "prod"
  sku_name                    = var.key_vault_sku

  # Access policy for Terraform service principal
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get", "List", "Set", "Delete", "Purge", "Recover"
    ]
  }

  tags = local.common_tags
}

# Key Vault access policy for App Service managed identity
resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.api.identity[0].principal_id

  secret_permissions = ["Get", "List"]
}

# =============================================================================
# Storage Account
# =============================================================================

resource "azurerm_storage_account" "main" {
  count = var.skip_storage_creation ? 0 : 1

  name                     = local.storage_account_name
  location                 = var.location
  resource_group_name      = var.resource_group_name
  account_tier             = "Standard"
  account_replication_type = replace(var.storage_sku, "Standard_", "")
  account_kind             = "StorageV2"
  min_tls_version          = "TLS1_2"

  blob_properties {
    dynamic "cors_rule" {
      for_each = length(var.cors_allowed_origins) > 0 ? [1] : []
      content {
        allowed_headers    = ["*"]
        allowed_methods    = ["GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS"]
        allowed_origins    = var.cors_allowed_origins
        exposed_headers    = ["*"]
        max_age_in_seconds = 3600
      }
    }
  }

  tags = local.common_tags
}

# Primary media container (hot tier - frequently accessed content)
resource "azurerm_storage_container" "media" {
  count = var.skip_storage_creation ? 0 : 1

  name                  = "mystira-app-media"
  storage_account_id    = azurerm_storage_account.main[0].id
  container_access_type = "blob"
}

# Avatar images container (hot tier - profile pictures, avatars)
resource "azurerm_storage_container" "avatars" {
  count = var.skip_storage_creation ? 0 : 1

  name                  = "avatars"
  storage_account_id    = azurerm_storage_account.main[0].id
  container_access_type = "blob"
}

# Audio content container (mixed tier - music, sound effects)
resource "azurerm_storage_container" "audio" {
  count = var.skip_storage_creation ? 0 : 1

  name                  = "audio"
  storage_account_id    = azurerm_storage_account.main[0].id
  container_access_type = "blob"
}

# Scenario content bundles (cool tier - downloadable content packs)
resource "azurerm_storage_container" "content_bundles" {
  count = var.skip_storage_creation ? 0 : 1

  name                  = "content-bundles"
  storage_account_id    = azurerm_storage_account.main[0].id
  container_access_type = "blob"
}

# Game session recordings/exports (archive tier - historical data)
resource "azurerm_storage_container" "archives" {
  count = var.skip_storage_creation ? 0 : 1

  name                  = "archives"
  storage_account_id    = azurerm_storage_account.main[0].id
  container_access_type = "private"  # Private - accessed via SAS tokens
}

# Temporary uploads container (hot tier - transient content)
resource "azurerm_storage_container" "uploads" {
  count = var.skip_storage_creation ? 0 : 1

  name                  = "uploads"
  storage_account_id    = azurerm_storage_account.main[0].id
  container_access_type = "private"
}

# =============================================================================
# Storage Lifecycle Management Policy
# Implements tiered storage strategy from ADR-0013
# =============================================================================

resource "azurerm_storage_management_policy" "tiering" {
  count = var.skip_storage_creation || !var.enable_storage_tiering ? 0 : 1

  storage_account_id = azurerm_storage_account.main[0].id

  # Rule 1: Audio content - move to cool after 30 days, archive after 90
  rule {
    name    = "audio-tiering"
    enabled = true

    filters {
      prefix_match = ["audio/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = var.storage_tier_to_cool_days
        tier_to_archive_after_days_since_modification_greater_than = var.storage_tier_to_archive_days
      }
    }
  }

  # Rule 2: Content bundles - move to cool tier quickly (downloaded once, rarely re-accessed)
  rule {
    name    = "content-bundle-tiering"
    enabled = true

    filters {
      prefix_match = ["content-bundles/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = 14  # 2 weeks
        tier_to_archive_after_days_since_modification_greater_than = 60  # 2 months
      }
    }
  }

  # Rule 3: Archives - move to archive tier after 7 days (already infrequent access)
  rule {
    name    = "archive-tiering"
    enabled = true

    filters {
      prefix_match = ["archives/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_archive_after_days_since_modification_greater_than = 7
      }
    }
  }

  # Rule 4: Uploads - clean up temporary uploads after 7 days
  rule {
    name    = "uploads-cleanup"
    enabled = true

    filters {
      prefix_match = ["uploads/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 7
      }
    }
  }

  # Rule 5: Delete old snapshots across all containers
  rule {
    name    = "snapshot-cleanup"
    enabled = true

    filters {
      blob_types = ["blockBlob"]
    }

    actions {
      snapshot {
        delete_after_days_since_creation_greater_than = 30
      }
    }
  }

  # Rule 6: General media tiering (for media not in specialized containers)
  rule {
    name    = "general-media-tiering"
    enabled = var.storage_tier_to_cool_days > 0

    filters {
      prefix_match = ["mystira-app-media/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = var.storage_tier_to_cool_days
        tier_to_archive_after_days_since_modification_greater_than = var.storage_tier_to_archive_days
      }
    }
  }

  # Rule 7: Avatar images - keep in hot tier longer (frequently accessed)
  rule {
    name    = "avatar-tiering"
    enabled = true

    filters {
      prefix_match = ["avatars/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = 90   # 3 months
        tier_to_archive_after_days_since_modification_greater_than = 365  # 1 year
      }
    }
  }
}

# =============================================================================
# Cosmos DB
# =============================================================================

resource "azurerm_cosmosdb_account" "main" {
  count = var.skip_cosmos_creation ? 0 : 1

  name                = "${local.name_prefix}-cosmos-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  # Serverless capacity mode (recommended for dev/staging)
  dynamic "capabilities" {
    for_each = var.cosmos_db_serverless ? [1] : []
    content {
      name = "EnableServerless"
    }
  }

  consistency_policy {
    consistency_level = var.cosmos_db_consistency_level
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  tags = local.common_tags
}

resource "azurerm_cosmosdb_sql_database" "main" {
  count = var.skip_cosmos_creation ? 0 : 1

  name                = "MystiraAppDb"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.main[0].name
}

resource "azurerm_cosmosdb_sql_container" "containers" {
  for_each = var.skip_cosmos_creation ? {} : { for c in local.cosmos_containers : c.name => c }

  name                = each.value.name
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.main[0].name
  database_name       = azurerm_cosmosdb_sql_database.main[0].name
  partition_key_paths = [each.value.partition_key]  # Updated: partition_key_path deprecated in AzureRM 4.0

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }
  }
}

# =============================================================================
# App Service Plan
# =============================================================================

resource "azurerm_service_plan" "main" {
  name                = "${local.name_prefix}-asp-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.app_service_sku

  tags = local.common_tags
}

# =============================================================================
# App Service (API Backend)
# =============================================================================

resource "azurerm_linux_web_app" "api" {
  name                = "${local.name_prefix}-api-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.main.id

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = var.dotnet_version
    }

    always_on = var.app_service_sku != "F1" && var.app_service_sku != "D1"

    # Enable WebSockets for SignalR real-time communication
    websockets_enabled = true

    cors {
      allowed_origins = var.cors_allowed_origins
    }
  }

  app_settings = {
    # Core Settings
    "ASPNETCORE_ENVIRONMENT"                     = var.environment == "prod" ? "Production" : (var.environment == "staging" ? "Staging" : "Development")
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"        = "false"

    # Application Insights
    "APPLICATIONINSIGHTS_CONNECTION_STRING"      = local.application_insights_connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION" = "~3"
    "XDT_MicrosoftApplicationInsights_Mode"      = "recommended"

    # Connection Strings (matching Bicep naming) - Always use Key Vault references
    "ConnectionStrings__CosmosDb"      = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/cosmos-connection-string/)"
    "ConnectionStrings__AzureStorage"  = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/storage-connection-string/)"

    # Azure Resource Settings (matching Bicep naming)
    "Azure__CosmosDb__DatabaseName"    = var.skip_cosmos_creation ? var.shared_cosmos_database_name : "MystiraAppDb"
    "Azure__BlobStorage__ContainerName" = "mystira-app-media"

    # CORS Settings
    "CorsSettings__AllowedOrigins" = join(",", var.cors_allowed_origins)

    # JWT Settings (Key Vault references)
    "JwtSettings__Issuer"        = var.jwt_issuer != "" ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/jwt-issuer/)" : "MystiraAPI"
    "JwtSettings__Audience"      = var.jwt_audience != "" ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/jwt-audience/)" : "MystiraPWA"
    "JwtSettings__RsaPrivateKey" = var.jwt_private_key != "" ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/jwt-rsa-private-key/)" : ""
    "JwtSettings__RsaPublicKey"  = var.jwt_public_key != "" ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/jwt-rsa-public-key/)" : ""

    # Azure Communication Services - use Key Vault ref if either created or shared ACS is configured
    "AzureCommunicationServices__ConnectionString" = (var.enable_communication_services || var.shared_acs_connection_string != "") ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/acs-connection-string/)" : ""
    "AzureCommunicationServices__SenderEmail"      = var.sender_email

    # PostgreSQL (for hybrid data strategy)
    "ConnectionStrings__PostgreSQL" = var.enable_postgresql ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/postgresql-connection-string/)" : ""

    # Redis Cache
    "ConnectionStrings__Redis" = var.enable_redis ? "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/redis-connection-string/)" : ""

    # Data Migration Settings
    "DataMigration__Phase"   = tostring(var.data_migration_phase)
    "DataMigration__Enabled" = var.enable_postgresql ? "true" : "false"
  }

  tags = local.common_tags
}

# Store secrets in Key Vault
resource "azurerm_key_vault_secret" "cosmos_connection_string" {
  count = var.skip_cosmos_creation ? 0 : 1

  name         = "cosmos-connection-string"
  value        = azurerm_cosmosdb_account.main[0].primary_sql_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

# Store shared Cosmos DB connection string when using shared resources
resource "azurerm_key_vault_secret" "shared_cosmos_connection_string" {
  count = var.skip_cosmos_creation ? 1 : 0

  name         = "cosmos-connection-string"
  value        = var.existing_cosmos_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "storage_connection_string" {
  count = var.skip_storage_creation ? 0 : 1

  name         = "storage-connection-string"
  value        = azurerm_storage_account.main[0].primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

# Store shared Storage connection string when using shared resources
resource "azurerm_key_vault_secret" "shared_storage_connection_string" {
  count = var.skip_storage_creation ? 1 : 0

  name         = "storage-connection-string"
  value        = var.shared_storage_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "jwt_rsa_private_key" {
  count = var.jwt_private_key != "" ? 1 : 0

  name         = "jwt-rsa-private-key"
  value        = var.jwt_private_key
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "jwt_rsa_public_key" {
  count = var.jwt_public_key != "" ? 1 : 0

  name         = "jwt-rsa-public-key"
  value        = var.jwt_public_key
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "jwt_issuer" {
  count = var.jwt_issuer != "" ? 1 : 0

  name         = "jwt-issuer"
  value        = var.jwt_issuer
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "jwt_audience" {
  count = var.jwt_audience != "" ? 1 : 0

  name         = "jwt-audience"
  value        = var.jwt_audience
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "acs_connection_string" {
  count = var.enable_communication_services ? 1 : 0

  name         = "acs-connection-string"
  value        = azurerm_communication_service.main[0].primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

# Store shared ACS connection string when using shared resources
resource "azurerm_key_vault_secret" "shared_acs_connection_string" {
  count = !var.enable_communication_services && var.shared_acs_connection_string != "" ? 1 : 0

  name         = "acs-connection-string"
  value        = var.shared_acs_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

# =============================================================================
# Static Web App (Blazor WASM PWA)
# Note: Not available in South Africa North, deployed to fallback region
# =============================================================================

resource "azurerm_static_web_app" "main" {
  count = var.enable_static_web_app ? 1 : 0

  name                = "${local.name_prefix}-swa-${local.fallback_region_code}"
  location            = var.fallback_location  # Static Web Apps not available in South Africa North
  resource_group_name = var.resource_group_name
  sku_tier            = var.static_web_app_sku
  sku_size            = var.static_web_app_sku

  tags = local.common_tags
}

# Custom domain for Static Web App
resource "azurerm_static_web_app_custom_domain" "main" {
  count = var.enable_static_web_app && var.enable_app_custom_domain && var.app_custom_domain != "" ? 1 : 0

  static_web_app_id = azurerm_static_web_app.main[0].id
  domain_name       = var.app_custom_domain
  validation_type   = "cname-delegation"
}

# Custom domain for API App Service
resource "azurerm_app_service_custom_hostname_binding" "api" {
  count = var.enable_api_custom_domain && var.api_custom_domain != "" ? 1 : 0

  hostname            = var.api_custom_domain
  app_service_name    = azurerm_linux_web_app.api.name
  resource_group_name = var.resource_group_name
}

# =============================================================================
# Communication Services (Email)
# =============================================================================

resource "azurerm_communication_service" "main" {
  count = var.enable_communication_services ? 1 : 0

  name                = "${local.name_prefix}-acs-${local.region_code}"
  resource_group_name = var.resource_group_name
  data_location       = "Africa"  # Data residency

  tags = local.common_tags
}

resource "azurerm_email_communication_service" "main" {
  count = var.enable_communication_services ? 1 : 0

  name                = "${local.name_prefix}-ecs-${local.region_code}"
  resource_group_name = var.resource_group_name
  data_location       = "Africa"

  tags = local.common_tags
}

# =============================================================================
# Azure Bot (Optional - Teams Integration)
# =============================================================================

resource "azurerm_bot_service_azure_bot" "main" {
  count = var.enable_azure_bot && var.bot_microsoft_app_id != "" ? 1 : 0

  name                = "${local.name_prefix}-bot-${local.region_code}"
  resource_group_name = var.resource_group_name
  location            = "global"
  microsoft_app_id    = var.bot_microsoft_app_id
  sku                 = "F0"

  endpoint = "https://${azurerm_linux_web_app.api.default_hostname}/api/messages"

  tags = local.common_tags
}

# Teams channel for bot
resource "azurerm_bot_channel_ms_teams" "main" {
  count = var.enable_azure_bot && var.bot_microsoft_app_id != "" ? 1 : 0

  bot_name            = azurerm_bot_service_azure_bot.main[0].name
  resource_group_name = var.resource_group_name
  location            = azurerm_bot_service_azure_bot.main[0].location
}

# =============================================================================
# Budget (Optional)
# =============================================================================

resource "azurerm_consumption_budget_resource_group" "main" {
  count = var.enable_budget ? 1 : 0

  name              = "${local.name_prefix}-budget"
  resource_group_id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${var.resource_group_name}"

  amount     = var.monthly_budget
  time_grain = "Monthly"

  time_period {
    start_date = formatdate("YYYY-MM-01'T'00:00:00Z", timestamp())
  }

  notification {
    enabled        = true
    threshold      = 80
    operator       = "GreaterThan"
    threshold_type = "Actual"
    contact_emails = var.budget_alert_emails
  }

  notification {
    enabled        = true
    threshold      = 100
    operator       = "GreaterThan"
    threshold_type = "Forecasted"
    contact_emails = var.budget_alert_emails
  }

  lifecycle {
    ignore_changes = [time_period]
  }
}

# =============================================================================
# PostgreSQL Database (for hybrid data strategy)
# Uses shared PostgreSQL server from shared/postgresql module
# =============================================================================

resource "azurerm_postgresql_flexible_server_database" "mystira_app" {
  count = var.enable_postgresql && var.use_shared_postgresql ? 1 : 0

  name      = var.postgresql_database_name
  server_id = var.shared_postgresql_server_id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# Store PostgreSQL connection string in Key Vault
resource "azurerm_key_vault_secret" "postgresql_connection_string" {
  count = var.enable_postgresql && var.use_shared_postgresql ? 1 : 0

  name         = "postgresql-connection-string"
  value        = "Host=${var.shared_postgresql_server_fqdn};Port=5432;Username=${var.shared_postgresql_admin_login};Password=${var.shared_postgresql_admin_password};Database=${var.postgresql_database_name};SSL Mode=Require;Trust Server Certificate=True"
  key_vault_id = azurerm_key_vault.main.id
}

# =============================================================================
# Redis Cache (for session and data caching)
# =============================================================================

resource "azurerm_redis_cache" "main" {
  count = var.enable_redis ? 1 : 0

  name                = "${local.name_prefix}-redis-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  capacity            = var.redis_capacity
  family              = var.redis_family
  sku_name            = var.redis_sku
  non_ssl_port_enabled = false  # Renamed from enable_non_ssl_port in AzureRM 4.x
  minimum_tls_version = "1.2"

  redis_configuration {
    maxmemory_policy = "volatile-lru"
  }

  tags = local.common_tags
}

# Store Redis connection string in Key Vault
resource "azurerm_key_vault_secret" "redis_connection_string" {
  count = var.enable_redis ? 1 : 0

  name         = "redis-connection-string"
  value        = azurerm_redis_cache.main[0].primary_connection_string
  key_vault_id = azurerm_key_vault.main.id
}
