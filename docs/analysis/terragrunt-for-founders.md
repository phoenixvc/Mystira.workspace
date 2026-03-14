# Terragrunt for Founders (Pre-Seed / Early Startup)

This document explains why Mystira uses Terragrunt, how it reduces risk for small teams, and how to think about cross-environment and cross-product adoption while keeping cost and complexity low.

## Why Terragrunt (in plain terms)

Terraform is the engine. Terragrunt is the wiring harness.

Terragrunt helps when you have more than one environment (dev/staging/prod) and more than one deployable product (Mystira App, Story Generator, Rooivalk, Chauffeur, etc) by:

- Keeping each environment and product in its own state file (so a mistake in one area doesn’t brick everything).
- Removing copy/paste across environment folders (so you don’t drift and you don’t forget flags).
- Making dependencies explicit (products can safely consume shared-infra outputs).
- Enabling safe parallelism (deploy only what changed, in the right order).

## What “good” looks like for pre-seed

Pre-seed priorities are usually:

- Reduce baseline spend
- Reduce operational load (fewest moving parts)
- Keep an upgrade path to a more secure / isolated prod later

The Terragrunt model that matches that is:

- Shared-infra per environment (dev/staging/prod), with “one-per-env where possible”.
- Product stacks per environment, consuming shared-infra outputs rather than recreating everything.
- Edge/networking (DNS, Front Door/WAF, VNet/AKS, ingress, cluster sizing) treated as a later phase when it’s worth the disruption.

Related:

- Cost estimate and measurement approach: [infra-consolidation-cost-estimate.md](file:///c:/Users/smitj/repos/Mystira.workspace/docs/analysis/infra-consolidation-cost-estimate.md)
- Edge/networking plan: [infra-edge-networking-plan.md](file:///c:/Users/smitj/repos/Mystira.workspace/docs/planning/infra-edge-networking-plan.md)

## Cross-Environment vs Cross-Product: a founder-friendly model

Use three buckets. This makes decisions fast and reversible.

### Bucket 1: Org-Global (shared across environments and products)

Share these early if you want centralized ops and lowest baseline:

- ACR (Container Registry)
- ACS (Azure Communication Services)
- Terraform state storage (single storage account + container)

These services are naturally multi-tenant and easy to permission with RBAC.

### Bucket 2: Per-Environment Shared (shared across products in the same env)

Default for early-stage:

- Log Analytics workspace
- Redis cache
- Cosmos DB account (multiple databases/containers)
- Storage account
- Service Bus namespace

This gives you cost + simplicity wins without mixing dev/staging/prod data and alerting.

### Bucket 3: Per-Product Dedicated (only when you must)

Use dedicated resources when:

- One product’s workload can break others (blast radius becomes real)
- Compliance requires it
- Isolation is a strong product requirement (multi-tenant concerns)

In practice, Redis and Cosmos are often “shared early, split later”.

## A simple adoption path (Rooivalk, Chauffeur, etc.)

## Do other repos need Terragrunt?

They don’t have to, but you should pick one of these based on team maturity and scale:

### Option A: Terraform only (no Terragrunt)

Use this when:

- One environment, one stack, low churn
- One person owns infra changes end-to-end
- You’re OK with a bit of copy/paste between envs (or you don’t have envs yet)

What you must still get right:

- Remote state (and locking) is non-negotiable.
- Keep shared resources (Redis/Cosmos/LA/ServiceBus/Storage) out of the product repo, or treat them as explicit external inputs.

### Option B: Terragrunt (recommended once you have 2+ envs or 2+ products)

Use this when:

- Multiple environments and you want consistent guardrails
- Multiple products will share “foundation” services (shared-infra contract)
- You want to make dependencies explicit and deploy safely in the right order

### Option C: “Pipelines only” (no Terraform yet)

Yes, a team can start with pipelines only. It’s common early, but treat it as a temporary phase.

Use this when:

- The team isn’t ready for IaC and wants the shortest path to shipping
- The infrastructure footprint is still tiny and the blast radius is acceptable

What to do to keep an upgrade path:

- Standardize naming (RGs, apps, KV secrets, tags) from day 1.
- Keep a single “inventory” output from the pipeline (resource IDs, endpoints, SKUs).
- Don’t hand-create complicated networking; defer it until there’s an IaC baseline.
- Plan for an “import-first” adoption later (see [other-repo-migration-plan.md](file:///c:/Users/smitj/repos/Mystira.workspace/docs/planning/other-repo-migration-plan.md)).

### Step 1: Adopt the shared-infra contract

New products should not create “their own everything”.

Instead, they consume shared-infra outputs:

- `redis_connection_string`
- `cosmos_db_connection_string`
- `log_analytics_workspace_id`
- `servicebus_connection_string`
- `storage_connection_string`

### Step 1.5: Decide where the product repo lives

You have two viable options:

- Keep product repos separate (recommended early): each repo contains only its own app code, plus a small infra folder that points at the same shared-infra/state conventions.
- Consolidate into the Mystira workspace later: once the platform pattern stabilizes, you can move repos into `apps/` and keep the same Terragrunt state structure.

### Step 2: Add a product stack per environment

Each product has:

- `infra/terraform/products/<product>/environments/dev`
- `.../staging`
- `.../prod`

That product stack should be small: mostly app-specific compute + app configuration wiring to shared services.

### Step 2.5: Migrate without disruption (adopt before you create)

If the product already exists in Azure, do not run `apply` into a blank state and hope it “matches”.

The safe order is:

1. Plan the target state layout (shared-infra state + per-product states).
2. Import/adopt existing resources into the new state.
3. Only then start refactoring modules and moving resources between state files.

Use the dedicated plan here:

- [other-repo-migration-plan.md](file:///c:/Users/smitj/repos/Mystira.workspace/docs/planning/other-repo-migration-plan.md)

### Step 3: Only then consider environment expansion

Pre-seed recommendation:

- Run dev only, and treat staging/prod as “add later”
- If you do need staging, keep it “cheap” and stop it when unused (especially AKS)

## Safety rails

Prod apply is blocked by default unless you opt in:

- `ALLOW_PROD_APPLY=true`

This helps prevent accidental prod changes during fast iteration.

## When NOT to use Terragrunt

Terragrunt adds one more tool and one more layer. Skip it when:

- You truly have one environment and one stack, and you are not expanding soon
- You are not ready to adopt consistent naming, state isolation, and dependency boundaries

Once you have 2+ environments or 2+ products, Terragrunt typically reduces total complexity rather than adding it.
