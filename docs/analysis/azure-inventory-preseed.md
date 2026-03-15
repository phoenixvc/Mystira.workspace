# Azure Inventory (Pre-Seed Snapshot)

This is a quick inventory of Azure resource groups and deployed services, based on the `az ... list` outputs captured in the terminal session during March 2026.

## Summary

- Mystira footprint is primarily in `southafricanorth` (SAN) with Static Web Apps in `eastus2` (EUS2).
- There are multiple non-Mystira projects in the same subscription (e.g., Rooivalk, Puffwise, PhoenixVC, WhatsSummarize, AI Gateway).
- For pre-seed cost control, the biggest levers are: AKS (running clusters), Cosmos account count, Redis count, and monitoring ingestion/retention.

## Mystira Resource Groups (by naming)

### Shared / Cross-Environment

- `mys-shared-terraform-rg-san` (DNS zone `mystira.app`, terraform state)
- `mys-shared-acr-rg-san` (ACR `myssharedacr`)

### Dev

- `mys-dev-core-rg-san`
- `mys-dev-app-rg-san`
- `mys-dev-story-rg-san`
- `mys-dev-admin-rg-san`
- `mys-dev-publisher-rg-san`
- `mys-dev-chain-rg-san`

### Staging

- `mys-staging-core-rg-san`
- `mys-staging-app-rg-san`
- `mys-staging-story-rg-san`
- `mys-staging-admin-rg-san`
- `mys-staging-publisher-rg-san`
- `mys-staging-chain-rg-san`

### Prod

- `mys-prod-core-rg-san`
- `mys-prod-mystira-rg-san`
- `mys-prod-story-rg-san`
- `mys-prod-admin-rg-san`

## Deployed Services (Mystira)

### AKS

| Cluster                    | Resource Group            | Region           | Power State |
| -------------------------- | ------------------------- | ---------------- | ----------- |
| `mys-dev-core-aks-san`     | `mys-dev-core-rg-san`     | southafricanorth | Stopped     |
| `mys-staging-core-aks-san` | `mys-staging-core-rg-san` | southafricanorth | Running     |

### App Services (Web Apps)

| Web App                         | Resource Group            | Region           | State   |
| ------------------------------- | ------------------------- | ---------------- | ------- |
| `mys-dev-api-san`               | `mys-dev-app-rg-san`      | southafricanorth | Running |
| `mys-staging-mystira-api-san`   | `mys-staging-app-rg-san`  | southafricanorth | Running |
| `mys-prod-mystira-api-san`      | `mys-prod-mystira-rg-san` | southafricanorth | Running |
| `mys-prod-mystira-adminapi-san` | `mys-prod-mystira-rg-san` | southafricanorth | Running |
| `mys-dev-story-api-san`         | `mys-dev-story-rg-san`    | southafricanorth | Running |
| `mys-dev-story-app-san`         | `mys-dev-story-rg-san`    | southafricanorth | Running |

### Static Web Apps (SWA)

| SWA                            | Resource Group             | Region  | SKU  |
| ------------------------------ | -------------------------- | ------- | ---- |
| `mys-dev-swa-eus2`             | `mys-dev-app-rg-san`       | eastus2 | Free |
| `mys-staging-mystira-swa-eus2` | `mys-staging-app-rg-san`   | eastus2 | Free |
| `mys-prod-mystira-swa-eus2`    | `mys-prod-mystira-rg-san`  | eastus2 | Free |
| `mys-dev-story-swa-eus2`       | `mys-dev-story-rg-san`     | eastus2 | Free |
| `mys-staging-story-swa-eus2`   | `mys-staging-story-rg-san` | eastus2 | Free |
| `mys-prod-story-swa-eus2`      | `mys-prod-story-rg-san`    | eastus2 | Free |

### PostgreSQL Flexible Server

| Server                | Resource Group            | Region           | Version | SKU           |
| --------------------- | ------------------------- | ---------------- | ------- | ------------- |
| `mys-dev-core-db`     | `mys-dev-core-rg-san`     | southafricanorth | 15      | Standard_B1ms |
| `mys-staging-core-db` | `mys-staging-core-rg-san` | southafricanorth | 15      | Standard_B1ms |
| `mys-prod-core-db`    | `mys-prod-core-rg-san`    | southafricanorth | 15      | Standard_B1ms |

### Cosmos DB Accounts

| Account                       | Resource Group            | Region           |
| ----------------------------- | ------------------------- | ---------------- |
| `mys-dev-core-cosmos-san`     | `mys-dev-core-rg-san`     | southafricanorth |
| `mys-staging-core-cosmos-san` | `mys-staging-core-rg-san` | southafricanorth |
| `mys-prod-mystira-cosmos-san` | `mys-prod-mystira-rg-san` | southafricanorth |
| `mys-dev-story-cosmos-san`    | `mys-dev-story-rg-san`    | southafricanorth |

### Redis

| Cache                           | Resource Group           | Region           | SKU                   |
| ------------------------------- | ------------------------ | ---------------- | --------------------- |
| `mys-dev-core-cache`            | `mys-dev-core-rg-san`    | southafricanorth | Standard (capacity 1) |
| `mys-prod-core-cache`           | `mys-prod-core-rg-san`   | southafricanorth | Standard (capacity 1) |
| `mys-staging-mystira-redis-san` | `mys-staging-app-rg-san` | southafricanorth | Basic (capacity 0)    |

### Service Bus

| Namespace             | Resource Group            | Region           | SKU      |
| --------------------- | ------------------------- | ---------------- | -------- |
| `mysstagingcoresbsan` | `mys-staging-core-rg-san` | southafricanorth | Standard |

### ACR

