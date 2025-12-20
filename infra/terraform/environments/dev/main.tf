# Mystira Development Environment - Azure
# Terraform configuration for dev environment

terraform {
  required_version = ">= 1.5.0"

  backend "azurerm" {
    resource_group_name  = "mys-prod-terraform-rg-eus"
    storage_account_name = "mysprodterraformstate"
    container_name       = "tfstate"
    key                  = "dev/terraform.tfstate"
    use_azuread_auth     = true
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
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

# Common tags for all resources
locals {
  common_tags = {
    Environment = "dev"
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "mys-dev-mystira-rg-san"
  location = var.location

  tags = {
    Environment = "dev"
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "mys-dev-mystira-vnet-san"
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

# Shared Azure Container Registry
# Note: This ACR is shared across all environments (dev, staging, prod)
# to align with the CI/CD workflows which expect a single registry named 'mystiraacr'.
# Consider moving to a separate shared infrastructure workspace in the future.
resource "azurerm_container_registry" "shared" {
  name                = "mysprodacr"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Standard"
  admin_enabled       = false

  tags = {
    Environment = "dev"
    Project     = "Mystira"
    ManagedBy   = "terraform"
    Shared      = "all-environments"
  }
}

# Chain Infrastructure
module "chain" {
  source = "../../modules/chain"

  environment           = "dev"
  location              = var.location
  region_code           = "san"
  resource_group_name   = azurerm_resource_group.main.name
  chain_node_count      = 1
  chain_vm_size         = "Standard_B2s"
  chain_storage_size_gb = 50
  vnet_id               = azurerm_virtual_network.main.id
  subnet_id             = azurerm_subnet.chain.id

  tags = {
    CostCenter = "development"
  }
}

# Publisher Infrastructure
module "publisher" {
  source = "../../modules/publisher"

  environment             = "dev"
  location                = var.location
  region_code             = "san"
  resource_group_name     = azurerm_resource_group.main.name
  publisher_replica_count = 1
  vnet_id                 = azurerm_virtual_network.main.id
  subnet_id               = azurerm_subnet.publisher.id
  chain_rpc_endpoint      = "http://mys-chain.mys-dev.svc.cluster.local:8545"

  tags = {
    CostCenter = "development"
  }
}

# Shared PostgreSQL Infrastructure
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  environment         = "dev"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  vnet_id             = azurerm_virtual_network.main.id
  subnet_id           = azurerm_subnet.postgresql.id

  databases = [
    "storygenerator",
    "publisher"
  ]

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

  retention_in_days = 30

  tags = {
    CostCenter = "development"
  }
}

# Story-Generator Infrastructure
module "story_generator" {
  source = "../../modules/story-generator"

  environment         = "dev"
  location            = var.location
  region_code         = "san"
  resource_group_name = azurerm_resource_group.main.name
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

# AKS Cluster for Dev
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mys-dev-mystira-aks-san"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mys-dev-mystira"

  default_node_pool {
    name           = "default"
    node_count     = 2
    vm_size        = "Standard_B2s"
    vnet_subnet_id = azurerm_subnet.aks.id
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin = "azure"
    network_policy = "calico"
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
  value = azurerm_container_registry.shared.login_server
}

output "acr_name" {
  value = azurerm_container_registry.shared.name
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
