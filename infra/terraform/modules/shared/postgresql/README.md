# Shared PostgreSQL Infrastructure Module

Terraform module for deploying shared PostgreSQL database infrastructure on Azure.

## Features

- PostgreSQL Flexible Server
- Private DNS zone and VNet integration
- Configurable databases
- Firewall rules for Azure services
- Backup and retention configuration

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

| Name                         | Description                       | Type           | Default           | Required |
| ---------------------------- | --------------------------------- | -------------- | ----------------- | -------- |
| environment                  | Deployment environment            | `string`       | -                 | yes      |
| location                     | Azure region                      | `string`       | `"eastus"`        | no       |
| resource_group_name          | Resource group name               | `string`       | -                 | yes      |
| vnet_id                      | Virtual Network ID                | `string`       | -                 | yes      |
| subnet_id                    | Subnet ID                         | `string`       | -                 | yes      |
| admin_login                  | PostgreSQL administrator login    | `string`       | `"mystira_admin"` | no       |
| admin_password               | PostgreSQL administrator password | `string`       | `null`            | no       |
| postgres_version             | PostgreSQL version                | `string`       | `"15"`            | no       |
| sku_name                     | PostgreSQL SKU name               | `string`       | `null`            | no       |
| storage_mb                   | Storage size in MB                | `number`       | `32768`           | no       |
| backup_retention_days        | Backup retention in days          | `number`       | `7`               | no       |
| geo_redundant_backup_enabled | Enable geo-redundant backup       | `bool`         | `false`           | no       |
| databases                    | List of database names to create  | `list(string)` | `[]`              | no       |
| tags                         | Resource tags                     | `map(string)`  | `{}`              | no       |

## Outputs

| Name                | Description                       |
| ------------------- | --------------------------------- |
| server_id           | PostgreSQL server ID              |
| server_fqdn         | PostgreSQL server FQDN            |
| admin_login         | PostgreSQL administrator login    |
| admin_password      | PostgreSQL administrator password |
| private_dns_zone_id | Private DNS Zone ID               |

## Database Management

The module creates databases specified in the `databases` variable. Each service (Story-Generator, App, Publisher) can use a dedicated database on the shared server, providing:

- Cost efficiency
- Centralized backup management
- Simplified network configuration
- Unified monitoring

