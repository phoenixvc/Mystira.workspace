# Runbook: Terraform State Cleanup - Missing Modules

**Last Updated**: 2025-12-22
**Owner**: Platform Engineering Team
**Approval Required**: Engineering Lead
**Estimated Time**: 30-60 minutes

## Problem

Terraform is attempting to initialize modules that no longer exist in the codebase, causing errors like:

```
Error: Module not found
  module.admin_api in ../../modules/admin-api
  module.external_id in ../../modules/external-id
  module.identity in ../../modules/shared/identity
  module.entra_id in ../../modules/entra-id
```

Additional related errors:
```
Error: User Assigned Identity was not found
  mys-dev-admin-api-identity-san

Error: Invalid for_each argument
  The "for_each" map includes keys derived from resource attributes that cannot be determined until apply
```

## Root Cause

The Terraform state contains references to modules and resources that:
1. Were removed from the codebase
2. Were never created in Azure but are in the state
3. Have dependencies on resources that don't exist

## Prerequisites

- [ ] Azure CLI authenticated with appropriate permissions
- [ ] Terraform CLI installed (version >= 1.5.0)
- [ ] Access to the Terraform state storage account
- [ ] Engineering Lead approval
- [ ] Backup of current state

## Procedure

### Step 1: Backup Current State

**ALWAYS backup state before making changes!**

```bash
# Navigate to environment
cd infra/terraform/environments/dev  # or staging/prod

# Download current state
terraform state pull > terraform.tfstate.backup.$(date +%Y%m%d_%H%M%S)

# Verify backup
ls -la terraform.tfstate.backup.*
```

### Step 2: List All Resources in State

```bash
# See all resources currently tracked
terraform state list

# Filter for modules we need to remove
terraform state list | grep -E "(admin_api|external_id|identity|entra_id)"
```

### Step 3: Remove Orphaned Module References

For each orphaned module, remove from state:

```bash
# Remove admin_api module (if exists in state)
terraform state rm 'module.admin_api'

# Remove external_id module (if exists in state)
terraform state rm 'module.external_id'

# Remove identity module (if exists in state)
terraform state rm 'module.identity'

# Remove entra_id module (if exists in state)
terraform state rm 'module.entra_id'
```

### Step 4: Remove Orphaned Resources

If specific resources are orphaned:

```bash
# Remove specific user assigned identity
terraform state rm 'module.shared_postgresql.data.azurerm_user_assigned_identity.aad_admins["admin-api"]'

# Remove role assignments with unknown keys
terraform state rm 'module.identity.azurerm_role_assignment.service_key_vault_reader'
```

### Step 5: Reinitialize Terraform

```bash
# Clear module cache
rm -rf .terraform/modules

# Reinitialize
terraform init -reconfigure

# Verify no errors
terraform validate
```

### Step 6: Plan and Review

```bash
# Create a plan
terraform plan -out=tfplan

# Review the plan carefully
# Should show no changes if cleanup was successful
# Or show only expected changes
```

### Step 7: Apply if Needed

```bash
# Only if plan shows expected changes
terraform apply tfplan
```

## Handling Specific Errors

### Error: for_each Unknown Keys

This error occurs when `for_each` depends on values not known until apply:

```
for_each = { for k, v in var.service_identities : k => v if v.key_vault_id != "" }
```

**Solution Options:**

1. **Use static keys:**
   ```hcl
   for_each = { for k, v in var.service_identities : k => v }
   # Move the condition to the resource attributes
   ```

2. **Use targeted apply:**
   ```bash
   # First apply the dependencies
   terraform apply -target=module.identity.azurerm_user_assigned_identity.main

   # Then apply everything
   terraform apply
   ```

3. **Define keys statically:**
   ```hcl
   variable "service_identities" {
     type = map(object({
       name         = string
       key_vault_id = optional(string, "")
     }))
   }
   ```

### Error: User Assigned Identity Not Found

This occurs when data sources reference identities that don't exist.

**Solution:**

1. Create the identity first (if needed)
2. Or remove the data source reference from state:
   ```bash
   terraform state rm 'module.shared_postgresql.data.azurerm_user_assigned_identity.aad_admins["admin-api"]'
   ```

3. Or add `count` or `for_each` with existence check:
   ```hcl
   data "azurerm_user_assigned_identity" "aad_admins" {
     for_each = toset([for id in var.aad_admin_identities : id if can(data.azurerm_resources.check[id].resources)])
     # ...
   }
   ```

## Verification

After cleanup, verify:

- [ ] `terraform init` completes without errors
- [ ] `terraform validate` passes
- [ ] `terraform plan` shows expected state (ideally no changes)
- [ ] No references to removed modules in state (`terraform state list`)

## Rollback

If cleanup causes issues:

```bash
# Restore from backup
terraform state push terraform.tfstate.backup.<timestamp>

# Verify restoration
terraform state list
```

## Prevention

To prevent future orphaned state:

1. **Always remove from code first, then state:**
   ```bash
   # 1. Remove module from .tf file
   # 2. Run terraform plan (should show destroy)
   # 3. Run terraform apply (destroys resources)
   ```

2. **Use `terraform destroy` for full cleanup:**
   ```bash
   terraform destroy -target=module.module_name
   ```

3. **Document module dependencies** before removal

4. **Review terraform plan** before any changes

## Related Issues

- Missing modules: `admin-api`, `external-id`, `identity`, `entra-id`
- These modules may need to be:
  - Created if they're actually needed
  - Or fully removed from state if deprecated

## References

- [Terraform State Management](https://developer.hashicorp.com/terraform/language/state)
- [Terraform State Commands](https://developer.hashicorp.com/terraform/cli/commands/state)
- [ADR-0008: Azure Resource Naming](../../architecture/adr/0008-azure-resource-naming-conventions.md)
