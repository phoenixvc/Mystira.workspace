# Mystira Production Environment - Azure
# Terraform configuration for production environment

terraform {
  required_version = ">= 1.5.0"

  backend "azurerm" {
    resource_group_name  = "mys-shared-terraform-rg-san"
    storage_account_name = "myssharedtfstatesan"
    container_name       = "tfstate"
    key                  = "prod/terraform.tfstate"
    use_azuread_auth     = true
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"  # 4.x required for .NET 9.0 support
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.47"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.0"  # Required for AI Foundry projects and catalog models
    }
  }
}

provider "azapi" {}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
    }
  }
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "southafricanorth"
}

variable "location_secondary" {
  description = "Secondary Azure region for DR"
  type        = string
  default     = "westus2"
}

variable "external_id_tenant_id" {
  description = "Microsoft Entra External ID tenant ID (optional - set when External ID tenant is created)"
  type        = string
  default     = ""
}

variable "alert_email_addresses" {
  description = "Email addresses for monitoring alerts"
  type        = list(string)
  default     = ["devops@mystira.app"]
}

variable "oidc_issuer_enabled" {
  description = "Enable OIDC issuer for AKS workload identity"
  type        = bool
  default     = true
}

variable "workload_identity_enabled" {
  description = "Enable workload identity for AKS"
  type        = bool
  default     = true
}

# Common tags for all resources
locals {
  common_tags = {
    Environment = "prod"
    Project     = "Mystira"
    ManagedBy   = "terraform"
    Critical    = "true"
  }
  region_code = "san"
}

# =============================================================================
# Resource Groups (ADR-0017: Resource Group Organization Strategy)
# =============================================================================

# Tier 1: Core Resource Group (shared infrastructure)
resource "azurerm_resource_group" "main" {
  name     = "mys-prod-core-rg-san"
  location = var.location

  tags = {
    Environment = "prod"
    Project     = "Mystira"
    Service     = "core"
    ManagedBy   = "terraform"
    Critical    = "true"
  }
}

# Tier 2: Service-Specific Resource Groups
resource "azurerm_resource_group" "chain" {
  name     = "mys-prod-chain-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "chain" })
}

resource "azurerm_resource_group" "publisher" {
  name     = "mys-prod-publisher-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "publisher" })
}

resource "azurerm_resource_group" "story" {
  name     = "mys-prod-story-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "story-generator" })
}

resource "azurerm_resource_group" "admin" {
  name     = "mys-prod-admin-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "admin-api" })
}

resource "azurerm_resource_group" "app" {
  name     = "mys-prod-app-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "app" })
}

# =============================================================================
# Networking (in core-rg)
# =============================================================================

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "mys-prod-core-vnet-san"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = ["10.2.0.0/16"]

  tags = {
    Environment = "prod"
    Critical    = "true"
  }
}

resource "azurerm_subnet" "chain" {
  name                 = "chain-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.1.0/24"]
}

resource "azurerm_subnet" "publisher" {
  name                 = "publisher-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.2.0/24"]
}

resource "azurerm_subnet" "aks" {
  name                 = "aks-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.10.0/22"]
}

resource "azurerm_subnet" "postgresql" {
  name                 = "postgresql-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.3.0/24"]

  delegation {
    name = "postgresql-delegation"
    service_delegation {
      name = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

resource "azurerm_subnet" "redis" {
  name                 = "redis-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.4.0/24"]
  # Note: Standard SKU Redis doesn't support VNet integration
  # Remove delegation if using Standard SKU, add back if upgrading to Premium
}

resource "azurerm_subnet" "story_generator" {
  name                 = "story-generator-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.5.0/24"]
}

resource "azurerm_subnet" "admin_api" {
  name                 = "admin-api-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.6.0/24"]
}

# Chain Infrastructure (in chain-rg per ADR-0017)
module "chain" {
  source = "../../modules/chain"

  environment                       = "prod"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.chain.name
  chain_node_count                  = 3
  chain_vm_size                     = "Standard_D4s_v3"
  chain_storage_size_gb             = 500
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.chain.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Publisher Infrastructure (in publisher-rg per ADR-0017)
module "publisher" {
  source = "../../modules/publisher"

