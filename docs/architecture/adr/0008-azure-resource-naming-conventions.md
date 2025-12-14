# ADR-0008: Azure Resource Naming Conventions

## Status

**Accepted** - 2025-12-14

## Context

Azure resources require consistent naming to:

- Enable easy identification of resources
- Support automation and scripting
- Facilitate cost tracking and organization
- Ensure compliance with Azure naming restrictions
- Support multi-environment deployments (dev, staging, prod)
- Support multi-organization structure (nl, pvc, tws, mys)

Azure has specific naming constraints:

- Resource names must be globally unique for some resource types (storage accounts, Key Vaults)
- Length restrictions vary by resource type
- Some resources only allow lowercase alphanumeric characters and hyphens
- Some resources don't allow hyphens (e.g., storage accounts, ACR)

## Decision

We will adopt the following naming conventions for all Azure resources:

### General Principles

1. **Lowercase only**: All resource names use lowercase characters
2. **Hyphens for separation**: Use hyphens to separate words (where allowed)
3. **Environment first**: Include environment name (`dev`, `staging`, `prod`) as second segment for grouping
4. **Resource type suffix**: Include resource type abbreviation for clarity
5. **Component prefix**: Include component/service name for service-specific resources
6. **Shared resources**: Prefix with descriptive name for resources shared across services
7. **No dashes for restricted resources**: Some resources (storage accounts, ACR) cannot use dashes
8. **No org duplication**: Never repeat the org code inside project names

### Naming Pattern

```text
[org]-[env]-[project]-[type]-[region]
```

For resource groups specifically:

```text
[org]-[env]-[project]-rg-[region]
```

Where:

- `{org}`: Organization code (nl, pvc, tws, mys)
- `{env}`: Environment (dev, staging, prod)
- `{project}`: Project/service name (short, no org prefix duplication)
- `{type}`: Resource type abbreviation (app, api, func, db, etc.)
- `{region}`: Short region code (euw, san, saf, etc.)

**Why env before project?**

- Groups all environments together in alphabetical listings
- Allows quick visual scanning of "blast radius" (prod vs dev)
- Aligns with industry best practices (Microsoft, Azure Well-Architected Framework)
- Supports automated linting and AI parsing
- Makes cost analysis and environment separation easier

### Resource Type Abbreviations

| Resource Type           | Abbreviation | Example                               |
| ----------------------- | ------------ | ------------------------------------- |
| Resource Group          | `rg`         | `mys-dev-mystira-rg-euw`              |
| Azure Kubernetes        | `aks`        | `mys-dev-mystira-aks-euw`             |
| Virtual Network         | `vnet`       | `mys-dev-mystira-vnet-euw`            |
| Subnet                  | `subnet`     | `mystira-app-subnet` (no env prefix)  |
| Key Vault               | `kv`         | `mys-dev-mystira-kv-euw`              |
| Storage Account         | (no dashes)  | `mysprodmystirastgeuw`                |
| Container Registry      | (no dashes)  | `mysprodacr` (or `mystiraacr` legacy) |
| PostgreSQL              | `pg`         | `mys-dev-shared-pg-euw`               |
| Redis Cache             | (in name)    | `mys-dev-shared-redis-cache-euw`      |
| Application Insights    | `ai`         | `mys-dev-mystira-ai-euw`              |
| Log Analytics Workspace | `log`        | `mys-dev-mystira-log-euw`             |
| App Service             | `app`        | `mys-dev-mystira-app-euw`             |
| API                     | `api`        | `mys-dev-mystira-api-euw`             |
| Function App            | `func`       | `mys-dev-mystira-func-euw`            |
| Static Web App          | `swa`        | `mys-dev-mystira-swa-euw`             |
| Database                | `db`         | `mys-dev-mystira-db-euw`              |
| DNS                     | `dns`        | `mys-prod-mystira-dns-glob`           |

### Detailed Naming Patterns

#### Organization Codes (Ownership)

