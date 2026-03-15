# ai-gateway Integration Plan (Shared-Infra Adoption)

Goal: onboard `phoenixvc/ai-gateway` as a Mystira product that consumes existing shared infrastructure (per environment) instead of creating its own duplicates.

Repo: https://github.com/phoenixvc/ai-gateway

## Notes from repo

- Deployment target: Azure Container Apps, OpenAI-compatible gateway (LiteLLM).
- Environments in repo: `dev`, `uat`, `prod` (map `uat` ↔ `staging` in Mystira).
- Key inputs:
  - `AZURE_OPENAI_ENDPOINT` (points at shared Azure OpenAI / AI Foundry endpoint)
  - `AZURE_OPENAI_API_KEY` (secret)
  - `AIGATEWAY_KEY` (gateway auth key; secret)
  - OIDC auth variables (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`)

## Shared resources to consume (per environment)

Follow the shared contract in [shared-resource-adoption-conventions.md](./shared-resource-adoption-conventions.md):

- Monitoring:
  - Log Analytics workspace (preferred)
  - Application Insights connection string (optional; per-service AI still OK)
- Key Vault:
  - Store all provider keys/connection strings; apps use managed identity to read
- Optional shared data-plane dependencies (only if ai-gateway needs them):
  - Redis (rate limits / caching)
  - Storage (request/response archives, prompt artifacts)

## Target deployment shape

- Compute: Azure Container Apps (ACA) per environment.
- Identity: user-assigned managed identity per environment for the ACA app.
- Secrets: Key Vault references or runtime fetch from Key Vault using managed identity.
- Networking:
  - Start with public ingress (to match current Mystira “pre-seed first” posture).
  - Add private ingress/VNet integration only after shared networking plan completes.

## Terraform/Terragrunt integration steps

1. Add a new product stack under `infra/terraform/products/ai-gateway/`.
   - Mirror the pattern used by `products/story-generator` and `products/admin`.
2. Wire shared-infra dependency inputs from `dependency.shared.outputs.*`.
   - Prefer connection-string style inputs where possible.
3. Create an `ai-gateway` module that provisions:
   - Container App + Container App Environment (if not already shared)
   - User-assigned identity
   - Role assignments (Key Vault Secrets User; Log Analytics ingest if needed)
   - Optional Application Insights (or shared) and diagnostic settings to Log Analytics
4. Ensure secrets are never output from Terraform.
   - Write secrets into Key Vault only, and reference by name/URI.
5. Add the product to Terragrunt orchestration (stack file) so `/plan` includes it.

## CI/CD and rollout

- CI:
  - Add a plan/validate target for `ai-gateway` alongside other products.
- CD:
  - Use the existing Mystira container build/publish conventions (ACR, tags).
  - Deploy with staged environments: dev → staging → prod.

## Open questions (to resolve before wiring)

- Does ai-gateway need Redis/Storage, or only monitoring + Key Vault?
- Does it require strict isolation (dedicated Redis/AI) or is shared-per-env acceptable initially?
- Does it need a stable public hostname (Front Door) or direct ACA ingress is sufficient initially?
- Do we standardize on `staging` naming, or keep `uat` as an alias for this product?