  environment                       = "prod"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.publisher.name
  publisher_replica_count           = 3
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.publisher.id
  chain_rpc_endpoint                = "http://mys-chain.mys-prod.svc.cluster.local:8545"
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  # Use shared Service Bus (in core-rg per ADR-0017)
  use_shared_servicebus          = true
  shared_servicebus_namespace_id = module.shared_servicebus.namespace_id
  shared_servicebus_queue_name   = "publisher-events"

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Shared PostgreSQL Infrastructure
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  environment             = "prod"
  location                = var.location
  resource_group_name     = azurerm_resource_group.main.name
  vnet_id                 = azurerm_virtual_network.main.id
  subnet_id               = azurerm_subnet.postgresql.id
  enable_vnet_integration = true

  databases = [
    "storygenerator",
    "publisher",
    "adminapi"
  ]

  sku_name                     = "GP_Standard_D4s_v3"
  storage_mb                   = 65536
  backup_retention_days        = 35
  geo_redundant_backup_enabled = true

  # Enable Azure AD authentication for passwordless access
  aad_auth_enabled = true
  aad_admin_identities = {
    "admin-api" = {
      principal_name = "mys-prod-admin-api-identity-san"
      principal_type = "ServicePrincipal"
    }
    "story-generator" = {
      principal_name = "mys-prod-story-identity-san"
      principal_type = "ServicePrincipal"
    }
  }

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Shared Redis Infrastructure
# Note: Standard SKU does not support VNet integration (subnet_id)
# Only Premium SKU supports subnet deployment
module "shared_redis" {
  source = "../../modules/shared/redis"

  environment         = "prod"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  # subnet_id omitted - Standard SKU doesn't support VNet integration

  capacity = 2
  family   = "C"
  sku_name = "Standard"

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Shared Service Bus Infrastructure (in core-rg per ADR-0017)
module "shared_servicebus" {
  source = "../../modules/shared/servicebus"

  environment         = "prod"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "Premium"
  capacity            = 1
  zone_redundant      = true

  queues = {
    "publisher-events" = {
      max_delivery_count                   = 3
      default_message_ttl                  = "P1D"
      dead_lettering_on_message_expiration = true
    }
    "admin-notifications" = {
      max_delivery_count                   = 5
      default_message_ttl                  = "P7D"
      dead_lettering_on_message_expiration = true
    }
    "app-events" = {
      max_delivery_count                   = 5
      default_message_ttl                  = "P7D"
      dead_lettering_on_message_expiration = true
    }
  }

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Shared Monitoring Infrastructure
module "shared_monitoring" {
  source = "../../modules/shared/monitoring"

  environment         = "prod"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name

  retention_in_days     = 90
  alert_email_addresses = var.alert_email_addresses

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Shared Azure AI Foundry Infrastructure (in core-rg)
# Updated to use AIServices (Azure AI Foundry) instead of legacy OpenAI
module "shared_azure_ai" {
  source = "../../modules/shared/azure-ai"

  environment         = "prod"
  location            = var.location
  region_code         = local.region_code
  resource_group_name = azurerm_resource_group.main.name

  # Enable AI Foundry project for workload isolation
  enable_project = true # Uses AzAPI to enable allowProjectManagement on account

  # Production: Higher capacity for AI workloads
  # Model deployments - OpenAI models only
  # See: https://ai.azure.com/catalog/models for full catalog
  # Note: Claude/Anthropic models require Azure AI Foundry portal deployment (not Terraform)
  # Note: GPT-4.1/5.x models not yet available - add when released
  model_deployments = {
    # GPT-4o-mini - high capacity for production
    "gpt-4o-mini" = {
      model_name    = "gpt-4o-mini"
      model_version = "2024-07-18"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 100
    }
    # Embedding models for RAG / Vector Search (reduces tokens ~20x)
    # Note: Must use GlobalStandard - Standard not available in SAN
    "text-embedding-3-large" = {
      model_name    = "text-embedding-3-large"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 240 # Higher capacity for production
    }
    "text-embedding-3-small" = {
      model_name    = "text-embedding-3-small"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 240 # Higher capacity for production
    }
  }

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Shared Azure AI Search Infrastructure (in core-rg)
# Provides RAG, vector search, and semantic search capabilities
module "shared_azure_search" {
  source = "../../modules/shared/azure-search"

  environment         = "prod"
  location            = var.location
  region_code         = local.region_code
  resource_group_name = azurerm_resource_group.main.name

