# Infrastructure Consolidation Cost Estimate (Pre vs Post)

This document provides a rough cost comparison for Mystira infrastructure before vs after consolidation into shared-infra + product states (Terragrunt).

## Scope

- Focus: high-leverage shared resources and “one-per-environment where possible” standardization:
  - Log Analytics / Application Insights strategy
  - Redis
  - Cosmos DB
  - Storage
  - Azure Communication Services (ACS)
  - Container Registry (ACR)
- Non-scope (planned separately): DNS zone management, Front Door/WAF, networking (VNet/AKS), ingress, and cluster sizing.

Non-scope plan: [infra-edge-networking-plan.md](../planning/infra-edge-networking-plan.md).

## Baseline Assumptions

- Environments: dev, staging, prod
- Pattern: prefer shared, environment-scoped foundational resources (core/shared-infra), and keep product resources app-specific only when required.
- Prices vary heavily by region, usage (GB ingested, requests, egress), and retention. Numbers below are directional.

## Pre-Consolidation (Typical Anti-Pattern)

Symptoms:

- Multiple products creating their own:
  - Log Analytics workspaces / Application Insights instances
  - Redis caches
  - Cosmos DB accounts (or multiple DBs without clear isolation)
  - Storage accounts
- Duplicate “support” resources (key vaults, monitoring) created per service even when workloads are small.

Impact:

- Higher baseline monthly spend (multiple base charges).
- Higher operational cost (more state churn, more drift, more secrets wiring).
- Harder optimization (each service needs its own tuning and alerting).

## Post-Consolidation (Target Pattern)

Shared-infra (per environment):

- 1× Log Analytics workspace per environment (shared)
- 1× Redis cache per environment (shared)
- 1× Cosmos DB account per environment (shared; multiple databases/containers as needed)
- 1× Storage account per environment (shared; multiple containers/queues as needed)
- 1× Service Bus namespace per environment (shared)
- 1× ACR per environment (or shared cross-env where policy allows)

Cross-environment shared (created once, referenced everywhere):

- 1× Azure Communication Services instance (shared)
- Shared DNS zone resource group / tfstate resource group (bootstrap-owned)

## Cross-Environment + Cross-Product Sharing Matrix

This section answers: “Which of these should be shared across environments and across other products (Rooivalk, Chauffeur, etc.)?”

Guiding rule:

- Prefer sharing across products within the same environment.
- Share across environments only when the service is truly global/tenant-scoped and secrets/identity boundaries are clear.
- Share across different products/repos only if you want a single platform foundation with shared ops/chargeback and consistent access controls.

| Resource                | Share across products in same env? | Share across environments? | Share across other products/repos? | Notes                                                                                                                                                                                           |
| ----------------------- | ---------------------------------- | -------------------------- | ---------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Log Analytics workspace | Yes                                | Usually no                 | Yes (if central platform)          | Best default: 1 workspace per env, used by all services. Cross-env sharing makes alerting/retention/chargeback harder and increases blast radius.                                               |
| Redis cache             | Yes (with isolation)               | Usually no                 | Sometimes                          | Prefer 1 per env; isolate with key prefixes, TTL discipline, and optionally separate logical DBs (where supported). For unrelated products, consider dedicated caches for blast-radius control. |
| Cosmos DB account       | Yes (with isolation)               | Usually no                 | Sometimes                          | Prefer 1 account per env, multiple databases/containers per product. Cross-repo sharing is viable if tenancy/permissions are clean; otherwise keep separate accounts per product family.        |
| Storage account         | Yes                                | Usually no                 | Sometimes                          | Often safe to share across products (containers per product). Separate accounts if you need different replication, access policies, or lifecycle rules.                                         |
| Service Bus namespace   | Yes                                | Usually no                 | Sometimes                          | Strong candidate for shared-infra per env. Use topics/queues per product. Cross-product sharing is OK if teams agree on governance and naming.                                                  |
| ACR                     | Yes                                | Yes (often)                | Yes                                | Strong candidate for org-wide sharing (one ACR reused by dev/staging/prod). Separate ACRs only for compliance boundaries or to enforce strict isolation.                                        |
| ACS                     | Yes                                | Yes                        | Yes                                | Best as cross-environment + cross-product (one service). Keep secrets in per-env Key Vaults or a shared Key Vault with strict RBAC.                                                             |

