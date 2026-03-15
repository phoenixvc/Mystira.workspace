# PVC CostOps Migration Plan (Import-First, No Surprises)

This plan is for bringing the PVC CostOps infrastructure under Terraform (optionally with Terragrunt), without creating duplicates and without surprises. You can move this document into the PVC repo; it’s written to stand alone.

## Current footprint (observed)

From Azure resources currently visible:

- `pvc-costops-dce-san` (Data Collection Endpoint)
- `pvc-costops-dcr-actionlog-san` (Data Collection Rule)
- `pvc-costops-dcr-focus-san` (Data Collection Rule)
- `pvc-shared-costops-law-san` (Log Analytics Workspace)
- `pvc-shared-costops-api-san` (App Service / Web App)
- `pvc-shared-costops-api-mi-san` (Managed Identity)
- `pvc-shared-costops-cae-san` (Container Apps Environment)
- `pvc-shared-costops-mi-san` (Managed Identity)

The names imply SAN region scope and a “shared costops” foundation with app-specific pieces.

## Goals

- Adopt IaC (Terraform; optionally Terragrunt) with state + locking.
- If resources already exist, import them into state (no “apply into empty state”).
- If this is greenfield, create from code first and avoid portal drift.
- Make cost control explicit (retention, ingestion rules, environments).

## Shared resource conventions

If PVC CostOps will share platform resources (or wants to stay compatible with Mystira-style shared-infra), follow the shared contract and naming conventions here:

- [shared-resource-adoption-conventions.md](./shared-resource-adoption-conventions.md)

## Non-goals

- Redesigning networking/edge (do that as a separate phase).
- Replacing the application runtime (App Service vs Container Apps) during state adoption.

## Guiding rules (the important bits)

- If it exists already: import it before any apply.
- If it’s truly new: create it from code first.
- No deletes: do not destroy data/monitoring resources during the first migration.
- One change axis at a time: state adoption first, refactors after.
- Treat prod as sacred: gated apply (manual approvals and/or env guard).

## If this is a brand-new app (recommended path)

If there is no existing production traffic and the Azure resources are either not created yet or safe to recreate, use this simplified path:

1. Create remote state + locking (bootstrap).
2. Create Terraform/Terragrunt structure for dev/staging/prod.
3. Run `plan` then `apply` for dev first.
4. Promote the same code to staging, then prod (with approvals).

Only use imports if:

- you already created foundational resources manually, or
- the pipeline created them outside Terraform.

## Decision: Terraform only vs Terragrunt

Recommended default for multi-env or multi-service:

- Terragrunt if you expect: dev + staging + prod, or multiple components, or you want guardrails and dependency wiring.
- Terraform only if: one environment, low churn, and one person owns infra.

If you don’t want to adopt Terragrunt yet, you can still follow this plan with Terraform only (same import-first process).

## Phase 0: Inventory and freeze

1. Confirm subscription/tenant and identify what is “prod” for PVC CostOps.

2. Export an inventory snapshot (save into the repo as an artifact):

```bash
az group list --query "[?starts_with(name,'pvc-')].{name:name,location:location}" -o table
az resource list -g <RG_NAME> --query "[].{name:name,type:type,id:id}" -o table
```

3. Capture current runtime configuration:

```bash
az webapp show -g <RG_NAME> -n pvc-shared-costops-api-san -o json
az webapp config appsettings list -g <RG_NAME> -n pvc-shared-costops-api-san -o json
az webapp config connection-string list -g <RG_NAME> -n pvc-shared-costops-api-san -o json
```

4. Put a temporary freeze on ad-hoc portal changes while importing.

## Phase 1: Choose a minimal target structure

Keep it boring at first:

- `shared` layer: monitoring + identities (and possibly DCE/DCR if shared)
- `app` layer: the web app and its app-specific configuration

If using Terragrunt, use:

```text
infra/terraform/shared-infra/environments/<env>
infra/terraform/products/costops/environments/<env>
```

If Terraform-only, keep:

```text
infra/terraform/environments/<env>
```

## Phase 2: Bootstrap remote state (once)

Use an Azure Storage Account + container for state with Azure AD auth and locking. Do this once and don’t change it lightly.

Minimum requirements:

- Storage account for state
- Container (e.g., `tfstate`)
- A consistent naming convention for state keys

Example key scheme:

- `shared-infra/prod.tfstate`
- `costops/prod.tfstate`

## Phase 3: Write Terraform for existing resources (skeleton first)

Create Terraform resources that match what exists in Azure today:

- Log Analytics Workspace
- Data Collection Endpoint + Data Collection Rules
- Managed identities and role assignments
- App Service Plan (if applicable) + Web App
- Container Apps Environment (if it is actually used)

Do not refactor structure or rename resources yet. Just represent them.

## Phase 4: Import existing resources into state

For each environment:

1. Run `plan` and confirm it wants to create resources you already have.
2. Import those resources into the correct addresses in state.
3. Re-run `plan` until it’s either no-op or only shows intentional diffs.

Import examples (pattern only; use real IDs):

```bash
terraform import azurerm_log_analytics_workspace.main "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.OperationalInsights/workspaces/pvc-shared-costops-law-san"
terraform import azurerm_monitor_data_collection_endpoint.main "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.Insights/dataCollectionEndpoints/pvc-costops-dce-san"
terraform import azurerm_monitor_data_collection_rule.actionlog "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.Insights/dataCollectionRules/pvc-costops-dcr-actionlog-san"
terraform import azurerm_monitor_data_collection_rule.focus "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.Insights/dataCollectionRules/pvc-costops-dcr-focus-san"
terraform import azurerm_user_assigned_identity.api "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.ManagedIdentity/userAssignedIdentities/pvc-shared-costops-api-mi-san"
terraform import azurerm_linux_web_app.api "/subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.Web/sites/pvc-shared-costops-api-san"
```

## Phase 5: Lock down prod safety

Add safety rails:

- Require explicit opt-in for prod apply (env var or pipeline parameter)
- Require manual approvals for prod apply
- Use drift detection (plan-only scheduled pipeline)

Suggested guard:

- Block prod apply unless a pipeline parameter like `ALLOW_PROD_APPLY=true` is set.

## Phase 6: CostOps-specific “cost controls”

Do these only after state adoption is stable:

- Log Analytics retention: set to a known value (shorter in non-prod).
- Data collection rules:
  - Remove noisy tables/streams
  - Sample or reduce collection rate where possible
  - Ensure only required sources are collected

Observability hygiene:

- Tag everything (`Environment`, `Service`, `Owner`, `CostCenter`).
- Prefer dashboards/alerts that filter by tags and resource groups.

## Phase 7: Optional architecture convergence (App Service vs Container Apps)

It looks like both an App Service (`pvc-shared-costops-api-san`) and a Container Apps Environment (`pvc-shared-costops-cae-san`) exist.

After imports are clean:

- Decide what is actually in use.
- If CAE is unused, keep it managed but do not delete until you verify (1–2 weeks).
- If moving from App Service → Container Apps, treat it as a separate migration with a cutover plan:
  - Deploy parallel runtime
  - Validate
  - Switch traffic (DNS/Front Door/ingress)
  - Decommission later

## Validation gates

- `plan` is clean (no unexpected creates/destroys)
- App health checks pass after any apply
- Log ingestion still flows to `pvc-shared-costops-law-san`
- No changes to DCE/DCR that reduce required telemetry without intent

## Rollback strategy

- State-only rollback: revert Terraform code; do not destroy Azure resources.
- Runtime rollback: restore previous app settings and identity assignments.
