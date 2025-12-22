# Azure Container Registry (ACR) Strategy

## Current Issue

**ACR Configuration** (per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md)):

1. **Terraform**: ACR `myssharedacr` is created **only in the `dev` environment** (see `infra/terraform/environments/dev/main.tf:127`)
2. **CI/CD Workflows**: All workflows push to `myssharedacr` (expecting it to exist)
3. **Kubernetes Overlays**: Environment overlays use shared ACR with environment tags
   - All environments use: `myssharedacr.azurecr.io` with tags: `dev`, `staging`, `prod`

**Result**: The ACR is shared across all environments with environment-specific tags for image organization.

## Recommendation: Shared ACR

**Best Practice**: Use a **single shared ACR** across all environments.

### Benefits

1. ✅ **Simpler CI/CD**: One registry to manage
2. ✅ **Cost Efficient**: One ACR instead of three
3. ✅ **Image Reusability**: Same image can be promoted between environments
4. ✅ **Simpler Access Control**: One set of credentials
5. ✅ **Consistent Naming**: Workflows don't need environment-specific logic

### How to Separate Images

Use **tags** for environment separation (per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md)):

```yaml
# Development images
myssharedacr.azurecr.io/chain:dev
myssharedacr.azurecr.io/chain:dev-abc123

# Staging images
myssharedacr.azurecr.io/chain:staging
myssharedacr.azurecr.io/chain:staging-abc123

# Production images
myssharedacr.azurecr.io/chain:prod
myssharedacr.azurecr.io/chain:prod-abc123
myssharedacr.azurecr.io/chain:v1.2.3  # Semantic versioning for prod
```

**Note**: ACR name `myssharedacr` follows [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md). ACR names cannot contain hyphens, so we use the format `mys{description}` (e.g., `myssharedacr` for Mystira production ACR). See ADR-0008 for the complete naming standard.

## Solution Options

### Option 1: Shared ACR in Separate "Shared" Infrastructure (Recommended)

Create a separate Terraform workspace for shared infrastructure:

```
infra/terraform/shared/
  ├── main.tf  # ACR, DNS zones, other shared resources
```

**Pros**:

- Clear separation of concerns
- Can be deployed independently
- Easier to manage lifecycle

**Cons**:

- Requires separate deployment step
- Need to coordinate deployments

### Option 2: Keep ACR in Dev, Make It Shared

Update staging/prod to reference the dev ACR:

1. Remove environment-specific ACR references from Kubernetes overlays
2. Use `myssharedacr.azurecr.io` in all environments
3. Use tags for environment separation

**Pros**:

- Minimal changes
- Quick fix

**Cons**:

- Dev environment must be deployed first
- Coupling between environments
- Not ideal for production

### Option 3: Environment-Specific ACRs

Create ACR in each environment and update workflows:

1. Create environment-specific ACRs (not recommended - use shared ACR with tags)
2. Update CI/CD workflows to use environment-specific ACR names
3. Update workflows to pass ACR name as variable

**Pros**:

- Complete isolation between environments
- Environment-specific access controls
- No cross-environment dependencies

**Cons**:

- More complex workflows
- Higher cost (3 ACRs)
- Images can't be easily shared/promoted
- More credentials to manage

## Immediate Fix

For now, ensure the `dev` environment is deployed so `myssharedacr` exists, OR:

### Quick Fix: Use Shared ACR

1. **Update Kubernetes overlays** to use `myssharedacr.azurecr.io` instead of environment-specific names
2. **Update CI/CD workflows** to use environment-specific tags:

```yaml
# In workflow, use tags like:
tags: |
  type=ref,event=branch
  type=raw,value=dev-latest,enable=${{ github.ref == 'refs/heads/dev' }}
  type=raw,value=staging-latest,enable=${{ github.ref == 'refs/heads/main' && github.event_name == 'push' }}
```

3. **Update Kubernetes deployments** to pull from `myssharedacr.azurecr.io` with appropriate tags

## Long-Term Solution

Move ACR to a shared infrastructure workspace:

```
infra/terraform/
  ├── shared/          # Shared resources (ACR, DNS, etc.)
  ├── environments/
  │   ├── dev/         # Dev-specific resources
  │   ├── staging/     # Staging-specific resources
  │   └── prod/        # Prod-specific resources
```

Deploy shared infrastructure first, then environments can reference it.

## Related Documentation

- [Shared Resources Guide](./shared-resources.md)
- [Infrastructure Guide](./infrastructure.md)
