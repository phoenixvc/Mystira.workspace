# Mystira Development Environment - Azure
# Terraform configuration for dev environment

terraform {
  required_version = ">= 1.5.0"

  backend "azurerm" {
    resource_group_name  = "mys-shared-terraform-rg-san"
    storage_account_name = "myssharedtfstatesan"
    container_name       = "tfstate"
    key                  = "dev/terraform.tfstate"
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
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
  }
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "southafricanorth"
}

variable "b2c_tenant_id" {
  description = "Azure AD B2C tenant ID (optional - set when B2C tenant is created)"
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
    Environment = "dev"
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
  region_code = "san"
}

# =============================================================================
# Resource Groups (ADR-0017: Resource Group Organization Strategy)
# =============================================================================

# Tier 1: Core Resource Group (shared infrastructure)
resource "azurerm_resource_group" "main" {
  name     = "mys-dev-core-rg-san"
  location = var.location

  tags = {
    Environment = "dev"
    Project     = "Mystira"
    Service     = "core"
    ManagedBy   = "terraform"
  }
}

# Tier 2: Service-Specific Resource Groups
resource "azurerm_resource_group" "chain" {
  name     = "mys-dev-chain-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "chain" })
}

resource "azurerm_resource_group" "publisher" {
  name     = "mys-dev-publisher-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "publisher" })
}

resource "azurerm_resource_group" "story" {
  name     = "mys-dev-story-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "story-generator" })
}

resource "azurerm_resource_group" "admin" {
  name     = "mys-dev-admin-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "admin-api" })
}

resource "azurerm_resource_group" "app" {
  name     = "mys-dev-app-rg-${local.region_code}"
  location = var.location
  tags     = merge(local.common_tags, { Service = "app" })
}

# Tier 3: Cross-Environment Shared Resource Groups
# Note: These are created once (in dev) and referenced by other environments
resource "azurerm_resource_group" "shared_acr" {
  name     = "mys-shared-acr-rg-${local.region_code}"
  location = var.location
  tags = merge(local.common_tags, {
    Environment = "shared"
    Service     = "acr"
    Shared      = "all-environments"
  })
}

resource "azurerm_resource_group" "shared_comms" {
  name     = "mys-shared-comms-rg-glob"
  location = var.location
  tags = merge(local.common_tags, {
    Environment = "shared"
    Service     = "communications"
    Shared      = "all-environments"
  })
}

# =============================================================================
# Networking (in core-rg)
# =============================================================================

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "mys-dev-core-vnet-san"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = ["10.0.0.0/16"]

  tags = {
    Environment = "dev"
  }
}

resource "azurerm_subnet" "chain" {
  name                 = "chain-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_subnet" "publisher" {
  name                 = "publisher-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]
}

resource "azurerm_subnet" "aks" {
  name                 = "aks-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.8.0/22"]
}

resource "azurerm_subnet" "postgresql" {
  name                 = "postgresql-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.3.0/24"]

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
  address_prefixes     = ["10.0.4.0/24"]
  # Note: Azure Cache for Redis does not support subnet delegation
}

resource "azurerm_subnet" "story_generator" {
  name                 = "story-generator-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.5.0/24"]
}

resource "azurerm_subnet" "admin_api" {
  name                 = "admin-api-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.6.0/24"]
}

# =============================================================================
# Shared Container Registry (in shared-acr-rg, cross-environment)
# =============================================================================
module "shared_acr" {
  source = "../../modules/shared/container-registry"

  resource_group_name = azurerm_resource_group.shared_acr.name
  location            = azurerm_resource_group.shared_acr.location
  acr_name            = "myssharedacr"
  sku                 = "Standard"

  tags = local.common_tags
}

# =============================================================================
# Shared Communications (in shared-comms-rg, cross-environment)
# =============================================================================
module "shared_comms" {
  source = "../../modules/shared/communications"

  resource_group_name = azurerm_resource_group.shared_comms.name
  location            = azurerm_resource_group.shared_comms.location
  name_prefix         = "mys-shared"
  data_location       = "Africa"

  tags = local.common_tags
}

# Chain Infrastructure (in chain-rg per ADR-0017)
module "chain" {
  source = "../../modules/chain"