| Code | Organisation / Brand | Owner  | Purpose                                   |
| ---- | -------------------- | ------ | ----------------------------------------- |
| nl   | NeuralLiquid         | Jurie  | Core AI, defence, infra, Autopr, Rooivalk |
| pvc  | Phoenix VC           | Eben   | VC brand, investor tooling, market data   |
| tws  | Twines & Straps      | Martyn | E-commerce & ops, Tassa integrations      |
| mys  | Mystira              | Eben   | Mystira platform + Story generator        |

These codes are **authoritative** — NEVER invent new org codes.

#### Environment Codes

| Env     | Meaning             |
| ------- | ------------------- |
| dev     | Development         |
| staging | Pre-production / QA |
| prod    | Production          |

No additional values allowed without updating this document.

#### Project Codes (Per Org)

**NeuralLiquid (nl)**:

| Project  | Description                   |
| -------- | ----------------------------- |
| rooivalk | Counter-UAS platform          |
| autopr   | Autopr automation platform    |
| nl-core  | Shared NL foundation services |
| nl-ai    | Shared NL AI services         |

**Phoenix VC (pvc)**:

| Project | Description              |
| ------- | ------------------------ |
| website | Public brand website     |
| portal  | Investor portal (future) |
| mktdata | Market/crypto pipelines  |

**Twines & Straps (tws)**:

| Project    | Description                 |
| ---------- | --------------------------- |
| website    | Public e-commerce frontend  |
| backoffice | Internal management systems |
| tassa-int  | Tassa integration services  |

**Mystira (mys)**:

| Project       | Description                          |
| ------------- | ------------------------------------ |
| mystira       | Core storytelling/AI engine          |
| mystira-story | Dedicated Story Generator deployment |

**Important Rule**: Never duplicate the org code inside project names.

- ✅ `tws-prod-website-rg-san`
- ❌ `tws-prod-tws-website-rg-san`
- ✅ `pvc-prod-website-rg-euw`
- ❌ `pvc-prod-phoenixvc-website-rg-euw`

#### Region Codes

| Code | Azure Region           |
| ---- | ---------------------- |
| euw  | West Europe            |
| eun  | North Europe           |
| wus  | West US                |
| eus  | East US                |
| san  | South Africa North     |
| saf  | South Africa West      |
| swe  | Sweden (Central/North) |
| uks  | UK South               |
| usw  | US West (generic)      |
| glob | Global / regionless    |

**Note**: `saf` (South Africa West) exists but has limited service availability. Always verify service support before using it.

**Note on `glob`**: Use for regionless resources like DNS zones, Traffic Manager, Front Door, or Azure AD resources.

### Examples

#### Resource Groups

Pattern: `[org]-[env]-[project]-rg-[region]`

```text
mys-dev-mystira-rg-euw
mys-prod-mystira-rg-euw
mys-staging-mystira-rg-euw
nl-prod-autopr-rg-san
pvc-prod-mktdata-rg-euw
tws-prod-website-rg-san
```

#### App Services / APIs / Functions

Pattern: `[org]-[env]-[project]-[type]-[region]`

```text
mys-prod-mystira-api-euw
mys-dev-mystira-app-euw
mys-dev-mystira-func-euw
mys-prod-mystira-story-app-euw
nl-dev-autopr-app-san
pvc-prod-mktdata-func-euw
tws-prod-backoffice-api-san
```

#### DNS Resources

DNS zones are typically global, so use `glob` as region:

```text
mys-prod-mystira-dns-glob
nl-prod-autopr-dns-glob
pvc-prod-website-dns-glob
```

Resource group for DNS:

```text
mys-prod-mystira-rg-glob
nl-prod-autopr-rg-glob
pvc-prod-website-rg-glob
```

#### Storage Accounts

Pattern: `[org][env][project][description]` (no dashes, lowercase only)

Storage accounts must be globally unique, 3-24 characters, lowercase alphanumeric only.

Examples:

```text
mysprodmystirastgeuw    # mys-prod-mystira-storage-euw
mysprodterraformstate   # Terraform state backend
nlprodautoprstgeuw      # nl-prod-autopr-storage-euw
pvcprodmktdatastgeuw    # pvc-prod-mktdata-storage-euw
```

