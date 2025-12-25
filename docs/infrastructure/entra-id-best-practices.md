# Entra ID Best Practices Guide

This guide provides best practices for configuring Microsoft Entra ID (Azure AD) across multiple environments, including external entity access, app registrations, and security considerations.

## Overview

Mystira uses Microsoft Entra ID for:
- **Admin authentication**: Internal staff accessing the Admin UI and API
- **Consumer authentication**: End users via Entra External ID (separate tenant)
- **Service-to-service**: Workload identity for AKS pods

## Multi-Environment Strategy

### Separate App Registrations per Environment

Each environment (dev, staging, prod) has its own app registrations:

| Environment | Admin API | Admin UI | Purpose |
|-------------|-----------|----------|---------|
| Dev | `Mystira Admin API (dev)` | `Mystira Admin UI (dev)` | Development testing |
| Staging | `Mystira Admin API (staging)` | `Mystira Admin UI (staging)` | Pre-production validation |
| Prod | `Mystira Admin API (prod)` | `Mystira Admin UI (prod)` | Production |

**Why separate registrations?**
- Isolated redirect URIs prevent cross-environment token leakage
- Environment-specific scopes and roles
- Easier to audit and troubleshoot
- Independent lifecycle management

### Terraform Configuration

```hcl
module "entra_id" {
  source = "../../modules/entra-id"

  environment = "dev"  # "staging" or "prod"

  admin_ui_redirect_uris = [
    "http://localhost:7001/auth/callback",    # Local development
    "https://admin.dev.mystira.app/auth/callback"
  ]

  tags = {
    Environment = "dev"
    CostCenter  = "development"
  }
}
```

### Environment-Specific Redirect URIs

| Environment | Redirect URIs |
|-------------|---------------|
| Dev | `http://localhost:*`, `https://admin.dev.mystira.app/*` |
| Staging | `https://admin.staging.mystira.app/*` |
| Prod | `https://admin.mystira.app/*` |

## External Entities

### Partner Applications

For external partners needing API access:

1. **Create a dedicated app registration** for the partner
2. **Use application permissions** (client credentials flow)
3. **Scope access** to specific API endpoints
4. **Set expiration** on client secrets (recommended: 6 months)

```hcl
# Partner app registration (add to entra-id module)
resource "azuread_application" "partner_api" {
  display_name = "Mystira Partner - ${var.partner_name} (${var.environment})"

  required_resource_access {
    resource_app_id = azuread_application.admin_api.client_id

    resource_access {
      id   = random_uuid.scope_ids["Partner.Read"].result
      type = "Role"  # Application permission, not delegated
    }
  }
}
```

### Third-Party Service Integration

For SaaS integrations (analytics, monitoring, etc.):

| Integration Type | Auth Method | Example |
|-----------------|-------------|---------|
| Incoming webhooks | API key or token | Stripe webhooks |
| Outgoing API calls | Managed identity or client credentials | Calling external APIs |
| Data export | Service principal with limited scope | Export to data warehouse |

**Best Practice**: Use workload identity where possible, avoiding stored secrets.

### Guest Users (B2B)

For external collaborators needing Admin UI access:

1. **Invite as guest** in Azure portal or via Graph API
2. **Assign app roles** after invitation is accepted
3. **Use Conditional Access** to require MFA for guests

```bash
# Invite guest user via Azure CLI
az ad user invite \
  --invite-redirect-url "https://admin.mystira.app" \
  --invited-user-email "partner@external.com" \
  --invited-user-display-name "Partner User"
```

## App Registration Patterns

### Admin API (Backend)

```hcl
resource "azuread_application" "admin_api" {
  display_name     = "Mystira Admin API (${var.environment})"
  identifier_uris  = ["api://mystira-admin-api-${var.environment}"]
  sign_in_audience = "AzureADMyOrg"  # Single tenant

  api {
    mapped_claims_enabled          = true
    requested_access_token_version = 2

    oauth2_permission_scope {
      id                         = random_uuid.scope_admin_read.result
      value                      = "Admin.Read"
      type                       = "Admin"
      admin_consent_description  = "Allows the app to read admin data"
      admin_consent_display_name = "Read admin data"
    }
  }

  app_role {
    id                   = random_uuid.role_admin.result
    value                = "Admin"
    display_name         = "Admin"
    description          = "Full admin access"
    allowed_member_types = ["User", "Application"]
  }
}
```

### Admin UI (SPA Frontend)

```hcl
resource "azuread_application" "admin_ui" {
  display_name     = "Mystira Admin UI (${var.environment})"
  sign_in_audience = "AzureADMyOrg"

  single_page_application {
    redirect_uris = var.admin_ui_redirect_uris
  }

  required_resource_access {
    resource_app_id = azuread_application.admin_api.client_id

    resource_access {
      id   = random_uuid.scope_admin_read.result
      type = "Scope"  # Delegated permission
    }
  }
}
```

## API Scopes and Roles

### Defined Scopes

| Scope | Type | Description | Assigned To |
|-------|------|-------------|-------------|
| `Admin.Read` | Admin consent | Read admin data | Admin UI |
| `Admin.Write` | Admin consent | Modify admin data | Admin UI |
| `Users.Manage` | Admin consent | Manage users | Admin UI |
| `Content.Moderate` | Admin consent | Moderate content | Admin UI |

### App Roles

| Role | Description | Typical Users |
|------|-------------|---------------|
| `SuperAdmin` | Full system access | Platform administrators |
| `Admin` | Standard admin access | Team leads, managers |
| `Moderator` | Content moderation only | Content moderators |
| `Viewer` | Read-only access | Auditors, support staff |

