# Shared PostgreSQL Infrastructure Module

Terraform module for deploying shared PostgreSQL database infrastructure on Azure.

## Features

- PostgreSQL Flexible Server
- Private DNS zone and VNet integration
- Configurable databases
- Firewall rules for Azure services
- Backup and retention configuration
- **Azure AD authentication** for passwordless access via managed identities

## Usage

```hcl
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mystira-dev-rg"
  vnet_id            = azurerm_virtual_network.main.id
  subnet_id          = azurerm_subnet.postgresql.id

  databases = [
    "storygenerator",
    "app",
    "publisher"
  ]

  tags = {
    Environment = "dev"
    Project     = "Mystira"
  }
}
```

## Inputs

| Name                         | Description                                    | Type           | Default           | Required |
| ---------------------------- | ---------------------------------------------- | -------------- | ----------------- | -------- |
| environment                  | Deployment environment                         | `string`       | -                 | yes      |
| location                     | Azure region                                   | `string`       | `"eastus"`        | no       |
| resource_group_name          | Resource group name                            | `string`       | -                 | yes      |
| vnet_id                      | Virtual Network ID                             | `string`       | -                 | yes      |
| subnet_id                    | Subnet ID                                      | `string`       | -                 | yes      |
| admin_login                  | PostgreSQL administrator login                 | `string`       | `"mystira_admin"` | no       |
| admin_password               | PostgreSQL administrator password              | `string`       | `null`            | no       |
| postgres_version             | PostgreSQL version                             | `string`       | `"15"`            | no       |
| sku_name                     | PostgreSQL SKU name                            | `string`       | `null`            | no       |
| storage_mb                   | Storage size in MB                             | `number`       | `32768`           | no       |
| backup_retention_days        | Backup retention in days                       | `number`       | `7`               | no       |
| geo_redundant_backup_enabled | Enable geo-redundant backup                    | `bool`         | `false`           | no       |
| databases                    | List of database names to create               | `list(string)` | `[]`              | no       |
| aad_auth_enabled             | Enable Azure AD authentication                 | `bool`         | `false`           | no       |
| aad_admin_identities         | Map of managed identities as AD administrators | `map(object)`  | `{}`              | no       |
| tags                         | Resource tags                                  | `map(string)`  | `{}`              | no       |

## Outputs

| Name                        | Description                                        |
| --------------------------- | -------------------------------------------------- |
| server_id                   | PostgreSQL server ID                               |
| server_fqdn                 | PostgreSQL server FQDN                             |
| admin_login                 | PostgreSQL administrator login                     |
| admin_password              | PostgreSQL administrator password                  |
| private_dns_zone_id         | Private DNS Zone ID                                |
| connection_strings          | Map of database names to connection strings        |
| aad_connection_string_template | Connection string template for Azure AD auth    |
| aad_admins                  | Map of Azure AD administrators configured          |

## Database Management

The module creates databases specified in the `databases` variable. Each service (Story-Generator, App, Publisher) can use a dedicated database on the shared server, providing:

- Cost efficiency
- Centralized backup management
- Simplified network configuration
- Unified monitoring

## Azure AD Authentication

Enable passwordless authentication using managed identities instead of storing database credentials.

### Configuration

```hcl
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  # ... other config ...

  # Enable Azure AD authentication
  aad_auth_enabled = true
  aad_admin_identities = {
    "admin-api" = {
      principal_id   = module.admin_api.identity_principal_id
      principal_name = "mys-dev-admin-api-identity-san"
      principal_type = "ServicePrincipal"
    }
    "story-generator" = {
      principal_id   = module.story_generator.identity_principal_id
      principal_name = "mys-dev-story-identity-san"
      principal_type = "ServicePrincipal"
    }
  }
}
```

### Connection String

Use this format for Azure AD authentication (no password needed):

```
Host=<server>.postgres.database.azure.com;Database=<database>;Username=<identity-name>;Ssl Mode=Require
```

### .NET Application Configuration

```csharp
// Install: Npgsql.EntityFrameworkCore.PostgreSQL
// Install: Azure.Identity

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseAzureADAuthentication(new DefaultAzureCredential());
options.UseNpgsql(dataSourceBuilder.Build());
```

### How It Works

1. Terraform creates the managed identity as a PostgreSQL AD administrator
2. The AKS pod runs with workload identity linked to the managed identity
3. Azure SDK (DefaultAzureCredential) automatically obtains an Azure AD token
4. Npgsql uses the token to authenticate to PostgreSQL