#### Container Registry (ACR)

Pattern: `[org][description]` (no dashes, shared across environments)

**Note**: ACR names cannot contain hyphens and must be globally unique. Existing ACRs (e.g., `mystiraacr`) should be kept as-is unless a full migration is planned.

Examples:

```text
nlprodacr    # Shared ACR for NL (uses tags: dev, staging, prod)
pvcprodacr   # Shared ACR for PVC
mysprodacr   # Shared ACR for Mystira (or keep existing: mystiraacr)
```

**Legacy Exception**: The existing `mystiraacr` is kept as-is due to migration complexity (all images would need to be re-tagged and all deployments updated).

### Kubernetes Resources

**Namespaces**: `{org}-{env}`

Examples:

- `mys-dev`, `mys-staging`, `mys-prod` (Mystira - new convention)
- `mystira-dev`, `mystira-staging`, `mystira-prod` (Mystira - legacy, keep as-is)
- `nl-dev`, `nl-staging`, `nl-prod` (NeuralLiquid)

**Deployments/StatefulSets**: `{org}-{component}`

Examples:

- `mys-chain`, `mys-publisher`, `mys-story-generator` (Mystira - new convention)
- `mystira-chain`, `mystira-publisher`, `mystira-story-generator` (Mystira - legacy, keep as-is)
- `nl-autopr`, `nl-rooivalk` (NeuralLiquid)

**Services**: `{org}-{component}`

Examples:

- `mys-chain`, `mys-publisher`, `mys-story-generator` (Mystira - new convention)
- `mystira-chain`, `mystira-publisher`, `mystira-story-generator` (Mystira - legacy, keep as-is)
- `nl-autopr`, `nl-rooivalk` (NeuralLiquid)

**ConfigMaps/Secrets**: `{org}-{component}-{purpose}`

Examples:

- `mys-publisher-config`, `mys-story-generator-secrets` (Mystira - new convention)
- `mystira-publisher-config`, `mystira-story-generator-secrets` (Mystira - legacy, keep as-is)
- `nl-autopr-config`, `nl-rooivalk-secrets` (NeuralLiquid)

**Note**: Existing Kubernetes resources using `mystira-{component}` naming should be kept as-is. New resources should use `mys-{component}` per the new convention. Migration can occur during cluster upgrades or namespace consolidation.

### DNS and URLs

**Pattern**: `{component}.{env}.{domain}` or `{component}.{domain}` (prod)

**Examples**:

- `mystira.dev.mystira.app` (dev)
- `mystira.staging.mystira.app` (staging)
- `mystira.mystira.app` (prod)
- `mktdata.staging.phoenixvc.tech` (staging)
- `website.phoenixvc.tech` (prod)

### Terraform State Backend

**Storage Account**: `{org}prodterraformstate` (no dashes, lowercase only)

Examples:

- `nlprodterraformstate` (NeuralLiquid)
- `mysprodterraformstate` (Mystira - new)
- `mystiraterraformstate` (Mystira - legacy, keep as-is)

**Container**: `tfstate`
**Resource Group**: `{org}-prod-terraform-rg-{region}` (for state storage RG only)

Examples:

- `nl-prod-terraform-rg-euw`
- `mys-prod-terraform-rg-euw`
- `mystira-terraform-state` (legacy, keep as-is)

**State File Keys**: `{env}/terraform.tfstate`

- `dev/terraform.tfstate`
- `staging/terraform.tfstate`
- `prod/terraform.tfstate`

**Note**: The existing `mystiraterraformstate` storage account and `mystira-terraform-state` resource group should be kept as-is unless a full state migration is planned.

## Renaming, Moving, and Recreating Resources

Most Azure resource names are **immutable**.

### Three Concepts

1. **Rename** (change the name itself) → Generally **not supported** for core resources
2. **Move** (change RG/subscription) → **Often supported**, but doesn't fix naming
3. **Recreate/Migrate** → How you "rename" in practice

### Capabilities Matrix