### Assigning Roles

```bash
# Assign role to user via Azure CLI
az ad app permission grant \
  --id <app-client-id> \
  --api <api-client-id> \
  --scope "Admin.Read Admin.Write"

# Assign app role to user
az rest --method POST \
  --uri "https://graph.microsoft.com/v1.0/servicePrincipals/{sp-id}/appRoleAssignedTo" \
  --body '{"principalId":"<user-object-id>","resourceId":"<sp-id>","appRoleId":"<role-id>"}'
```

## Security Best Practices

### Token Configuration

1. **Use access token version 2** for modern OAuth 2.0 compliance
2. **Keep token lifetimes short** (default: 1 hour access, 24 hour refresh)
3. **Enable token encryption** for sensitive claims

### Conditional Access (Enterprise)

Recommended policies for production:

| Policy | Target | Condition | Control |
|--------|--------|-----------|---------|
| Require MFA | Admin apps | All users | MFA required |
| Block legacy auth | All apps | Legacy protocols | Block |
| Sign-in risk | Admin apps | Medium/high risk | Block or MFA |
| Device compliance | Admin apps | Unmanaged devices | Block or limited access |

### Secret Management

1. **Never store client secrets in code**
2. **Use managed identities** for Azure-to-Azure communication
3. **Rotate secrets automatically** via Terraform
4. **Set expiration** on all secrets (max 2 years)

```hcl
# Auto-populated secrets in Key Vault (from entra_id module)
resource "azurerm_key_vault_secret" "admin_entra_tenant_id" {
  name         = "azure-ad-tenant-id"
  value        = module.entra_id.tenant_id
  key_vault_id = module.admin_api.key_vault_id
  content_type = "azure-ad"
  tags         = { AutoPopulated = "true", Source = "entra-id-module" }
}
```

### Audit and Monitoring

1. **Enable sign-in logs** in Azure AD
2. **Stream to Log Analytics** for long-term retention
3. **Alert on suspicious activity**:
   - Multiple failed sign-ins
   - Sign-ins from unusual locations
   - App consent from non-admins

## Microsoft Entra External ID (Consumer Authentication)

For end-user (consumer) authentication, Mystira uses Microsoft Entra External ID - Microsoft's modern customer identity and access management (CIAM) solution.

### Entra ID vs External ID

| Feature | Entra ID (Admin) | Entra External ID (Consumers) |
|---------|------------------|-------------------------------|
| Users | Internal staff | External customers |
| Identity providers | Microsoft account | Social (Google, Facebook), local accounts |
| Branding | Corporate | Fully customizable |
| Authentication | Standard sign-in | Self-service sign-up/sign-in |
| Scale | Thousands | Millions |
| Login domain | `login.microsoftonline.com` | `*.ciamlogin.com` |

### External ID Configuration

```hcl
# External ID tenant must be created manually first
module "entra_external_id" {
  source = "../../modules/entra-external-id"
  count  = var.external_id_tenant_id != "" ? 1 : 0

  environment   = "dev"
  tenant_id     = var.external_id_tenant_id
  tenant_name   = "mystiradev"

  pwa_redirect_uris = [
    "http://localhost:5173/auth/callback",
    "https://app.dev.mystira.app/auth/callback"
  ]
}
```

### Multi-Environment External ID Strategy

For complete isolation, create separate External ID tenants per environment:

| Environment | Tenant Domain | Purpose |
|-------------|---------------|---------|
| Dev | `mystiradev.ciamlogin.com` | Development testing |
| Staging | `mystirastaging.ciamlogin.com` | Pre-production validation |
| Prod | `mystira.ciamlogin.com` | Production |

### Setting Up External ID Tenant

1. **Create tenant** in Azure Portal:
   - Microsoft Entra ID → Manage tenants → Create
   - Select "External" configuration
   - Set organization name and domain

2. **Configure sign-in experience**:
   - Choose authentication methods (Email + password, Email + OTP, Social)
   - Customize branding (logo, colors, background)
   - Configure user attributes to collect

3. **Add identity providers**:
   - Google: Create OAuth credentials, add to External Identities
   - Facebook: Create app, configure in External Identities
   - Custom OIDC: Configure discovery endpoint

See [Entra External ID Module Documentation](../../infra/terraform/modules/entra-external-id/README.md) for detailed setup instructions.

## Troubleshooting

### Common Issues

**"AADSTS50011: Reply URL mismatch"**
- Verify redirect URI matches exactly (including trailing slashes)
- Check environment-specific configuration

**"AADSTS65001: User needs to consent"**
- Admin consent may be required for admin-scoped permissions
- Grant consent via Azure portal or Graph API

**"AADSTS700016: Application not found"**
- Verify correct tenant ID
- Check app registration exists in target tenant

### Diagnostic Commands

```bash
# Check app registration
az ad app show --id <client-id>

# List API permissions
az ad app permission list --id <client-id>

# Check service principal
az ad sp show --id <client-id>

# List role assignments
az ad app show --id <client-id> --query appRoles
```

## Related Documentation

- [Terraform Entra ID Module](../../infra/terraform/modules/entra-id/)
- [Terraform Entra External ID Module](../../infra/terraform/modules/entra-external-id/)
- [Secrets Management](./secrets-management.md)
- [Infrastructure Guide](./infrastructure.md)
- [Microsoft Entra ID Documentation](https://learn.microsoft.com/en-us/entra/identity/)
- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/en-us/entra/external-id/)