## Cost Comparison (Directional)

## Pre-Seed / Startup Cost Profile (When Cash Burn Matters)

As we are in pre-seed and optimizing for lowest baseline cost while still keeping an upgrade path, prefer:

- Founder-oriented adoption model: [terragrunt-for-founders.md](./terragrunt-for-founders.md).
- Other repo adoption/migration plan: [other-repo-migration-plan.md](../planning/other-repo-migration-plan.md).

- Run only one environment initially (dev-like). Treat staging/prod as “add later”.
- Maximize “shared-infra per env” reuse (one Redis/Cosmos/Storage/Service Bus/Log Analytics).
- Prefer SKUs and modes that scale with usage rather than fixed baseline:
  - Cosmos DB: serverless (or lowest throughput/autoscale if serverless is not acceptable).
  - Redis: smallest tier that meets requirements; move to dedicated/per-product only when blast radius becomes a problem.
  - Log Analytics: short retention in non-prod; keep ingestion low by pruning noisy logs and sampling traces.
  - Service Bus: Standard unless you truly need Premium features (dedicated capacity, advanced isolation).
  - ACR: Basic/shared.

Startup-minded tradeoffs:

- Application Insights: keep per-service AI instances — we don't yet need strong separation; therefore, our infra favors a single workspace with consistent tags and alert rules.
- Cosmos and Redis: share early, split later. Early-stage operational simplicity is worth the shared blast radius.
- “Non-scope” items (DNS/Front Door/networking/AKS) are the most likely to force fixed monthly spend; defer until we have real traffic or clear requirements.

### Baseline Savings Drivers

- Log Analytics: reducing N workspaces → 1 workspace per env eliminates duplicated base costs and simplifies retention policy.
- Redis: shared cache avoids multiple caches with low utilization.
- Cosmos: consolidating to 1 account per env reduces duplicated account-level overhead and simplifies throughput strategy.
- Storage: consolidating reduces account sprawl; cost impact is usually small but operational impact is high.
- ACS: one cross-env service reduces repeated fixed costs and key management duplication.

### Example “Order of Magnitude” Scenarios

Small/medium usage (typical dev/staging):

- Pre: multiple workspaces + multiple redis + multiple small storage/cosmos accounts
- Post: shared-infra only
- Expected savings: 20–60% of infra baseline (excluding compute), mostly from monitoring + caches.

Prod:

- Monitoring costs are driven by ingestion/retention rather than number of workspaces, but consolidation still reduces baseline overhead and makes ingestion governance easier.
- Redis/Cosmos consolidation savings depend on whether per-service caches/accounts were underutilized.

## How to Measure with Infracost (Recommended)

Infracost will give the most reliable estimate for baseline vs target, using the Terraform code at each stage.

### Pre-Consolidation

- Use the legacy configuration (historical commit) or the legacy `infra/terraform/environments/*` directory if still present.
- Run:
  - `infracost breakdown --path <legacy-dir> --format table`

### Post-Consolidation

- Use Terragrunt entrypoints:
  - `infra/terraform/shared-infra/environments/<env>`
  - `infra/terraform/products/<product>/environments/<env>`
- Run:
  - `infracost breakdown --path infra/terraform/shared-infra/environments/<env> --format table`
  - `infracost breakdown --path infra/terraform/products/mystira-app/environments/<env> --format table`
  - Repeat for other products

## Notes / Risks

- Consolidation can increase “blast radius” if shared resources are not isolated well (e.g., noisy neighbor on Redis).
- Cosmos consolidation requires careful database/container naming and throughput strategy.
- Monitoring consolidation requires alert routing discipline (service tags, resource-specific alerts).
