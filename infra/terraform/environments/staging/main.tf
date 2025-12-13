# Mystira Staging Environment - Azure
# Terraform configuration for staging environment

terraform {
  required_version = ">= 1.5.0"

  backend "azurerm" {
    resource_group_name  = "mystira-terraform-state"
    storage_account_name = "mystiraterraformstate"
    container_name       = "tfstate"
    key                  = "staging/terraform.tfstate"
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

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "mystira-staging-rg"
  location = var.location

  tags = {
    Environment = "staging"
    Project     = "Mystira"
    ManagedBy   = "terraform"
  }
}

# Virtual Network
resource "azurerm_virtual_network" "main" {
  name                = "mystira-staging-vnet"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = ["10.1.0.0/16"]

  tags = {
    Environment = "staging"
  }
}

resource "azurerm_subnet" "chain" {
  name                 = "chain-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.1.1.0/24"]
}

resource "azurerm_subnet" "publisher" {
  name                 = "publisher-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.1.2.0/24"]
}

resource "azurerm_subnet" "aks" {
  name                 = "aks-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.1.10.0/22"]
}

# Chain Infrastructure
module "chain" {
  source = "../../modules/chain"

  environment           = "staging"
  location              = var.location
  resource_group_name   = azurerm_resource_group.main.name
  chain_node_count      = 2
  chain_vm_size         = "Standard_D2s_v3"
  chain_storage_size_gb = 100
  vnet_id               = azurerm_virtual_network.main.id
  subnet_id             = azurerm_subnet.chain.id

  tags = {
    CostCenter = "staging"
  }
}

# Publisher Infrastructure
module "publisher" {
  source = "../../modules/publisher"

  environment             = "staging"
  location                = var.location
  resource_group_name     = azurerm_resource_group.main.name
  publisher_replica_count = 2
  vnet_id                 = azurerm_virtual_network.main.id
  subnet_id               = azurerm_subnet.publisher.id
  chain_rpc_endpoint      = "http://mystira-chain.staging.mystira.internal:8545"

  tags = {
    CostCenter = "staging"
  }
}

# AKS Cluster for Staging
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mystira-staging-aks"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mystira-staging"

  default_node_pool {
    name           = "default"
    node_count     = 3
    vm_size        = "Standard_D2s_v3"
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
    Environment = "staging"
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
