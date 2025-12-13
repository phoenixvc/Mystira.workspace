# Mystira Development Environment - Azure
# Terraform configuration for dev environment

terraform {
  required_version = ">= 1.5.0"

  backend "azurerm" {
    resource_group_name  = "mystira-terraform-state"
    storage_account_name = "mystiraterraformstate"
    container_name       = "tfstate"
    key                  = "dev/terraform.tfstate"
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
  default     = "eastus"
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "mystira-dev-rg"
  location = var.location

  tags = {
    Environment = "dev"
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "mystira-dev-vnet"
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
  address_prefixes     = ["10.0.10.0/22"]
}

# Shared Azure Container Registry
resource "azurerm_container_registry" "shared" {
  name                = "mystiraacr"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Standard"
  admin_enabled       = false

  tags = {
    Environment = "dev"
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
}

# Chain Infrastructure
module "chain" {
  source = "../../modules/chain"

  environment           = "dev"
  location              = var.location
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
  resource_group_name     = azurerm_resource_group.main.name
  publisher_replica_count = 1
  vnet_id                 = azurerm_virtual_network.main.id
  subnet_id               = azurerm_subnet.publisher.id
  chain_rpc_endpoint      = "http://mystira-chain.dev.mystira.internal:8545"

  tags = {
    CostCenter = "development"
  }
}

# AKS Cluster for Dev
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mystira-dev-aks"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mystira-dev"

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
