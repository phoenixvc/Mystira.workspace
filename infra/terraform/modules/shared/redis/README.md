# Shared Redis Infrastructure Module

Terraform module for deploying shared Redis cache infrastructure on Azure.

## Features

- Azure Cache for Redis
- VNet integration (Premium tier)
- Configurable capacity and SKU
- TLS enforcement
- Memory policy configuration

## Usage

```hcl
module "shared_redis" {
  source = "../../modules/shared/redis"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mystira-dev-rg"
  subnet_id          = azurerm_subnet.redis.id

  capacity = 1
  family   = "C"
  sku_name = "Standard"

  tags = {
    Environment = "dev"
    Project     = "Mystira"
  }
}
```

## Inputs

| Name                | Description                      | Type          | Default          | Required |
| ------------------- | -------------------------------- | ------------- | ---------------- | -------- |
| environment         | Deployment environment           | `string`      | -                | yes      |
| location            | Azure region                     | `string`      | `"eastus"`       | no       |
| resource_group_name | Resource group name              | `string`      | -                | yes      |
| subnet_id           | Subnet ID (required for Premium) | `string`      | `null`           | no       |
| capacity            | Redis cache capacity             | `number`      | `1`              | no       |
| family              | Redis cache family               | `string`      | `"C"`            | no       |
| sku_name            | Redis cache SKU name             | `string`      | `"Standard"`     | no       |
| enable_non_ssl_port | Enable non-SSL port              | `bool`        | `false`          | no       |
| minimum_tls_version | Minimum TLS version              | `string`      | `"1.2"`          | no       |
| maxmemory_policy    | Redis maxmemory policy           | `string`      | `"volatile-lru"` | no       |
| tags                | Resource tags                    | `map(string)` | `{}`             | no       |

## Outputs

| Name                      | Description                           |
| ------------------------- | ------------------------------------- |
| cache_id                  | Redis cache ID                        |
| cache_name                | Redis cache name                      |
| hostname                  | Redis cache hostname                  |
| ssl_port                  | Redis cache SSL port                  |
| port                      | Redis cache port                      |
| primary_access_key        | Redis cache primary access key        |
| primary_connection_string | Redis cache primary connection string |

## SKU Recommendations

- **Development**: `Basic` or `Standard` (C1, 1GB)
- **Staging**: `Standard` (C1, 1GB)
- **Production**: `Standard` (C2, 2.5GB) or `Premium` (P1, 6GB) with VNet integration

## Security

- TLS 1.2 minimum enforced by default
- Non-SSL port disabled by default
- VNet integration recommended for production (Premium tier)

