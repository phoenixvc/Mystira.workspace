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
  }
}

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

# Resource Group
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

  delegation {
    name = "redis-delegation"
    service_delegation {
      name = "Microsoft.Cache/redis"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

resource "azurerm_subnet" "story_generator" {
  name                 = "story-generator-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.2.5.0/24"]
}

# Chain Infrastructure
module "chain" {
  source = "../../modules/chain"

  environment                       = "prod"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.main.name
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

# Publisher Infrastructure
module "publisher" {
  source = "../../modules/publisher"

  environment                       = "prod"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.main.name
  publisher_replica_count           = 3
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.publisher.id
  chain_rpc_endpoint                = "http://mys-chain.mys-prod.svc.cluster.local:8545"
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

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
    "publisher"
  ]

  sku_name                     = "GP_Standard_D4s_v3"
  storage_mb                   = 65536
  backup_retention_days        = 35
  geo_redundant_backup_enabled = true

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

# Shared Monitoring Infrastructure
module "shared_monitoring" {
  source = "../../modules/shared/monitoring"

  environment         = "prod"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name

  retention_in_days = 90

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Story-Generator Infrastructure
module "story_generator" {
  source = "../../modules/story-generator"

  environment         = "prod"
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
    CostCenter = "production"
    Critical   = "true"
  }
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

output "chain_acr_login_server" {
  value = module.chain.acr_login_server
}

output "publisher_acr_login_server" {
  value = module.publisher.acr_login_server
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