  environment                       = "dev"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.chain.name
  chain_node_count                  = 1
  chain_vm_size                     = "Standard_B2s"
  chain_storage_size_gb             = 100 # Premium file shares require minimum 100 GB
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.chain.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  tags = {
    CostCenter = "development"
  }
}

# Publisher Infrastructure (in publisher-rg per ADR-0017)
module "publisher" {
  source = "../../modules/publisher"

  environment                       = "dev"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.publisher.name
  publisher_replica_count           = 1
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.publisher.id
  chain_rpc_endpoint                = "http://mys-chain.mys-dev.svc.cluster.local:8545"
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  tags = {
    CostCenter = "development"
  }
}

# Shared PostgreSQL Infrastructure
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  environment             = "dev"
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

  # Azure AD authentication disabled - identities need to be created first
  # To enable: 1) Create user assigned identities for each service
  #            2) Set aad_auth_enabled = true
  #            3) Add identity entries to aad_admin_identities
  aad_auth_enabled = false
  # aad_admin_identities = {
  #   "admin-api" = {
  #     principal_name = "mys-dev-admin-api-identity-san"
  #     principal_type = "ServicePrincipal"
  #   }
  #   "story-generator" = {
  #     principal_name = "mys-dev-story-identity-san"
  #     principal_type = "ServicePrincipal"
  #   }
  # }

  tags = {
    CostCenter = "development"
  }
}

# Shared Redis Infrastructure
# Note: Standard SKU does not support VNet integration (subnet_id)
# Only Premium SKU supports subnet deployment
module "shared_redis" {
  source = "../../modules/shared/redis"

  environment         = "dev"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  # subnet_id omitted - Standard SKU doesn't support VNet integration

  capacity = 1
  family   = "C"
  sku_name = "Standard"

  tags = {
    CostCenter = "development"
  }
}

# Shared Monitoring Infrastructure
module "shared_monitoring" {
  source = "../../modules/shared/monitoring"

  environment         = "dev"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name

  retention_in_days       = 30
  alert_email_addresses   = var.alert_email_addresses

  tags = {
    CostCenter = "development"
  }
}

# Story-Generator Infrastructure (in story-rg per ADR-0017)
module "story_generator" {
  source = "../../modules/story-generator"

  environment         = "dev"
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

  tags = {
    CostCenter = "development"
  }
}

# Admin API Infrastructure (in admin-rg per ADR-0017)
module "admin_api" {
  source = "../../modules/admin-api"

  environment                       = "dev"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.admin.name
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.admin_api.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
  shared_postgresql_server_id       = module.shared_postgresql.server_id

  tags = {
    CostCenter = "development"
  }
}

# Entra ID Authentication
module "entra_id" {
  source = "../../modules/entra-id"

  environment = "dev"

  admin_ui_redirect_uris = [
    "http://localhost:7001/auth/callback",
    "http://localhost:3000/auth/callback",
    "https://admin.dev.mystira.app/auth/callback"
  ]

  tags = {
    CostCenter = "development"
  }
}

# Shared Identity and RBAC Configuration
module "identity" {
  source = "../../modules/shared/identity"

  resource_group_name = azurerm_resource_group.main.name

  # AKS to ACR access
  aks_principal_id = azurerm_kubernetes_cluster.main.identity[0].principal_id
  acr_id           = module.shared_acr.acr_id

