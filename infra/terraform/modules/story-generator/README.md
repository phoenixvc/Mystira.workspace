# Story-Generator Infrastructure Module

Terraform module for deploying **Mystira.StoryGenerator.Api** infrastructure on Azure.

> **Note**: This module provisions infrastructure for the **API** component, not the Blazor WASM frontend.
> StoryGenerator follows the same pattern as Mystira.App:
> - **API** (`Mystira.StoryGenerator.Api`) → Kubernetes (uses this infrastructure)
> - **Web** (`Mystira.StoryGenerator.Web`, Blazor WASM) → Static Web App (separate infrastructure if needed)
>
> See [ADR-0019](../../../docs/adr/ADR-0019-dockerfile-location-standardization.md) for architecture documentation.

## Features

- Network Security Groups for API and health check endpoints
- Managed Identity for service authentication
- PostgreSQL database (dedicated or shared)
- Redis cache (dedicated or shared)
- Log Analytics and Application Insights integration
- Key Vault for secrets management

## Usage

```hcl
module "story_generator" {
  source = "../../modules/story-generator"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mystira-dev-rg"
  vnet_id            = azurerm_virtual_network.main.id
  subnet_id          = azurerm_subnet.story_generator.id

  # Optional: Use shared resources
  shared_postgresql_server_id     = module.shared_postgresql.server_id
  shared_redis_cache_id           = module.shared_redis.cache_id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  tags = {
    Environment = "dev"
    Project     = "Mystira"
  }
}
```

## Inputs

| Name                              | Description                       | Type          | Default    | Required |
| --------------------------------- | --------------------------------- | ------------- | ---------- | -------- |
| environment                       | Deployment environment            | `string`      | -          | yes      |
| location                          | Azure region                      | `string`      | `"eastus"` | no       |
| resource_group_name               | Resource group name               | `string`      | -          | yes      |
| vnet_id                           | Virtual Network ID                | `string`      | -          | yes      |
| subnet_id                         | Subnet ID                         | `string`      | -          | yes      |
| shared_postgresql_server_id       | Shared PostgreSQL server ID       | `string`      | `null`     | no       |
| shared_redis_cache_id             | Shared Redis cache ID             | `string`      | `null`     | no       |
| shared_log_analytics_workspace_id | Shared Log Analytics workspace ID | `string`      | `null`     | no       |
| tags                              | Resource tags                     | `map(string)` | `{}`       | no       |

## Outputs

| Name                           | Description                            |
| ------------------------------ | -------------------------------------- |
| nsg_id                         | Network Security Group ID              |
| identity_id                    | Managed Identity ID                    |
| postgresql_server_id           | PostgreSQL server ID                   |
| redis_cache_id                 | Redis cache ID                         |
| log_analytics_workspace_id     | Log Analytics workspace ID             |
| app_insights_connection_string | Application Insights connection string |
| key_vault_id                   | Key Vault ID                           |

## Database Configuration

The module can use either:
- **Dedicated resources**: Creates its own PostgreSQL and Redis instances
- **Shared resources**: References shared PostgreSQL and Redis from `shared/` modules

Using shared resources reduces costs and simplifies management for multi-service deployments.

