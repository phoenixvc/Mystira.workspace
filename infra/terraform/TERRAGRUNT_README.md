# Terragrunt Configuration

## Overview

This directory uses [Terragrunt](https://terragrunt.gruntwork.io/) to manage Terraform configurations across multiple products and environments with:

- **Separate state files** per product/environment
- **DRY configuration** through inheritance
- **Dependency management** between products
- **Parallel deployments** for independent products

## Directory Structure

```
infra/terraform/
├── terragrunt.hcl              # Root config (common settings)
├── MIGRATION_PLAN.md           # Migration guide
├── shared-infra/               # Layer 0: Shared resources
│   ├── terragrunt.hcl
│   └── environments/
│       ├── dev/
│       │   ├── terragrunt.hcl
│       │   └── main.tf
│       ├── staging/
│       └── prod/
├── products/                   # Layer 1: Applications
│   ├── mystira-app/
│   ├── story-generator/
│   ├── admin/
│   ├── publisher/
│   └── chain/
└── modules/                    # Reusable Terraform modules
```

## Prerequisites

1. **Install Terragrunt**:

   ```bash
   # macOS
   brew install terragrunt

   # Linux
   curl -L https://github.com/gruntwork-io/terragrunt/releases/download/v0.55.0/terragrunt_linux_amd64 \
     -o /usr/local/bin/terragrunt && chmod +x /usr/local/bin/terragrunt

   # Or use asdf
   asdf plugin add terragrunt
   asdf install terragrunt latest
   ```

2. **Azure CLI authenticated**:

   ```bash
   az login
   az account set --subscription "Your-Subscription"
   ```

3. **Environment variables**:
   ```bash
   export ARM_SUBSCRIPTION_ID="your-subscription-id"
   export ARM_TENANT_ID="your-tenant-id"
   export TF_VAR_environment="dev"  # or staging, prod
   ```

## Usage

### Deploy Shared Infrastructure (Required First)

```bash
cd infra/terraform/shared-infra/environments/dev
terragrunt plan
terragrunt apply
```

### Deploy a Product

```bash
cd infra/terraform/products/mystira-app/environments/dev
terragrunt plan
terragrunt apply
```

### Production Safety

Prod `apply` and `destroy` are blocked by default.

```bash
export ALLOW_PROD_APPLY=true
cd infra/terraform/shared-infra/environments/prod
terragrunt apply
```

### Deploy All Products (Respects Dependencies)

```bash
cd infra/terraform
terragrunt run-all plan
terragrunt run-all apply
```

### Deploy Only Changed Products

```bash
# Use --terragrunt-include-dir to scope
terragrunt run-all apply --terragrunt-include-dir "products/mystira-app/**"
```

## Dependency Graph

```
shared-infra (Layer 0)
    │
    ├── mystira-app
    ├── story-generator
    ├── admin (admin-api + admin-ui)
    ├── publisher
    └── chain
```

All products depend on `shared-infra`. Products are independent of each other and can be deployed in parallel.

## State Files

Each product/environment gets a separate state file in Azure Blob Storage:

| Product         | State Key                     |
| --------------- | ----------------------------- |
| shared-infra    | `shared-infra/dev.tfstate`    |
| mystira-app     | `mystira-app/dev.tfstate`     |
| story-generator | `story-generator/dev.tfstate` |
| admin           | `admin/dev.tfstate`           |
| publisher       | `publisher/dev.tfstate`       |
| chain           | `chain/dev.tfstate`           |

## Common Commands

```bash
# Format all Terraform files
terragrunt hclfmt

# Validate configuration
terragrunt validate

# Show dependency graph
terragrunt graph-dependencies

# Destroy a specific product (careful!)
cd products/mystira-app/environments/dev
terragrunt destroy

# Destroy everything in reverse dependency order
terragrunt run-all destroy
```

## CI/CD Integration

Example GitHub Actions workflow:

```yaml
name: Terraform Deploy

on:
  push:
    branches: [main]
    paths:
      - "infra/terraform/**"

jobs:
  deploy:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        product: [shared-infra, mystira-app, story-generator, admin]

    steps:
      - uses: actions/checkout@v4

      - name: Setup Terraform
        uses: hashicorp/setup-terraform@v3
        with:
          terraform_version: 1.5.0

      - name: Setup Terragrunt
        run: |
          curl -L https://github.com/gruntwork-io/terragrunt/releases/latest/download/terragrunt_linux_amd64 \
            -o /usr/local/bin/terragrunt
          chmod +x /usr/local/bin/terragrunt

      - name: Terragrunt Apply
        working-directory: infra/terraform/${{ matrix.product }}/environments/dev
        run: terragrunt apply -auto-approve
        env:
          ARM_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          ARM_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
          ARM_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
          ARM_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
```

## Troubleshooting

### "Backend configuration changed"

```bash
# Reinitialize with migration
terragrunt init -migrate-state
```

### "Dependency not found"

```bash
# Deploy shared-infra first
cd shared-infra/environments/dev
terragrunt apply
```

### "State lock held"

```bash
# Force unlock (use with caution!)
terragrunt force-unlock LOCK_ID
```

## Migration from Legacy Structure

See [MIGRATION_PLAN.md](./MIGRATION_PLAN.md) for detailed migration steps from the monolithic structure.
