# Mystira Production Environment - Azure
# Terraform configuration for production environment

terraform {
  required_version = ">= 1.5.0"

  backend "azurerm" {
    resource_group_name  = "mystira-terraform-state"
    storage_account_name = "mystiraterraformstate"
    container_name       = "tfstate"
    key                  = "prod/terraform.tfstate"
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
      purge_soft_delete_on_destroy = false
    }
  }
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "eastus"
}

variable "location_secondary" {
  description = "Secondary Azure region for DR"
  type        = string
  default     = "westus2"
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "mystira-prod-rg"
  location = var.location

  tags = {
    Environment = "prod"
    Project     = "Mystira"
    ManagedBy   = "terraform"
    Critical    = "true"
  }
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "mystira-prod-vnet"
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

# Chain Infrastructure
module "chain" {
  source = "../../modules/chain"

  environment           = "prod"
  location              = var.location
  resource_group_name   = azurerm_resource_group.main.name
  chain_node_count      = 3
  chain_vm_size         = "Standard_D4s_v3"
  chain_storage_size_gb = 500
  vnet_id               = azurerm_virtual_network.main.id
  subnet_id             = azurerm_subnet.chain.id

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# Publisher Infrastructure
module "publisher" {
  source = "../../modules/publisher"

  environment             = "prod"
  location                = var.location
  resource_group_name     = azurerm_resource_group.main.name
  publisher_replica_count = 3
  vnet_id                 = azurerm_virtual_network.main.id
  subnet_id               = azurerm_subnet.publisher.id
  chain_rpc_endpoint      = "http://mystira-chain.prod.mystira.internal:8545"

  tags = {
    CostCenter = "production"
    Critical   = "true"
  }
}

# AKS Cluster for Production
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mystira-prod-aks"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mystira-prod"
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