| Resource Type               | Rename Name? | Move RG/Sub? | Typical Action to Fix Naming          |
| --------------------------- | ------------ | ------------ | ------------------------------------- |
| Resource Group              | ❌ No        | n/a          | New RG + move resources if supported  |
| App Service                 | ❌ No        | ✅ Often     | New app + DNS/traffic cutover         |
| Function App                | ❌ No        | ✅ Often     | New app + config migration            |
| Static Web App              | ❌ No        | ⚠ Limited    | New SWA + rebind domains              |
| Storage Account             | ❌ No        | ⚠ Limited    | New account + data migration          |
| SQL / DB / Managed Instance | ❌ No        | ⚠ Depends    | New server/db + data migration        |
| Key Vault                   | ❌ No        | ⚠ Limited    | New vault + re-seed secrets           |
| VNet / Subnet               | ❌ No        | ⚠ Limited    | New VNet/Subnets + reattach resources |
| DNS Zone                    | ❌ No        | ⚠ Limited    | New zone (domain = name)              |
| Log Analytics Workspace     | ❌ No        | ✅ Often     | New workspace + update diagnostics    |
| Dashboards / Workbooks      | ✅ Yes       | n/a          | Rename in portal                      |
| Tags                        | ✅ Yes       | n/a          | Edit freely                           |

**Always check latest Azure documentation before planning large migrations.**

## Resource Discovery & Inventory

### Azure CLI Commands

List all resource groups:

```bash
az group list --query "[].{name:name, location:location}" -o table
```

List all resources:

```bash
az resource list \
  --query "[].{name:name, type:type, rg:resourceGroup, location:location}" \
  -o table
```

List resources in a resource group:

```bash
az resource list -g <resource-group-name> \
  --query "[].{name:name, type:type, location:location}" -o table
```

Filter by resource type:

```bash
# App Services
az resource list --resource-type "Microsoft.Web/sites" -o table

# Function Apps
az resource list --resource-type "Microsoft.Web/sites" \
  --query "[?kind=='functionapp']" -o table

# Storage Accounts
az storage account list \
  --query "[].{name:name, rg:resourceGroup, location:location}" -o table
```

Group resources by type:

```bash
az resource list \
  --query "sort_by([].{name:name, type:type, rg:resourceGroup}, &type)" \
  -o table
```

Export to JSON:

```bash
az resource list -o json > all-resources.json
```

### Azure Resource Graph Queries

Find non-compliant names:

```kusto
Resources
| extend nameParts = split(name, '-')
| extend partCount = array_length(nameParts)
| extend org = tostring(nameParts[0])
| extend env = tostring(nameParts[1])
| where org !in~ ('nl','pvc','tws','mys')
   or env !in~ ('dev','staging','prod')
   or partCount < 4
| project name, type, resourceGroup, location, org, env, partCount
| order by name asc
```

## AI Guidance (for LLMs / Tools)

AI systems working with this Azure environment MUST:

1. **Treat names as structured identifiers**: `[org]-[env]-[project]-[type]-[region]` is not a free-form string
2. **Never invent new vocabularies**: Only use org/env/project/type/region codes defined in this ADR
3. **Select org based on ownership**:
   - nl → Jurie
   - pvc → Eben
   - tws → Martyn
   - mys → Eben
4. **Never duplicate org in project names**: `website` not `tws-website` when org is `tws`
5. **Assume names are immutable**: To "rename", propose new resource + migration, NOT in-place rename
6. **Derive names from parameters**: In IaC (Bicep/Terraform), always derive names from org/env/project/type/region parameters
7. **Prefer existing project names**: Reuse project codes instead of creating near-duplicates
8. **Validate region support**: Check service availability before suggesting regional deployments

### AI Usage Summary (Copy-Paste Block)

```text
Use [org]-[env]-[project]-[type]-[region] for resources.
Use [org]-[env]-[project]-rg-[region] for resource groups.

Valid org codes: nl, pvc, tws, mys.
Valid env codes: dev, staging, prod.
Valid type/region codes: MUST match tables in ADR-0008.

Never invent new org/env/project/type/region codes.
Assume names are immutable → use migrations, not renames.
All IaC must derive names from org/env/project/type/region parameters.
Never duplicate org code inside project names.
```

