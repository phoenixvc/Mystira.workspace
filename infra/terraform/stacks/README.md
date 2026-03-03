# Terragrunt Stacks

Terragrunt Stacks enable deploying the complete Mystira platform as a cohesive unit with proper dependency ordering.

## Overview

The Mystira platform is organized into **stacks** - collections of Terragrunt units that are deployed together:

```
┌─────────────────────────────────────────────────────────────────┐
│                     Mystira Platform Stack                       │
├─────────────────────────────────────────────────────────────────┤
│  Layer 1: Application Products (parallel deployment)             │
│  ┌─────────┐ ┌─────────┐ ┌───────────┐ ┌─────────┐ ┌──────────┐ │
│  │  admin  │ │  chain  │ │mystira-app│ │publisher│ │story-gen │ │
│  └────┬────┘ └────┬────┘ └─────┬─────┘ └────┬────┘ └────┬─────┘ │
│       │           │            │            │           │        │
│       └───────────┴────────────┼────────────┴───────────┘        │
│                                │                                  │
│  ┌─────────────────────────────▼─────────────────────────────┐   │
│  │              Layer 0: shared-infra (foundation)            │   │
│  │  PostgreSQL, Redis, Cosmos DB, Service Bus, Monitoring    │   │
│  └────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Stack Files

| File                   | Description                                          |
| ---------------------- | ---------------------------------------------------- |
| `terragrunt.stack.hcl` | Base stack configuration (uses `TF_VAR_environment`) |
| `dev.stack.hcl`        | Development environment stack                        |
| `staging.stack.hcl`    | Staging environment stack                            |
| `prod.stack.hcl`       | Production environment stack                         |

## Usage

### Deploy Full Platform

```bash
cd infra/terraform

# Development
terragrunt stack run plan --stack-config stacks/dev.stack.hcl
terragrunt stack run apply --stack-config stacks/dev.stack.hcl

# Staging
terragrunt stack run plan --stack-config stacks/staging.stack.hcl
terragrunt stack run apply --stack-config stacks/staging.stack.hcl

# Production (requires approval)
terragrunt stack run plan --stack-config stacks/prod.stack.hcl
terragrunt stack run apply --stack-config stacks/prod.stack.hcl
```

### Deploy Specific Units

```bash
# Deploy only shared infrastructure
terragrunt stack run plan --stack-config stacks/dev.stack.hcl --target shared-infra

# Deploy specific product
terragrunt stack run plan --stack-config stacks/dev.stack.hcl --target admin
terragrunt stack run apply --stack-config stacks/dev.stack.hcl --target mystira-app

# Deploy multiple products
terragrunt stack run plan --stack-config stacks/dev.stack.hcl --target admin --target chain
```

### Using Environment Variable

```bash
# Alternative: Use TF_VAR_environment with base stack
export TF_VAR_environment=staging
terragrunt stack run plan --stack-config stacks/terragrunt.stack.hcl
```

## Dependency Management

Stacks automatically handle dependencies:

1. **shared-infra** is always deployed first (no dependencies)
2. **Products** wait for shared-infra to complete
3. **Products** can deploy in parallel (no inter-product dependencies)

### Mock Outputs

When planning without deployed dependencies, stacks use mock outputs to allow validation:

```hcl
dependency {
  config       = unit.shared-infra
  skip_outputs = true  # Use mocks during plan
}
```

## CI/CD Integration

### GitHub Actions Workflow

```yaml
- name: Plan Platform Stack
  run: |
    terragrunt stack run plan \
      --stack-config stacks/${{ inputs.environment }}.stack.hcl \
      --out=platform.tfplan

- name: Apply Platform Stack
  if: github.event_name == 'push' && github.ref == 'refs/heads/main'
  run: |
    terragrunt stack run apply \
      --stack-config stacks/${{ inputs.environment }}.stack.hcl \
      --auto-approve
```

### Atlantis Integration

Stacks work with Atlantis for PR-based reviews. See `atlantis.yaml` in repository root.

## Environment Configuration

Each environment stack defines its own configuration:

| Setting            | Dev             | Staging            | Prod               |
| ------------------ | --------------- | ------------------ | ------------------ |
| PostgreSQL SKU     | B_Standard_B1ms | GP_Standard_D2s_v3 | GP_Standard_D4s_v3 |
| PostgreSQL Storage | 32GB            | 64GB               | 256GB              |
| Redis SKU          | Basic           | Standard           | Premium            |
| Min Replicas       | 1               | 1                  | 2                  |
| Max Replicas       | 2               | 3                  | 10                 |
| Log Retention      | 30 days         | 60 days            | 90 days            |
| HA Enabled         | No              | No                 | Yes                |
| Zone Redundant     | No              | No                 | Yes                |

## Troubleshooting

### Lock Contention

If a stack deployment is stuck due to a stale lock:

```bash
# Force unlock (use with caution)
cd infra/terraform/shared-infra/environments/dev
terragrunt force-unlock <LOCK_ID>
```

### Dependency Failures

If a unit fails due to missing dependency outputs:

```bash
# 1. Deploy shared-infra first
terragrunt stack run apply --stack-config stacks/dev.stack.hcl --target shared-infra

# 2. Then deploy products
terragrunt stack run apply --stack-config stacks/dev.stack.hcl
```

### Partial Failures

If some units fail during apply:

```bash
# Re-run plan to see current state
terragrunt stack run plan --stack-config stacks/dev.stack.hcl

# Apply only failed units
terragrunt stack run apply --stack-config stacks/dev.stack.hcl --target <failed-unit>
```

## Best Practices

1. **Always plan before apply** - Review changes before deployment
2. **Use environment-specific stacks** - Avoid mixing environments
3. **Deploy shared-infra first** - Ensure dependencies exist
4. **Monitor during production deploys** - Watch Application Insights
5. **Use Atlantis for PRs** - Enable team review of infrastructure changes