  # Use standard tier for production (semantic search, higher capacity)
  sku                 = "standard"
  replica_count       = 2  # High availability
  partition_count     = 1
  semantic_search_sku = "standard" # Standard semantic search for production

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Story-Generator Infrastructure (in story-rg per ADR-0017)
# Supports both API (Kubernetes) and Web (Static Web App) components
module "story_generator" {
  source = "../../modules/story-generator"

  environment         = "prod"
  location            = var.location
  region_code         = "san"
  resource_group_name = azurerm_resource_group.story.name
  vnet_id             = azurerm_virtual_network.main.id
  subnet_id           = azurerm_subnet.story_generator.id

  # Use shared resources
  use_shared_postgresql             = true
  use_shared_redis                  = true
  use_shared_log_analytics          = true
  shared_postgresql_server_id       = module.shared_postgresql.server_id
  shared_redis_cache_id             = module.shared_redis.cache_id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  # Static Web App (Blazor WASM frontend) - same pattern as Mystira.App
  enable_static_web_app    = true
  static_web_app_sku       = "Standard"  # Standard tier for production
  fallback_location        = "eastus2"   # SWA not available in South Africa North
  github_repository_url    = "https://github.com/phoenixvc/Mystira.StoryGenerator"
  github_branch            = "main"
  enable_swa_custom_domain = false  # Enable after DNS is configured
  swa_custom_domain        = "story.mystira.app"

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Admin API Infrastructure (in admin-rg per ADR-0017)
module "admin_api" {
  source = "../../modules/admin-api"

  environment                       = "prod"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.admin.name
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.admin_api.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
  shared_postgresql_server_id       = module.shared_postgresql.server_id

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Entra ID Authentication
module "entra_id" {
  source = "../../modules/entra-id"

  environment = "prod"

  admin_ui_redirect_uris = [
    "https://admin.mystira.app/auth/callback"
  ]

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Microsoft Entra External ID Consumer Authentication
# Note: External ID tenant must be created manually first, then set external_id_tenant_id variable
module "entra_external_id" {
  source = "../../modules/entra-external-id"
  count  = var.external_id_tenant_id != "" ? 1 : 0

  environment   = "prod"
  tenant_id     = var.external_id_tenant_id
  tenant_name   = "mystira"

  pwa_redirect_uris = [
    "https://app.mystira.app/auth/callback"
  ]
}

# Shared Identity and RBAC Configuration
module "identity" {
  source = "../../modules/shared/identity"

  resource_group_name = azurerm_resource_group.main.name

  # AKS to ACR access (uses shared ACR from dev environment)
  enable_aks_acr_pull = true
  aks_principal_id    = azurerm_kubernetes_cluster.main.identity[0].principal_id
  acr_id              = data.azurerm_container_registry.shared.id