## Bicep Naming Module

Create `infrastructure/modules/naming.bicep`:

```bicep
@description('Owning organisation code')
@allowed([
  'nl'
  'pvc'
  'tws'
  'mys'
])
param org string

@description('Deployment environment')
@allowed([
  'dev'
  'staging'
  'prod'
])
param env string

@description('Logical project / system name')
param project string

@description('Short region code')
@allowed([
  'euw'
  'eun'
  'wus'
  'eus'
  'san'
  'saf'
  'swe'
  'uks'
  'usw'
  'glob'
])
param region string

var base = '${org}-${env}-${project}'

// Resource group
output rgName string = '${base}-rg-${region}'

// Common resource names
output name_app string     = '${base}-app-${region}'
output name_api string     = '${base}-api-${region}'
output name_func string    = '${base}-func-${region}'
output name_swa string     = '${base}-swa-${region}'
output name_db string      = '${base}-db-${region}'
output name_storage string = '${base}-storage-${region}'
output name_kv string      = '${base}-kv-${region}'
output name_ai string      = '${base}-ai-${region}'
output name_dns string     = '${base}-dns-${region}'
output name_log string     = '${base}-log-${region}'
output name_aks string     = '${base}-aks-${region}'
output name_vnet string    = '${base}-vnet-${region}'
```

Usage:

```bicep
module naming './modules/naming.bicep' = {
  name: 'naming-autopr-prod-san'
  params: {
    org: 'nl'
    env: 'prod'
    project: 'autopr'
    region: 'san'
  }
}

resource app 'Microsoft.Web/sites@2022-03-01' = {
  name: naming.outputs.name_app
  location: 'southafricanorth'
  properties: {
    serverFarmId: appServicePlan.id
  }
}
```

## Migration Strategy

For existing resources that don't follow this convention:

1. **Document current names**: List all existing resources and their current names
2. **Plan migration**: Identify resources that need renaming
3. **Execute during maintenance windows**: Migrate resources during planned maintenance
4. **Update documentation**: Update all references to resource names
5. **Update CI/CD**: Update any scripts or workflows that reference resource names

**Note**: Most resources cannot be renamed and must be recreated. Plan migrations carefully using the capabilities matrix above.

**See the comprehensive "Legacy Resource Mapping (Existing → New Convention)" section above for detailed mapping of all existing Mystira resources.**

## Implementation

### Terraform Conventions

In Terraform modules, use `locals` blocks to define naming patterns:

```hcl
locals {
  name_prefix = "${var.org}-${var.env}-${var.project}"
  resource_group_name = "${var.org}-${var.env}-${var.project}-rg-${var.region}"
  common_tags = {
    Environment = var.env
    Project     = "Mystira"
    ManagedBy   = "terraform"
    Org         = var.org
  }
}
```

### Consistency Checks

1. All resource names should follow these patterns
2. Review Terraform code for consistency
3. Document deviations with rationale
4. Update existing resources during migration/refactoring
5. Use automated linting in CI/CD

## Consequences

### Positive

- ✅ Easy to identify resources by environment and component
- ✅ Supports automation and scripting
- ✅ Clear resource ownership and purpose
- ✅ Easier cost tracking and organization
- ✅ Consistent across all environments
- ✅ Aligns with Azure best practices
- ✅ Environment-first grouping improves readability
- ✅ Supports AI tooling and automated validation

### Negative

- ⚠️ Some existing resources may not follow this pattern (migration needed)
- ⚠️ Storage accounts and ACR have restrictions (no dashes)
- ⚠️ Must ensure globally unique names where required
- ⚠️ Requires discipline to maintain project name vocabulary

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0007: NuGet Feed Strategy](./0007-nuget-feed-strategy-for-shared-libraries.md)

## References

- [Azure naming conventions](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming)
- [Azure resource naming restrictions](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules)
- [Terraform Azure Provider documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Well-Architected Framework](https://learn.microsoft.com/en-us/azure/architecture/framework/)
