# Shared Monitoring Infrastructure Module

Terraform module for deploying shared monitoring infrastructure (Log Analytics + Application Insights) on Azure.

## Features

- Log Analytics Workspace for centralized logging
- Application Insights for APM
- Metric alerts (CPU usage)
- Action groups for alert notifications

## Usage

```hcl
module "shared_monitoring" {
  source = "../../modules/shared/monitoring"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mystira-dev-rg"

  retention_in_days = 30

  tags = {
    Environment = "dev"
    Project     = "Mystira"
  }
}
```

## Inputs

| Name                | Description                     | Type          | Default       | Required |
| ------------------- | ------------------------------- | ------------- | ------------- | -------- |
| environment         | Deployment environment          | `string`      | -             | yes      |
| location            | Azure region                    | `string`      | `"eastus"`    | no       |
| resource_group_name | Resource group name             | `string`      | -             | yes      |
| sku                 | Log Analytics SKU               | `string`      | `"PerGB2018"` | no       |
| retention_in_days   | Log Analytics retention in days | `number`      | `null`        | no       |
| tags                | Resource tags                   | `map(string)` | `{}`          | no       |

## Outputs

| Name                                       | Description                                |
| ------------------------------------------ | ------------------------------------------ |
| log_analytics_workspace_id                 | Log Analytics Workspace ID                 |
| log_analytics_workspace_name               | Log Analytics Workspace name               |
| log_analytics_workspace_primary_shared_key | Log Analytics Workspace primary shared key |
| application_insights_id                    | Application Insights ID                    |
| application_insights_connection_string     | Application Insights connection string     |
| application_insights_instrumentation_key   | Application Insights instrumentation key   |

## Integration

Services (Chain, Publisher, Story-Generator) can reference this shared workspace:

```hcl
module "publisher" {
  source = "../../modules/publisher"
  # ...
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
}
```

This provides:
- Unified logging across all services
- Centralized monitoring dashboards
- Consistent retention policies
- Cost optimization through resource sharing

## Retention

- **Development**: 30 days
- **Staging**: 30 days
- **Production**: 90 days

Custom retention can be configured via `retention_in_days` variable.

