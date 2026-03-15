# Infra Edge + Networking Plan (Planned Separately)

This plan covers the items explicitly excluded from the “shared-infra + products” consolidation work, because changing them has higher disruption risk and tends to impact routing, certificates, and downtime windows.

## Scope

- DNS zone management and record automation
- Front Door / WAF strategy (non-prod shared vs prod dedicated)
- Networking (VNet, subnets, AKS network model)
- Ingress (NGINX, cert-manager, external IP lifecycle)
- Cluster sizing and autoscaling policy (AKS)

## Goals

- Remove operational dependence on legacy Terraform entrypoints without breaking routing.
- Keep external domains stable while internal infra is refactored.
- Make edge/networking reusable across products and repos when desired, without forcing it.

## Minimal-Disruption Phases

### Phase 1: Decouple legacy entrypoints

- Ensure product deployments run via Terragrunt entrypoints:
  - `infra/terraform/shared-infra/environments/<env>`
  - `infra/terraform/products/<product>/environments/<env>`
- Leave DNS and Front Door managed by existing workflows temporarily.

### Phase 2: DNS ownership model

- Decide on one of:
  - Domain-per-product (separate zones / delegations)
  - Shared zone with strict record ownership conventions (recommended if all products share `mystira.app`)
- Implement record ownership boundaries:
  - `dev.*`, `staging.*`, `prod.*` (environment prefixes)
  - product subdomains: `publisher`, `admin`, `story`, etc.

### Phase 3: Front Door / WAF migration

- Keep current model:
  - Non-prod shared Front Door (dev + staging)
  - Prod Front Door separate
- Move Front Door resources to a dedicated “edge” stack (Terragrunt):
  - `infra/terraform/edge/environments/<env>` (or `shared-infra-edge/` if you prefer)
- Ensure outputs drive DNS records (validation tokens, endpoint hostnames).

### Phase 4: Networking + AKS consolidation

- Make VNet/subnet definitions their own stack (separate from shared-infra data services).
- Keep AKS cluster as a separate stack from the network (to reduce blast radius).
- Standardize:
  - subnet IP ranges
  - private endpoint policy (if enabled later)
  - workload identity / OIDC configuration

### Phase 5: Ingress and cert lifecycle

- Manage ingress IP lifecycle explicitly:
  - stable static public IP per env
  - cert-manager issuer + renewal monitoring
- Make DNS records depend on ingress IP outputs.

### Phase 6: Cost + capacity policy

- Define environment sizing profiles (dev/staging/prod):
  - node pools, autoscale bounds
  - SKU policy
  - budget guardrails

## Cross-Product / Cross-Repo Reuse Notes

- DNS:
  - Shareable if products use the same apex domain and you accept shared governance.
  - Otherwise delegate subzones per product (`rooivalk.mystira.app`, `chauffeur.mystira.app`).
- Front Door/WAF:
  - Very shareable across products when domains are under one umbrella, but increases blast radius.
- Networking/AKS:
  - Generally not shared across unrelated products unless you want a single platform cluster (not recommended initially).
- Ingress:
  - Can be shared per cluster; if clusters are separate, ingress is separate.
