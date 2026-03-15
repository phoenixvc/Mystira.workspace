# Other Repo Migration Plan (Adopt Mystira Shared-Infra + Terragrunt)

This plan is for migrating another product repo (e.g., Rooivalk/Chauffeur) to the Mystira “shared-infra + per-product state” model, without breaking production and without creating duplicate resources.

## Goals

- Adopt the shared-infra contract (Cosmos/Redis/Storage/ServiceBus/Log Analytics) while keeping product-specific compute separate.
- Isolate state per product and per environment.
- Avoid accidental prod changes.
- Avoid “apply into empty state” against an already-deployed environment.

## Non-goals

- Moving edge/networking (DNS/Front Door/WAF/VNet/AKS/Ingress). Keep that as a separate, deliberate migration phase.

## Pre-flight checklist

- Ensure you can authenticate:
  - `az login`
  - `az account show`
- Ensure Terraform + Terragrunt are installed and match the workspace versions.
- Confirm which subscription/tenant you’re targeting.
- Identify current Azure footprint:
  - Resource groups
  - App Service / SWA / AKS
  - Datastores (Cosmos/Postgres/Storage)
  - Messaging (Service Bus)
  - Monitoring (Log Analytics + App Insights)
  - Secrets (Key Vault)

## Target structure (minimal)

In the product repo:

```text
infra/terraform/products/<product>/environments/dev
infra/terraform/products/<product>/environments/staging
infra/terraform/products/<product>/environments/prod
```

Use the Mystira shared conventions:

- Remote state: `/<product>/<env>.tfstate`
- Product depends on shared-infra outputs (by contract), not by copying resource creation.

## Tooling options for other repos

Pick one, but be explicit so migration later is predictable.

### Option 1: Terraform + Terragrunt (recommended once there is scale)

Pros:

- Multi-env and multi-product drift is much lower
- Dependencies are explicit (shared-infra outputs)
- Easier to add safety rails (like blocking prod apply by default)

Cons:

- One more tool to install and learn

### Option 2: Terraform only (acceptable for small scope)

Pros:

- Simpler toolchain (Terraform only)

Cons:

- More copy/paste and more chances of env drift
- You end up rebuilding Terragrunt-like patterns manually (folders, vars, state keys)

### Option 3: Pipelines-only (no Terraform yet)

This is valid for an early team, as long as you accept that:

- You are accumulating “manual state” inside scripts and the portal
- The migration later should be import-first (adopt existing resources into state)

If you choose pipelines-only, do these minimum steps now:

- Adopt naming + tags conventions (so imports later are feasible).
- Produce a machine-readable inventory artifact every run (resource IDs and key settings).
- Keep secrets in Key Vault (not in pipeline variables where possible).

## Migration phases

### Phase 0: Freeze prod drift

- Pause new infra changes for the target product while migrating state.
- Ensure prod apply is protected (policy, approvals, or `ALLOW_PROD_APPLY=true` guard).

### Phase 1: Create state layout without changing resources

- Add Terragrunt entrypoints for the product (dev/staging/prod).
- Run:
  - `terragrunt init -backend=false`
  - `terragrunt validate`

Do not apply yet.

### Phase 2: Adopt existing resources into state (import-first)

For each environment:

1. Run `terragrunt plan` and confirm it wants to create resources that already exist.
2. For each such resource, import it into state.
3. Re-run `plan` until it becomes “no changes” or only contains intentional diffs.

Rules:

- Prefer importing full resources rather than recreating them.
- If a resource is misnamed vs the new convention, still import it; rename later as a controlled change.
- Never delete data services (Cosmos/Postgres) during adoption. Replace only after proven cutover.

### Phase 3: Switch to shared-infra consumption (dev first)

Goal: stop creating duplicate shared resources per product.

- In dev, rewire the product to use shared-infra outputs:
  - Redis connection
  - Cosmos connection
  - Storage connection
  - Service Bus connection
  - Log Analytics workspace id
- Validate runtime and telemetry.
- Only after dev is stable, repeat for staging, then prod.

### Phase 4: Optional cleanup (after confidence window)

After 1–2 weeks:

- Decommission product-local duplicates if they are truly unused.
- Prefer “disable references first”, “delete later”.

## “Shared-infra contract” (what products should consume)

Products should treat these as inputs (outputs from shared-infra):

- `redis_connection_string`
- `cosmos_db_connection_string`
- `storage_connection_string`
- `servicebus_connection_string`
- `log_analytics_workspace_id`

If a product needs dedicated resources, document why and keep it explicit.

## Validation gates

- `terragrunt plan` shows no unexpected changes.
- App health checks pass (dev → staging → prod).
- Logs/metrics still flow to the expected workspace.
- No data loss (Cosmos/Postgres not recreated).

## Rollback approach

- State-only rollback: revert the Terraform/Terragrunt code change; do not delete resources.
- Runtime rollback: switch app configuration back to prior connection strings/endpoints.
