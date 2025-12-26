# Entra ID (Azure AD) Authentication Module

This Terraform module manages Microsoft Entra ID (Azure AD) app registrations for the Mystira platform.

## Overview

This module creates and configures:

- **Admin API** app registration with exposed API scopes
- **Admin UI** app registration (SPA) with API permissions
- **App Roles** for role-based authorization
- **Service Principals** for the applications

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Microsoft Entra ID                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────┐     ┌─────────────────────┐           │
│  │   Admin API App     │     │    Admin UI App     │           │
│  │   Registration      │◀────│    Registration     │           │
│  ├─────────────────────┤     ├─────────────────────┤           │
│  │ Exposed Scopes:     │     │ API Permissions:    │           │
│  │ - Admin.Read        │     │ - Admin.Read        │           │
│  │ - Admin.Write       │     │ - Admin.Write       │           │
│  │ - Users.Manage      │     │ - Users.Manage      │           │
│  │ - Content.Moderate  │     │ - Content.Moderate  │           │
│  ├─────────────────────┤     ├─────────────────────┤           │
│  │ App Roles:          │     │ Redirect URIs:      │           │
│  │ - Admin             │     │ - localhost:7001    │           │
│  │ - SuperAdmin        │     │ - admin.mystira.app │           │
│  │ - Moderator         │     │                     │           │
│  │ - Viewer            │     │                     │           │
│  └─────────────────────┘     └─────────────────────┘           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Usage

```hcl
module "entra_id" {
  source = "../../modules/entra-id"

  environment = "dev"

  admin_ui_redirect_uris = [
    "http://localhost:7001/auth/callback",
    "https://dev.admin.mystira.app/auth/callback"
  ]

  tags = {
    CostCenter = "development"
  }
}
```

## Requirements

### Providers

| Provider | Version |
|----------|---------|
| azuread  | ~> 2.47 |
| azurerm  | ~> 3.80 |
| random   | ~> 3.5  |

### Permissions

The service principal running Terraform needs the following Microsoft Graph API permissions:

- `Application.ReadWrite.All` - To create and manage app registrations
- `AppRoleAssignment.ReadWrite.All` - To assign app roles
- `DelegatedPermissionGrant.ReadWrite.All` - To grant admin consent

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| environment | Deployment environment (dev, staging, prod) | `string` | n/a | yes |
| admin_ui_redirect_uris | Redirect URIs for Admin UI application | `list(string)` | `[]` | no |
| enable_external_id | Enable Entra External ID configuration | `bool` | `false` | no |
| external_id_tenant_domain | External ID tenant domain | `string` | `""` | no |
| tags | Tags to apply to resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| admin_api_client_id | Admin API application (client) ID |
| admin_api_identifier_uri | Admin API identifier URI (audience) |
| admin_ui_client_id | Admin UI application (client) ID |
| tenant_id | Azure AD tenant ID |
| api_scopes | Map of API scope names to their IDs |
| app_roles | Map of app role names to their IDs |
| admin_api_config | Configuration values for Admin API |
| admin_ui_config | Configuration values for Admin UI |

## Post-Deployment Steps

After running Terraform, complete these manual steps:

### 1. Create Conditional Access Policies

In Azure Portal → Entra ID → Security → Conditional Access:

```yaml
Policy: Require MFA for Mystira Admin
Assignments:
  Users: mystira-admins, mystira-superadmins
  Cloud apps: mystira-admin-api, mystira-admin-ui
Access controls:
  Grant: Require MFA
```

### 2. Create Security Groups

```bash
# Create admin groups
az ad group create --display-name "mystira-admins" --mail-nickname "mystira-admins"
az ad group create --display-name "mystira-superadmins" --mail-nickname "mystira-superadmins"
az ad group create --display-name "mystira-moderators" --mail-nickname "mystira-moderators"
```

### 3. Assign Users to Roles

```bash
# Assign admin role to a user
az ad app role assignment create \
  --app-id <admin-api-client-id> \
  --assignee-principal-type User \
  --assignee-object-id <user-object-id> \
  --id <admin-role-id>
```

### 4. Configure Applications

Use the output values to configure your applications:

**Admin API (appsettings.json)**:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant_id>",
    "ClientId": "<admin_api_client_id>",
    "Audience": "<admin_api_identifier_uri>"
  }
}
```

**Admin UI (.env)**:
```bash
VITE_AZURE_CLIENT_ID=<admin_ui_client_id>
VITE_AZURE_TENANT_ID=<tenant_id>
VITE_AZURE_AUTHORITY=https://login.microsoftonline.com/<tenant_id>
VITE_API_SCOPE=<admin_api_identifier_uri>/Admin.Read
```

## Related Documentation

- [ADR-0011: Entra ID Authentication Integration](../../../docs/architecture/adr/0011-entra-id-authentication-integration.md)
- [ADR-0010: Authentication and Authorization Strategy](../../../docs/architecture/adr/0010-authentication-and-authorization-strategy.md)
- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
