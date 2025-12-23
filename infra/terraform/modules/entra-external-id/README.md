# Microsoft Entra External ID Consumer Authentication Module

This Terraform module manages Microsoft Entra External ID app registrations for consumer-facing authentication in the Mystira platform.

## Overview

Microsoft Entra External ID is Microsoft's customer identity and access management (CIAM) solution for consumer-facing applications. This module creates and configures:

- **Public API** app registration with exposed scopes
- **PWA/SPA** app registration with API permissions
- **Mobile App** app registration with native client configuration (optional)
- **API Scopes** for consumer operations

## Prerequisites

**IMPORTANT**: Microsoft Entra External ID tenant creation is not fully supported via Terraform. You must:

1. Create the External ID tenant manually in Azure Portal or using Azure CLI
2. Configure sign-up and sign-in experiences
3. Set up identity providers (Google, Facebook, etc.) in the external tenant
4. Then run this Terraform module to create app registrations

### Manual External ID Tenant Setup

#### Option 1: Azure Portal (Recommended)

1. **Create External Tenant**:
   ```
   Azure Portal → Microsoft Entra ID → Manage tenants → Create
   - Select "External" configuration
   - Organization name: Mystira External ID
   - Domain name: mystira (will become mystira.ciamlogin.com)
   - Country/Region: Select your region
   ```

2. **Configure Sign-in Experience**:
   ```
   Microsoft Entra admin center → External Identities → Overview → Get started guide
   - Choose authentication methods: Email + password, Email + OTP, or Social
   - Customize branding (logo, colors, background)
   - Configure user attributes to collect during sign-up
   ```

3. **Configure Identity Providers**:

   **Google**:
   - Create OAuth credentials at Google Cloud Console
   - Microsoft Entra admin center → External Identities → All identity providers → Google
   - Add Client ID and Secret

   **Facebook**:
   - Create app at Facebook Developers
   - Microsoft Entra admin center → External Identities → All identity providers → Facebook
   - Add App ID and App Secret

   **Custom OIDC**:
   - For Discord or other providers
   - Microsoft Entra admin center → External Identities → All identity providers → Custom OIDC
   - Configure discovery endpoint and credentials

#### Option 2: Azure CLI with Device Code Flow

For automated deployments, you can use the Azure CLI with device code authentication:

```bash
# Login with device code (requires manual intervention)
az login --use-device-code

# Create external tenant (requires user_impersonation scope)
az rest --method put \
  --url "https://management.azure.com/subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.AzureActiveDirectory/ciamDirectories/{tenant-name}?api-version=2023-05-17-preview" \
  --body '{
    "location": "westus",
    "sku": {
      "name": "Base",
      "tier": "A0"
    },
    "properties": {
      "createTenantProperties": {
        "displayName": "Mystira External ID",
        "countryCode": "US"
      }
    }
  }'
```

## Usage

### Basic Configuration (PWA Only)

```hcl
module "entra_external_id" {
  source = "../../modules/entra-external-id"

  environment     = "dev"
  tenant_name     = "mystira"  # Your external tenant subdomain
  tenant_id       = "<external-tenant-id>"  # From Azure Portal

  pwa_redirect_uris = [
    "http://localhost:5173/authentication/login-callback",
    "https://mystira.app/authentication/login-callback"
  ]
}
```

### With Mobile App Support

```hcl
module "entra_external_id" {
  source = "../../modules/entra-external-id"

  environment     = "dev"
  tenant_name     = "mystira"
  tenant_id       = "<external-tenant-id>"

  pwa_redirect_uris = [
    "http://localhost:5173/authentication/login-callback",
    "https://mystira.app/authentication/login-callback"
  ]

  mobile_redirect_uris = [
    "mystira://auth/callback"
  ]
}
```

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| environment | Deployment environment | `string` | n/a | yes |
| tenant_name | External tenant subdomain | `string` | `"mystira"` | no |
| tenant_id | External tenant ID (GUID) | `string` | n/a | yes |
| pwa_redirect_uris | PWA redirect URIs | `list(string)` | `[]` | no |
| mobile_redirect_uris | Mobile app redirect URIs | `list(string)` | `[]` | no |

## Outputs

| Name | Description |
|------|-------------|
| public_api_client_id | Public API application (client) ID |
| pwa_client_id | PWA application (client) ID |
| api_scopes | Map of API scopes |
| public_api_config | Configuration for Public API |
| pwa_config | Configuration for PWA |
| mobile_config | Configuration for Mobile App |
| auth_endpoints | Authentication endpoints |

## API Scopes