  # Service identity configurations
  service_identities = {
    "story-generator" = {
      principal_id               = module.story_generator.identity_principal_id
      key_vault_id               = module.story_generator.key_vault_id
      postgres_server_id         = module.shared_postgresql.server_id
      redis_cache_id             = module.shared_redis.cache_id
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
    "publisher" = {
      principal_id               = module.publisher.identity_principal_id
      key_vault_id               = module.publisher.key_vault_id
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
    "chain" = {
      principal_id               = module.chain.identity_principal_id
      key_vault_id               = module.chain.key_vault_id
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
    "admin-api" = {
      principal_id               = module.admin_api.identity_principal_id
      key_vault_id               = module.admin_api.key_vault_id
      postgres_server_id         = module.shared_postgresql.server_id
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
  }

  # Workload identity for AKS pods
  workload_identities = {
    "story-generator" = {
      identity_id         = module.story_generator.identity_id
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "story-generator-sa"
    }
    "publisher" = {
      identity_id         = module.publisher.identity_id
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "publisher-sa"
    }
    "chain" = {
      identity_id         = module.chain.identity_id
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "chain-sa"
    }
    "admin-api" = {
      identity_id         = module.admin_api.identity_id
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "admin-api-sa"
    }
  }

  tags = {
    CostCenter = "development"
  }

  depends_on = [
    azurerm_kubernetes_cluster.main,
    module.story_generator,
    module.publisher,
    module.chain,
    module.admin_api
  ]
}

# Azure AD B2C Consumer Authentication
# Note: B2C tenant must be created manually first, then set b2c_tenant_id variable
module "azure_ad_b2c" {
  source = "../../modules/azure-ad-b2c"
  count  = var.b2c_tenant_id != "" ? 1 : 0

  environment     = "dev"
  b2c_tenant_id   = var.b2c_tenant_id
  b2c_tenant_name = "mystirab2cdev"

  pwa_redirect_uris = [
    "http://localhost:5173/auth/callback",
    "http://localhost:3000/auth/callback",
    "https://app.dev.mystira.app/auth/callback"
  ]
}

# AKS Cluster for Dev
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mys-dev-core-aks-san"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mys-dev-core"

  default_node_pool {
    name           = "default"
    node_count     = 2
    vm_size        = "Standard_B2s"
    vnet_subnet_id = azurerm_subnet.aks.id
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

  tags = {
    Environment = "dev"
    Project     = "Mystira"
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

output "acr_login_server" {
  value = module.shared_acr.acr_login_server
}

output "acr_name" {
  value = module.shared_acr.acr_name
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

output "story_generator_key_vault_id" {
  description = "Story-Generator Key Vault ID (for secret storage)"
  value       = module.story_generator.key_vault_id
}

# Connection string for Story-Generator (from shared PostgreSQL)
# Note: Password must be retrieved from Key Vault or shared_postgresql.admin_password output
# This is a template - replace <PASSWORD> with actual password
output "shared_postgresql_connection_string_storygenerator_template" {
  description = "PostgreSQL connection string template for storygenerator database (replace <PASSWORD> with actual password)"
  value       = "Host=${module.shared_postgresql.server_fqdn};Port=5432;Username=${module.shared_postgresql.admin_login};Password=<PASSWORD>;Database=storygenerator;SSLMode=Require;Trust Server Certificate=true"
  sensitive   = false
}

# Actual connection string (sensitive)
output "shared_postgresql_connection_string_storygenerator" {
  description = "PostgreSQL connection string for storygenerator database"
  value       = module.shared_postgresql.connection_strings["storygenerator"]
  sensitive   = true
}

output "shared_redis_connection_string" {
  description = "Redis connection string (from shared Redis module)"
  value       = module.shared_redis.primary_connection_string
  sensitive   = true
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

output "entra_admin_api_config" {
  description = "Configuration for Admin API (appsettings.json)"
  value       = module.entra_id.admin_api_config
}

output "entra_admin_ui_config" {
  description = "Configuration for Admin UI (.env)"
  value       = module.entra_id.admin_ui_config
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

output "resource_group_shared_acr" {
  description = "Shared ACR resource group name"
  value       = azurerm_resource_group.shared_acr.name
}

output "resource_group_shared_comms" {
  description = "Shared Communications resource group name"
  value       = azurerm_resource_group.shared_comms.name
}

# =============================================================================
# Shared Communications Outputs
# =============================================================================

output "communication_service_id" {
  description = "Azure Communication Service ID"
  value       = module.shared_comms.communication_service_id
}

output "communication_service_name" {
  description = "Azure Communication Service name"
  value       = module.shared_comms.communication_service_name
}

output "communication_service_connection_string" {
  description = "Azure Communication Service connection string"
  value       = module.shared_comms.communication_service_primary_connection_string
  sensitive   = true
}

output "email_service_id" {
  description = "Email Communication Service ID"
  value       = module.shared_comms.email_service_id
}
