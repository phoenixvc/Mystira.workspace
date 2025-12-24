# Story-Generator Infrastructure Module

Terraform module for deploying **Mystira.StoryGenerator** infrastructure on Azure.

> **Note**: This module provisions infrastructure for both components:
> - **API** (`Mystira.StoryGenerator.Api`) → Kubernetes (Container Apps)
> - **Web** (`Mystira.StoryGenerator.Web`, Blazor WASM) → Azure Static Web Apps
>
> This follows the same pattern as Mystira.App. See [ADR-0019](../../../docs/adr/ADR-0019-dockerfile-location-standardization.md) for architecture documentation.

## Features

### API Infrastructure
- Network Security Groups for API and health check endpoints
- Managed Identity for service authentication
- PostgreSQL database (dedicated or shared)
- Redis cache (dedicated or shared)
- Log Analytics and Application Insights integration
- Key Vault for secrets management

### Static Web App (SWA) Infrastructure
- Azure Static Web Apps for Blazor WASM hosting
- GitHub integration for automated deployments
- Custom domain support with SSL certificates
- Front Door integration for CDN and WAF protection

## Usage

### API Only (Kubernetes)

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

### With Static Web App (Full Stack)

```hcl
module "story_generator" {
  source = "../../modules/story-generator"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mystira-dev-rg"
  vnet_id            = azurerm_virtual_network.main.id
  subnet_id          = azurerm_subnet.story_generator.id

  # Enable Static Web App for Blazor WASM
  enable_static_web_app = true
  static_web_app_sku    = "Free"  # or "Standard" for production
  fallback_location     = "eastus2"
  github_repository_url = "https://github.com/phoenixvc/Mystira.StoryGenerator"
  github_branch         = "dev"

  # Optional: Custom domain for SWA
  enable_swa_custom_domain = true
  swa_custom_domain        = "dev.story.mystira.app"

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

### Core Infrastructure

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

### Static Web App (SWA)

| Name                     | Description                                    | Type     | Default     | Required |
| ------------------------ | ---------------------------------------------- | -------- | ----------- | -------- |
| enable_static_web_app    | Deploy Static Web App for Blazor WASM frontend | `bool`   | `false`     | no       |
| static_web_app_sku       | SWA pricing tier (Free or Standard)            | `string` | `"Free"`    | no       |
| fallback_location        | Fallback region for SWA (must differ from primary) | `string` | `"eastus2"` | no       |
| github_repository_url    | GitHub repo URL for SWA deployment             | `string` | `""`        | no       |
| github_branch            | Git branch for SWA deployment                  | `string` | `"main"`    | no       |
| enable_swa_custom_domain | Enable custom domain for SWA                   | `bool`   | `false`     | no       |
| swa_custom_domain        | Custom domain for SWA (e.g., story.mystira.app)| `string` | `""`        | no       |

## Outputs

### API Infrastructure

| Name                           | Description                            |
| ------------------------------ | -------------------------------------- |
| nsg_id                         | Network Security Group ID              |
| identity_id                    | Managed Identity ID                    |
| postgresql_server_id           | PostgreSQL server ID                   |
| redis_cache_id                 | Redis cache ID                         |
| log_analytics_workspace_id     | Log Analytics workspace ID             |
| app_insights_connection_string | Application Insights connection string |
| key_vault_id                   | Key Vault ID                           |

### Static Web App

| Name                           | Description                            |
| ------------------------------ | -------------------------------------- |
| static_web_app_id              | SWA resource ID (if enabled)           |
| static_web_app_url             | SWA default hostname URL               |
| static_web_app_api_key         | SWA deployment token (sensitive)       |
| static_web_app_default_hostname| SWA default hostname for Front Door    |

## Database Configuration

The module can use either:
- **Dedicated resources**: Creates its own PostgreSQL and Redis instances
- **Shared resources**: References shared PostgreSQL and Redis from `shared/` modules

Using shared resources reduces costs and simplifies management for multi-service deployments.