| Scope | Description |
|-------|-------------|
| `API.Access` | General API access |
| `Stories.Read` | Read user stories |
| `Stories.Write` | Create and edit stories |
| `Profile.Read` | Read user profile |

## Configuration Examples

### ASP.NET Core Public API (appsettings.json)

```json
{
  "MicrosoftEntraExternalId": {
    "Instance": "https://mystira.ciamlogin.com",
    "Domain": "mystira.onmicrosoft.com",
    "TenantId": "<external-tenant-id>",
    "ClientId": "<public_api_client_id>"
  }
}
```

### Blazor WASM PWA (appsettings.json)

```json
{
  "MicrosoftEntraExternalId": {
    "Authority": "https://mystira.ciamlogin.com/<external-tenant-id>/v2.0",
    "ClientId": "<pwa_client_id>",
    "ValidateAuthority": true
  },
  "MystiraApi": {
    "Scopes": ["api://<public_api_client_id>/API.Access"],
    "BaseUrl": "https://api.mystira.app/"
  }
}
```

### React/Vite PWA (.env)

```bash
VITE_ENTRA_CLIENT_ID=<pwa_client_id>
VITE_ENTRA_TENANT_ID=<external_tenant_id>
VITE_ENTRA_AUTHORITY=https://mystira.ciamlogin.com/<external_tenant_id>/v2.0
VITE_ENTRA_API_SCOPE=api://<public_api_client_id>/API.Access
```

### React Native / Expo Mobile App (.env)

```bash
EXPO_PUBLIC_ENTRA_CLIENT_ID=<mobile_client_id>
EXPO_PUBLIC_ENTRA_TENANT_ID=<external_tenant_id>
EXPO_PUBLIC_ENTRA_AUTHORITY=https://mystira.ciamlogin.com/<external_tenant_id>/v2.0
EXPO_PUBLIC_ENTRA_API_SCOPE=api://<public_api_client_id>/API.Access
EXPO_PUBLIC_ENTRA_REDIRECT_URI=mystira://auth/callback
```

### Mobile App Integration (React Native)

Install required dependencies:

```bash
npm install @azure/msal-react-native
# or
yarn add @azure/msal-react-native
```

Configure authentication:

```typescript
import { PublicClientApplication } from '@azure/msal-react-native';

const config = {
  auth: {
    clientId: process.env.EXPO_PUBLIC_ENTRA_CLIENT_ID,
    authority: process.env.EXPO_PUBLIC_ENTRA_AUTHORITY,
  },
};

const pca = new PublicClientApplication(config);

// Sign in
const result = await pca.acquireTokenInteractive({
  scopes: [process.env.EXPO_PUBLIC_ENTRA_API_SCOPE],
});
```

## Social Login Configuration

> **CAUTION: Custom Domain Limitations**
>
> Social federation with custom domains (e.g., `mystira.ciamlogin.com`) has provider-specific limitations:
>
> - **Google & Facebook**: Require manual registration of custom redirect URIs with their internal federation services. This **cannot be completed from the Azure portal alone** - you must contact Google/Facebook support to allowlist your custom domain.
> - **Custom OIDC Providers (Discord)**: May require explicit OpenID Connect discovery endpoint configuration and manual validation of the discovery document.
> - **Default Domain Alternative**: If custom domain registration is blocked or delayed, you can temporarily use the default Microsoft domain: `mystira.onmicrosoft.com`

### Google