| Registry       | Resource Group          | Region           | SKU      |
| -------------- | ----------------------- | ---------------- | -------- |
| `myssharedacr` | `mys-shared-acr-rg-san` | southafricanorth | Standard |

### Log Analytics Workspaces (Mystira)

| Workspace                  | Resource Group            | Region           | Retention |
| -------------------------- | ------------------------- | ---------------- | --------: |
| `mys-dev-core-log`         | `mys-dev-core-rg-san`     | southafricanorth |        30 |
| `mys-staging-core-log`     | `mys-staging-core-rg-san` | southafricanorth |        30 |
| `mys-prod-core-log`        | `mys-prod-core-rg-san`    | southafricanorth |        90 |
| `mys-prod-mystira-log-san` | `mys-prod-mystira-rg-san` | southafricanorth |        90 |

### DNS Zones

| Zone          | Resource Group                | Record Sets |
| ------------- | ----------------------------- | ----------: |
| `mystira.app` | `mys-shared-terraform-rg-san` |          48 |

## Non-Mystira Projects Observed

This subscription also contains resource groups and services for other projects (examples seen in outputs):

- Rooivalk (`dev-eus2-rg-rooivalk`, `dev-euw-rg-rooivalk`, DNS zone `phoenixrooivalk.com`)
- Puffwise (`nl-dev-puffwise-rg-san`, `nl-staging-puffwise-rg-san`, `nl-prod-puffwise-rg-san`, `nl-shared-puffwise-rg-san`)
- WhatsSummarize (`rg-whatssummarize-dev`)
- AI Gateway (`pvc-dev-aigateway-rg-san`, `pvc-prod-aigateway-rg-san`)
- PhoenixVC website (`prod-euw-rg-phoenixvc-website`, `staging-euw-rg-phoenixvc-website`, etc.)

## Pre-Seed Cost Flags (Actionable)

- Staging AKS is Running (`mys-staging-core-aks-san`). If staging is not actively used, stopping it is a fast cost win.
- Cosmos accounts: there is an extra dev cosmos under story (`mys-dev-story-cosmos-san`) in addition to the dev core cosmos (`mys-dev-core-cosmos-san`). Consolidating dev workloads onto the dev core account is consistent with the “one-per-env” target.
- Redis: staging Redis is currently in the app RG (`mys-staging-mystira-redis-san`) rather than the staging core RG (`mys-staging-core-cache` doesn’t appear in the captured list). Migrating staging to a core cache aligns with consolidation and reduces sprawl.
- Monitoring: prod has both `mys-prod-core-log` and `mys-prod-mystira-log-san`. If the desired model is “one LA workspace per env”, this likely needs cleanup/migration.

## Suggested Pre-Seed Actions (Concrete)

### 1) Pause staging to reduce burn

- Stop staging AKS (fastest win):
  - `az aks stop -g mys-staging-core-rg-san -n mys-staging-core-aks-san`
  - Restart later with: `az aks start -g mys-staging-core-rg-san -n mys-staging-core-aks-san`

### 2) Consolidate dev StoryGenerator Cosmos onto dev core Cosmos

Goal: Use `mys-dev-core-cosmos-san` instead of `mys-dev-story-cosmos-san`.

- Ensure shared-infra/dev is deployed (it owns `mys-dev-core-cosmos-san`).
- Update StoryGenerator to consume shared Cosmos:
  - Terraform now supports storing Cosmos endpoint/key/db/container secrets in the StoryGenerator Key Vault from the shared Cosmos connection string.
  - Apply:
    - `cd infra/terraform/shared-infra/environments/dev && terragrunt apply`
    - `cd infra/terraform/products/story-generator/environments/dev && terragrunt apply`
- Then repoint the StoryGenerator runtime config to read:
  - `CosmosDb:Endpoint` from Key Vault secret `cosmos-endpoint`
  - `CosmosDb:ApiKey` from Key Vault secret `cosmos-api-key`
  - `CosmosDb:DatabaseId` from Key Vault secret `cosmos-database-id`
  - `CosmosDb:ContainerId` from Key Vault secret `cosmos-container-id`

### 3) Align staging Redis to core/shared cache

Goal: Stop using `mys-staging-mystira-redis-san` and use `mys-staging-core-cache` from shared-infra.

- Deploy shared-infra/staging (creates the core cache):
  - `cd infra/terraform/shared-infra/environments/staging && terragrunt apply`
- Ensure product stacks are configured to use shared Redis (Mystira.App is already wired to shared Redis in the product layer).
- After cutover, delete the staging app-level cache when it’s no longer referenced (cache data is usually safe to drop).

### 4) Fix prod Log Analytics duplication without breaking anything

Goal: Move toward “one workspace per env” without deleting anything prematurely.

- Identify the workspaces:
  - `az monitor log-analytics workspace list --query "[?contains(resourceGroup,'mys-prod')].{name:name, rg:resourceGroup, retention:retentionInDays}" -o table`
- For each of these resources, check which workspace they are sending diagnostics to:
  - Web App:
    - `az webapp show -g mys-prod-mystira-rg-san -n mys-prod-mystira-api-san --query id -o tsv`
    - `az monitor diagnostic-settings list --resource <RESOURCE_ID> -o json`
  - AKS:
    - `az aks show -g mys-prod-core-rg-san -n mys-prod-core-aks-san --query id -o tsv`
    - `az monitor diagnostic-settings list --resource <RESOURCE_ID> -o json`
- Update diagnostics to point at the chosen workspace (recommend: `mys-prod-core-log`), then validate logs still arrive.
- Only after 1–2 weeks of confidence, consider deleting the unused workspace.

Safety rule: do not delete Cosmos DB accounts as part of any of these cost actions.
