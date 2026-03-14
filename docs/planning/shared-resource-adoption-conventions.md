# Shared Resource Adoption + Naming Conventions (Cross-Repo)

This document defines a lightweight “shared resource contract” and naming conventions that other repos/products can follow when adopting shared infrastructure (Redis, Cosmos DB, Service Bus, Storage, Log Analytics, Key Vault).

Goal: make it easy to share early (cost + speed), then split later (blast radius + compliance), without a rewrite.

## Shared resource contract (what a product should consume)

If a platform/shared-infra layer exists, product repos should treat these as inputs (not things they create themselves):

- Redis:
  - `redis_connection_string` (preferred) or `redis_hostname` + `redis_ssl_port`
- Cosmos DB:
  - `cosmos_db_connection_string` (preferred)
  - optionally `cosmos_db_endpoint` (for SDKs that separate endpoint/key)
- Storage:
  - `storage_connection_string`
  - optionally `storage_blob_endpoint`
- Service Bus:
  - `servicebus_connection_string`
- Monitoring:
  - `log_analytics_workspace_id`
  - `application_insights_connection_string` (optional; use if you keep AI per-service)

Products should not depend on the internals of how these are created—only the outputs.

## Environment scoping (default)

Default for early-stage:

- One shared instance per environment (dev/staging/prod):
  - 1 Redis cache
  - 1 Cosmos DB account
  - 1 Storage account
  - 1 Service Bus namespace
  - 1 Log Analytics workspace

This keeps data/telemetry separated across environments while still reducing baseline spend.

## Naming conventions (resources)

### General

- Prefix with org/project + environment + component.
- Keep names stable; rename only in deliberate migrations.
- Tag everything with:
  - `Project`, `Environment`, `Product`, `ManagedBy`, and optionally `Owner`, `CostCenter`.

### Log Analytics

- Prefer one workspace per env and route diagnostics there.
- Set short retention in non-prod (and lower ingestion by pruning noisy logs).

### Service Bus (topics/queues/subscriptions)

Use a consistent namespace within the shared Service Bus namespace:

- Topic names: `<product>.<domain>.<event>`
  - Example: `mystira.accounts.user-created`
- Queue names: `<product>.<purpose>`
  - Example: `costops.ingest`
- Subscription names: `<consumer-product>` or `<consumer-service>`
  - Example: `admin-api`, `publisher-worker`

Rules:

- Keep topic names lowercase and kebab-case or dot-separated; pick one and keep it consistent.
- Avoid embedding env in the entity name if the namespace is already per-env.

### Cosmos DB (database + container)

Shared Cosmos account per env; isolate per product by database, then per feature by container:

- Database id: `<ProductName>` in PascalCase
  - Example: `MystiraStoryGenerator`
- Container id: `<FeatureName>` in PascalCase
  - Example: `StorySessions`

Rules:

- Do not use env suffixes inside database/container ids if the account is already per-env.
- Partition keys must be chosen per container with long-term query patterns in mind.
- Prefer separate containers for high-write/high-volume streams to avoid noisy-neighbor RU contention.

### Redis (keyspace)

In shared Redis per env, isolate by key prefix:

- Key prefix: `<product>:<namespace>:`
  - Example: `costops:cache:`, `mystira:session:`

Rules:

- TTL discipline: everything cache-like should have an expiry unless explicitly persistent.
- Do not use Redis logical databases as isolation unless you have a strong reason; key prefixes are simpler.

### Storage (containers/paths)

Shared storage account per env; isolate by container per product (or per data class):

- Container name: `<product>-<category>` (lowercase, hyphenated)
  - Example: `mystira-media`, `costops-reports`

Rules:

- Use lifecycle policies per prefix/container to control retention.

## Key Vault secret naming (configuration compatibility)

Prefer secrets that map cleanly to configuration keys across .NET and other stacks:

- Use double-dash to represent nested config keys:
  - `CosmosDb--Endpoint`
  - `CosmosDb--ApiKey`
  - `CosmosDb--DatabaseId`
  - `CosmosDb--ContainerId`
  - `ServiceBus--ConnectionString`
  - `Redis--ConnectionString`

This lets apps load configuration directly from Key Vault into standard config systems.

## When to split shared resources (rule of thumb)

Split (dedicate) when at least one is true:

- A product’s load breaks another product (latency, throttling, outages).
- You need hard access boundaries (compliance, regulated data).
- You need different SKUs/retention/replication policies that conflict.

When you split, keep naming and output contracts the same so product code doesn’t change—only the wiring does.