1. **Create OAuth 2.0 credentials** at [Google Cloud Console](https://console.cloud.google.com/)
   - Navigate to APIs & Services > Credentials > Create Credentials > OAuth client ID
   - Application type: Web application
   - Authorized redirect URIs: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`

2. **Manual Custom Domain Registration** (Required for production custom domains)
   - Custom domain redirect URIs require approval by Google's internal federation team
   - Submit a request through Google Cloud Support to allowlist your custom domain
   - Processing time: 5-10 business days
   - Alternative: Use default Microsoft domain until approved

3. **Required OAuth Scopes**:
   - `openid` (required)
   - `profile` (recommended - provides name, picture)
   - `email` (recommended - provides email address)

4. **Configure in Microsoft Entra admin center**:
   - Navigate to External Identities > All identity providers > Google
   - Enter Client ID and Client Secret from Google Console
   - Map attributes: email → email, given_name → firstName, family_name → lastName

### Facebook

1. **Create app** at [Facebook Developers](https://developers.facebook.com/)
   - Create New App > Consumer > Add Facebook Login product
   - Valid OAuth Redirect URIs: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`

2. **Manual Custom Domain Registration** (Required for production custom domains)
   - Custom domain redirect URIs require approval by Facebook's Platform Operations team
   - Submit via Facebook Developer Support > Platform Policy & Support
   - Processing time: 7-14 business days
   - Alternative: Use default Microsoft domain until approved

3. **Required Permissions**:
   - `email` (default - user's email address)
   - `public_profile` (default - name, profile picture, gender, locale)

4. **Configure in Microsoft Entra admin center**:
   - Navigate to External Identities > All identity providers > Facebook
   - Enter App ID and App Secret from Facebook Developers
   - Map attributes: email → email, first_name → firstName, last_name → lastName

### Discord (Custom OIDC)

1. **Create application** at [Discord Developer Portal](https://discord.com/developers/applications)
   - Create New Application
   - Navigate to OAuth2 settings
   - Add redirect URI: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`

2. **Required OAuth2 Scopes**:
   - `identify` (required - basic user info)
   - `email` (required - user email address)

3. **Configure as Custom OIDC provider in Microsoft Entra admin center**:
   - Navigate to External Identities > All identity providers > Custom OIDC
   - Provider name: Discord
   - Metadata URL: `https://discord.com/.well-known/openid-configuration`
   - Enter Client ID and Client Secret from Discord Developer Portal
   - Claims mapping: `sub` → User ID, `email` → Email, `username` → Display Name

## Multi-Environment Strategy

### Separate External Tenants per Environment

For complete isolation, create separate External ID tenants:

| Environment | Tenant Domain | Purpose |
|-------------|--------------|---------|
| Dev | `mystiradev.ciamlogin.com` | Development testing |
| Staging | `mystirastaging.ciamlogin.com` | Pre-production validation |
| Prod | `mystira.ciamlogin.com` | Production |

### Single Tenant with Separate App Registrations

For cost efficiency, use a single tenant with environment-specific apps:

```hcl
# Each environment gets its own app registrations within the same tenant
module "entra_external_id" {
  source = "../../modules/entra-external-id"

  environment = "dev"  # "staging" or "prod"
  tenant_name = "mystira"
  tenant_id   = var.external_id_tenant_id

  pwa_redirect_uris = [
    "http://localhost:5173/auth/callback",
    "https://app.dev.mystira.app/auth/callback"  # Environment-specific URLs
  ]
}
```

## Rollback Procedure

If you need to revert changes:

### Terraform State Rollback

```bash
# List available state versions (Azure Storage backend)
az storage blob list \
  --account-name <storage_account> \
  --container-name <container_name> \
  --prefix "terraform.tfstate" \
  --output table

# Download previous state
az storage blob download \
  --account-name <storage_account> \
  --container-name <container_name> \
  --name "terraform.tfstate.<version>" \
  --file "terraform.tfstate.backup"

# Restore and apply
cp terraform.tfstate.backup terraform.tfstate
terraform plan
terraform apply
```

### Targeted Resource Destruction

```bash
# Remove specific app registrations
terraform destroy \
  -target=module.entra_external_id.azuread_application.pwa \
  -target=module.entra_external_id.azuread_application.public_api

# Verify
az ad app list --display-name "Mystira" --output table
```

## Mobile App URL Scheme Configuration

### iOS (app.json / app.config.js)

```json
{
  "expo": {
    "scheme": "mystira",
    "ios": {
      "bundleIdentifier": "com.mystira.app"
    }
  }
}
```

### Android (app.json / app.config.js)

```json
{
  "expo": {
    "scheme": "mystira",
    "android": {
      "package": "com.mystira.app",
      "intentFilters": [
        {
          "action": "VIEW",
          "data": [{ "scheme": "mystira" }],
          "category": ["BROWSABLE", "DEFAULT"]
        }
      ]
    }
  }
}
```

## Terraform Provider Limitations

**Note**: The `azuread` Terraform provider works well for managing app registrations in External ID tenants, but tenant creation requires either:

- Manual creation via Azure Portal (recommended)
- Azure CLI with device code flow
- `azapi` provider with user impersonation scope

This module focuses on app registration management and assumes the external tenant already exists.

## Related Documentation

- [Microsoft Entra External ID Overview](https://learn.microsoft.com/en-us/entra/external-id/external-identities-overview)
- [External ID Quickstart](https://learn.microsoft.com/en-us/entra/external-id/customers/quickstart-get-started-guide)
- [Terraform AzureAD Provider](https://registry.terraform.io/providers/hashicorp/azuread/latest/docs)
- [ADR-0011: Entra ID Authentication Integration](../../../docs/architecture/adr/0011-entra-id-authentication-integration.md)
- [External ID User Flows](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-user-flow-sign-up-sign-in-customers)