  # Service identity configurations (with explicit boolean flags for RBAC)
  service_identities = {
    "story-generator" = {
      principal_id               = module.story_generator.identity_principal_id
      enable_key_vault_access    = true
      key_vault_id               = module.story_generator.key_vault_id
      enable_postgres_access     = true
      postgres_server_id         = module.shared_postgresql.server_id
      postgres_role              = "reader"
      enable_redis_access        = true
      redis_cache_id             = module.shared_redis.cache_id
      enable_log_analytics       = true
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
    "publisher" = {
      principal_id               = module.publisher.identity_principal_id
      enable_key_vault_access    = true
      key_vault_id               = module.publisher.key_vault_id
      enable_log_analytics       = true
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
      enable_servicebus_sender   = true
      enable_servicebus_receiver = true
      servicebus_namespace_id    = module.shared_servicebus.namespace_id
    }
    "chain" = {
      principal_id               = module.chain.identity_principal_id
      enable_key_vault_access    = true
      key_vault_id               = module.chain.key_vault_id
      enable_log_analytics       = true
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
    "admin-api" = {
      principal_id               = module.admin_api.identity_principal_id
      enable_key_vault_access    = true
      key_vault_id               = module.admin_api.key_vault_id
      enable_postgres_access     = true
      postgres_server_id         = module.shared_postgresql.server_id
      postgres_role              = "reader"
      enable_log_analytics       = true
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
  }

  # Workload identity for AKS pods
  workload_identities = {
    "story-generator" = {
      identity_id         = module.story_generator.identity_id
      resource_group_name = azurerm_resource_group.story.name
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "story-generator-sa"
    }
    "publisher" = {
      identity_id         = module.publisher.identity_id
      resource_group_name = azurerm_resource_group.publisher.name
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "publisher-sa"
    }
    "chain" = {
      identity_id         = module.chain.identity_id
      resource_group_name = azurerm_resource_group.chain.name
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "chain-sa"
    }
    "admin-api" = {
      identity_id         = module.admin_api.identity_id
      resource_group_name = azurerm_resource_group.admin.name
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "admin-api-sa"
    }
  }

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }

  depends_on = [
    azurerm_kubernetes_cluster.main,
    module.story_generator,
    module.publisher,
    module.chain,
    module.admin_api
  ]
}

# =============================================================================
# Auto-Populated Key Vault Secrets (from Shared Resources)
# These secrets are automatically created from shared infrastructure outputs
# Manual secrets (API keys, external credentials) must be added via CI/CD
# =============================================================================

# Story-Generator Key Vault Secrets
resource "azurerm_key_vault_secret" "story_postgres" {
  name         = "postgres-connection-string"
  value        = "Host=${module.shared_postgresql.server_fqdn};Port=5432;Database=storygenerator;Username=${module.shared_postgresql.admin_login};Password=${module.shared_postgresql.admin_password};SSL Mode=Require;Trust Server Certificate=true"
  key_vault_id = module.story_generator.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-postgresql" }
}

resource "azurerm_key_vault_secret" "story_redis" {
  name         = "redis-connection-string"
  value        = module.shared_redis.primary_connection_string
  key_vault_id = module.story_generator.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-redis" }
}

resource "azurerm_key_vault_secret" "story_appinsights" {
  name         = "appinsights-connection-string"
  value        = module.shared_monitoring.application_insights_connection_string
  key_vault_id = module.story_generator.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-monitoring" }
}

# Story-Generator Azure AI Foundry Secrets (auto-populated from shared_azure_ai module)
resource "azurerm_key_vault_secret" "story_azure_ai_endpoint" {
  name         = "azure-ai-endpoint"
  value        = module.shared_azure_ai.endpoint
  key_vault_id = module.story_generator.key_vault_id
  content_type = "azure-ai"
  tags         = { AutoPopulated = "true", Source = "shared-azure-ai" }
}

resource "azurerm_key_vault_secret" "story_azure_ai_key" {
  name         = "azure-ai-api-key"
  value        = module.shared_azure_ai.primary_access_key
  key_vault_id = module.story_generator.key_vault_id
  content_type = "azure-ai"
  tags         = { AutoPopulated = "true", Source = "shared-azure-ai" }
}

# Publisher Key Vault Secrets
resource "azurerm_key_vault_secret" "publisher_servicebus" {
  name         = "servicebus-connection-string"
  value        = module.shared_servicebus.default_primary_connection_string
  key_vault_id = module.publisher.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-servicebus" }
}

resource "azurerm_key_vault_secret" "publisher_appinsights" {
  name         = "appinsights-connection-string"
  value        = module.shared_monitoring.application_insights_connection_string
  key_vault_id = module.publisher.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-monitoring" }
}

# Admin-API Key Vault Secrets
resource "azurerm_key_vault_secret" "admin_postgres" {
  name         = "postgres-connection-string"
  value        = "Host=${module.shared_postgresql.server_fqdn};Port=5432;Database=adminapi;Username=${module.shared_postgresql.admin_login};Password=${module.shared_postgresql.admin_password};SSL Mode=Require;Trust Server Certificate=true"
  key_vault_id = module.admin_api.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-postgresql" }
}

resource "azurerm_key_vault_secret" "admin_redis" {
  name         = "redis-connection-string"
  value        = module.shared_redis.primary_connection_string
  key_vault_id = module.admin_api.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-redis" }
}

resource "azurerm_key_vault_secret" "admin_appinsights" {
  name         = "appinsights-connection-string"
  value        = module.shared_monitoring.application_insights_connection_string
  key_vault_id = module.admin_api.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-monitoring" }
}

# Chain Key Vault Secrets
resource "azurerm_key_vault_secret" "chain_appinsights" {
  name         = "appinsights-connection-string"
  value        = module.shared_monitoring.application_insights_connection_string
  key_vault_id = module.chain.key_vault_id
  content_type = "connection-string"
  tags         = { AutoPopulated = "true", Source = "shared-monitoring" }
}

# Admin-API Entra ID Secrets (auto-populated from entra_id module)
resource "azurerm_key_vault_secret" "admin_entra_tenant_id" {
  name         = "azure-ad-tenant-id"
  value        = module.entra_id.tenant_id
  key_vault_id = module.admin_api.key_vault_id
  content_type = "azure-ad"
  tags         = { AutoPopulated = "true", Source = "entra-id-module" }
}

resource "azurerm_key_vault_secret" "admin_entra_client_id" {
  name         = "azure-ad-client-id"
  value        = module.entra_id.admin_api_client_id
  key_vault_id = module.admin_api.key_vault_id
  content_type = "azure-ad"
  tags         = { AutoPopulated = "true", Source = "entra-id-module" }
}

resource "azurerm_key_vault_secret" "admin_entra_ui_client_id" {
  name         = "admin-ui-client-id"
  value        = module.entra_id.admin_ui_client_id
  key_vault_id = module.admin_api.key_vault_id
  content_type = "azure-ad"
  tags         = { AutoPopulated = "true", Source = "entra-id-module" }
}

# Reference to shared ACR (in shared-acr-rg per ADR-0017)
data "azurerm_container_registry" "shared" {
  name                = "myssharedacr"
  resource_group_name = "mys-shared-acr-rg-san"
}

# AKS Cluster for Production
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mys-prod-core-aks-san"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mys-prod-core"
  kubernetes_version  = "1.28"

  default_node_pool {
    name                = "system"
    node_count          = 3
    vm_size             = "Standard_D4s_v3"
    vnet_subnet_id      = azurerm_subnet.aks.id
    enable_auto_scaling = true
    min_count           = 3
    max_count           = 10
    zones               = ["1", "2", "3"]
  }

  identity {
    type = "SystemAssigned"
  }

  # Enable OIDC issuer for workload identity
  oidc_issuer_enabled       = var.oidc_issuer_enabled
  workload_identity_enabled = var.workload_identity_enabled

  network_profile {
    network_plugin = "azure"
    network_policy = "calico"
    # Use a non-overlapping CIDR for Kubernetes services (VNet is 10.0.0.0/16)
    service_cidr   = "172.16.0.0/16"
    dns_service_ip = "172.16.0.10"
  }

  azure_active_directory_role_based_access_control {
    managed            = true
    azure_rbac_enabled = true
  }

  tags = {
    Environment = "prod"
    Project     = "Mystira"
    Critical    = "true"
  }
}

# Additional node pool for chain workloads
resource "azurerm_kubernetes_cluster_node_pool" "chain" {
  name                  = "chain"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = "Standard_D4s_v3"
  node_count            = 3
  vnet_subnet_id        = azurerm_subnet.aks.id
  enable_auto_scaling   = true
  min_count             = 3
  max_count             = 6
  zones                 = ["1", "2", "3"]

  node_labels = {
    "workload" = "chain"
  }

  node_taints = [
    "workload=chain:NoSchedule"
  ]

  tags = {
    Environment = "prod"
    Workload    = "chain"
  }
}

# Additional node pool for publisher workloads
resource "azurerm_kubernetes_cluster_node_pool" "publisher" {
  name                  = "publisher"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = "Standard_D2s_v3"
  node_count            = 3
  vnet_subnet_id        = azurerm_subnet.aks.id
  enable_auto_scaling   = true
  min_count             = 3
  max_count             = 10
  zones                 = ["1", "2", "3"]

  node_labels = {
    "workload" = "publisher"
  }

  tags = {
    Environment = "prod"
    Workload    = "publisher"
  }
}

# DNS Configuration
module "dns" {
  source = "../../modules/dns"

