# Shared Cosmos DB Infrastructure Module - Azure
# Terraform module for deploying shared Cosmos DB infrastructure
# Used by: Mystira.App, Admin API, future services

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "serverless" {
  description = "Enable serverless capacity mode (cost-effective for dev/staging)"
  type        = bool
  default     = true
}

variable "consistency_level" {
  description = "Cosmos DB consistency level"
  type        = string
  default     = "Session"

  validation {
    condition     = contains(["BoundedStaleness", "ConsistentPrefix", "Eventual", "Session", "Strong"], var.consistency_level)
    error_message = "Consistency level must be BoundedStaleness, ConsistentPrefix, Eventual, Session, or Strong."
  }
}

variable "databases" {
  description = "Map of databases to create with their containers"
  type = map(object({
    containers = list(object({
      name          = string
      partition_key = string
    }))
  }))
  default = {
    "MystiraAppDb" = {
      containers = [
        { name = "UserProfiles", partition_key = "/id" },
        { name = "Accounts", partition_key = "/id" },
        { name = "Scenarios", partition_key = "/id" },
        { name = "GameSessions", partition_key = "/accountId" },
        { name = "ContentBundles", partition_key = "/id" },
        { name = "PendingSignups", partition_key = "/email" },
        { name = "CompassTrackings", partition_key = "/id" },
      ]
    }
  }
}

variable "enable_free_tier" {
  description = "Enable free tier (only one per subscription)"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  region_code = lookup({
    "southafricanorth" = "san"
    "eastus2"          = "eus2"
    "westeurope"       = "weu"
    "northeurope"      = "neu"
  }, var.location, substr(var.location, 0, 4))

  name_prefix = "mys-${var.environment}-core"

  common_tags = merge(var.tags, {
    Component   = "shared-cosmos-db"
    Environment = var.environment
    Service     = "core"
    ManagedBy   = "terraform"
    Project     = "Mystira"
    SharedBy    = "app,admin"
  })

  # Flatten containers for for_each
  containers = flatten([
    for db_name, db in var.databases : [
      for container in db.containers : {
        key           = "${db_name}/${container.name}"
        database_name = db_name
        name          = container.name
        partition_key = container.partition_key
      }
    ]
  ])
}

# Cosmos DB Account
resource "azurerm_cosmosdb_account" "shared" {
  name                = "${local.name_prefix}-cosmos-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  # Free tier (only one per subscription)
  dynamic "capabilities" {
    for_each = var.enable_free_tier ? [1] : []
    content {
      name = "EnableFreeTier"
    }
  }

  # Serverless capacity mode (recommended for dev/staging)
  dynamic "capabilities" {
    for_each = var.serverless ? [1] : []
    content {
      name = "EnableServerless"
    }
  }

  consistency_policy {
    consistency_level = var.consistency_level
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  tags = local.common_tags
}

# Cosmos DB SQL Databases
resource "azurerm_cosmosdb_sql_database" "databases" {
  for_each = var.databases

  name                = each.key
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.shared.name
}

# Cosmos DB SQL Containers
resource "azurerm_cosmosdb_sql_container" "containers" {
  for_each = { for c in local.containers : c.key => c }

  name                = each.value.name
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.shared.name
  database_name       = azurerm_cosmosdb_sql_database.databases[each.value.database_name].name
  partition_key_paths = [each.value.partition_key]

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }
  }
}

# Outputs
output "account_id" {
  description = "Cosmos DB account ID"
  value       = azurerm_cosmosdb_account.shared.id
}

output "account_name" {
  description = "Cosmos DB account name"
  value       = azurerm_cosmosdb_account.shared.name
}

output "endpoint" {
  description = "Cosmos DB endpoint URL"
  value       = azurerm_cosmosdb_account.shared.endpoint
}

output "primary_key" {
  description = "Cosmos DB primary key"
  value       = azurerm_cosmosdb_account.shared.primary_key
  sensitive   = true
}

output "primary_sql_connection_string" {
  description = "Cosmos DB primary SQL connection string"
  value       = azurerm_cosmosdb_account.shared.primary_sql_connection_string
  sensitive   = true
}

output "secondary_sql_connection_string" {
  description = "Cosmos DB secondary SQL connection string"
  value       = azurerm_cosmosdb_account.shared.secondary_sql_connection_string
  sensitive   = true
}

output "database_names" {
  description = "List of database names"
  value       = [for db in azurerm_cosmosdb_sql_database.databases : db.name]
}

output "database_ids" {
  description = "Map of database names to IDs"
  value       = { for name, db in azurerm_cosmosdb_sql_database.databases : name => db.id }
}