  environment         = "prod"
  resource_group_name = azurerm_resource_group.main.name
  domain_name         = "mystira.app"

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "aks_cluster_name" {
  value = azurerm_kubernetes_cluster.main.name
}

output "chain_nsg_id" {
  value = module.chain.nsg_id
}

output "publisher_nsg_id" {
  value = module.publisher.nsg_id
}

# ACR outputs - using shared ACR (per ADR-0017)
output "acr_login_server" {
  description = "Shared ACR login server"
  value       = data.azurerm_container_registry.shared.login_server
}

output "acr_name" {
  description = "Shared ACR name"
  value       = data.azurerm_container_registry.shared.name
}

output "dns_name_servers" {
  description = "Name servers for DNS zone - configure these in your domain registrar"
  value       = module.dns.name_servers
}

output "publisher_domain" {
  description = "Publisher service domain"
  value       = module.dns.publisher_fqdn
}

output "chain_domain" {
  description = "Chain service domain"
  value       = module.dns.chain_fqdn
}

# Shared Infrastructure Outputs
output "shared_postgresql_server_id" {
  description = "Shared PostgreSQL server ID"
  value       = module.shared_postgresql.server_id
}

output "shared_postgresql_server_fqdn" {
  description = "Shared PostgreSQL server FQDN"
  value       = module.shared_postgresql.server_fqdn
}

output "shared_redis_cache_id" {
  description = "Shared Redis cache ID"
  value       = module.shared_redis.cache_id
}

output "shared_redis_hostname" {
  description = "Shared Redis cache hostname"
  value       = module.shared_redis.hostname
}

output "shared_log_analytics_workspace_id" {
  description = "Shared Log Analytics workspace ID"
  value       = module.shared_monitoring.log_analytics_workspace_id
}

output "story_generator_postgresql_database_name" {
  description = "Story-Generator PostgreSQL database name"
  value       = module.story_generator.postgresql_database_name
}

output "story_generator_swa_url" {
  description = "Story-Generator Static Web App URL"
  value       = module.story_generator.static_web_app_url
}

output "story_generator_swa_api_key" {
  description = "Story-Generator Static Web App API key (for deployments)"
  value       = module.story_generator.static_web_app_api_key
  sensitive   = true
}

output "story_generator_swa_default_hostname" {
  description = "Story-Generator Static Web App default hostname"
  value       = module.story_generator.static_web_app_default_hostname
}

# Entra ID Authentication Outputs
output "entra_admin_api_client_id" {
  description = "Admin API application (client) ID"
  value       = module.entra_id.admin_api_client_id
}

output "entra_admin_ui_client_id" {
  description = "Admin UI application (client) ID"
  value       = module.entra_id.admin_ui_client_id
}

output "entra_tenant_id" {
  description = "Azure AD tenant ID"
  value       = module.entra_id.tenant_id
}

# Identity and RBAC Outputs
output "aks_oidc_issuer_url" {
  description = "OIDC issuer URL for AKS workload identity"
  value       = azurerm_kubernetes_cluster.main.oidc_issuer_url
}

output "identity_aks_acr_role_assignment" {
  description = "AKS to ACR role assignment ID"
  value       = module.identity.aks_acr_role_assignment_id
}

# Admin API Outputs
output "admin_api_identity_id" {
  description = "Admin API managed identity ID"
  value       = module.admin_api.identity_id
}

output "admin_api_identity_client_id" {
  description = "Admin API managed identity client ID (for workload identity)"
  value       = module.admin_api.identity_client_id
}

output "admin_api_key_vault_id" {
  description = "Admin API Key Vault ID"
  value       = module.admin_api.key_vault_id
}

output "admin_api_key_vault_uri" {
  description = "Admin API Key Vault URI"
  value       = module.admin_api.key_vault_uri
}

# PostgreSQL Azure AD connection string template
output "postgresql_aad_connection_template" {
  description = "PostgreSQL Azure AD connection string template"
  value       = module.shared_postgresql.aad_connection_string_template
}

# =============================================================================
# Resource Group Outputs (ADR-0017)
# =============================================================================

output "resource_group_chain" {
  description = "Chain service resource group name"
  value       = azurerm_resource_group.chain.name
}

output "resource_group_publisher" {
  description = "Publisher service resource group name"
  value       = azurerm_resource_group.publisher.name
}

output "resource_group_story" {
  description = "Story-Generator service resource group name"
  value       = azurerm_resource_group.story.name
}

output "resource_group_admin" {
  description = "Admin API service resource group name"
  value       = azurerm_resource_group.admin.name
}

output "resource_group_app" {
  description = "App (PWA/API) service resource group name"
  value       = azurerm_resource_group.app.name
}

# =============================================================================
# Shared Service Bus Outputs
# =============================================================================

output "servicebus_namespace_id" {
  description = "Shared Service Bus namespace ID"
  value       = module.shared_servicebus.namespace_id
}

output "servicebus_namespace_name" {
  description = "Shared Service Bus namespace name"
  value       = module.shared_servicebus.namespace_name
}

output "servicebus_connection_string" {
  description = "Shared Service Bus primary connection string"
  value       = module.shared_servicebus.default_primary_connection_string
  sensitive   = true
}
