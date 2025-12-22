# Microsoft Entra External ID Consumer Authentication Module

This Terraform module manages Microsoft Entra External ID app registrations for consumer-facing authentication in the Mystira platform.

> **Important**: As of May 1, 2025, Azure AD B2C is no longer available for new customers. This module has been updated to support **Microsoft Entra External ID**, which is Microsoft's next-generation customer identity and access management (CIAM) solution.

## Overview

This module creates and configures:

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
   Microsoft Entra admin center → External ID → Overview → Get started guide
   - Choose authentication methods: Email + password, Email + OTP, or Social
   - Customize branding (logo, colors, background)
   - Configure user attributes to collect during sign-up
   ```

3. **Configure Identity Providers**:

   **Google**:
   - Create OAuth credentials at Google Cloud Console
   - Microsoft Entra admin center → External ID → Identity providers → Google
   - Add Client ID and Secret

   **Facebook**:
   - Create app at Facebook Developers
   - Microsoft Entra admin center → External ID → Identity providers → Facebook
   - Add App ID and App Secret

   **Custom OIDC**:
   - For Discord or other providers
   - Microsoft Entra admin center → External ID → Identity providers → Custom OIDC
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
  source = "../../modules/azure-ad-b2c"

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
  source = "../../modules/azure-ad-b2c"

  environment     = "dev"
  tenant_name     = "mystira"
  tenant_id       = "<external-tenant-id>"

  pwa_redirect_uris = [
    "http://localhost:5173/authentication/login-callback",
    "https://mystira.app/authentication/login-callback"
  ]

  mobile_redirect_uris = [
    "mystira://auth/callback",
    "exp://localhost:8081/--/auth/callback"  # For Expo development
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
| mobile_client_id | Mobile application (client) ID |
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

### Google

1. Create OAuth 2.0 credentials at [Google Cloud Console](https://console.cloud.google.com/)
2. Add authorized redirect URI: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`
3. Configure in Microsoft Entra admin center

### Facebook

1. Create app at [Facebook Developers](https://developers.facebook.com/)
2. Add OAuth redirect URI: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`
3. Configure in Microsoft Entra admin center

### Discord (Custom OIDC)

1. Create application at [Discord Developer Portal](https://discord.com/developers/applications)
2. Add redirect URI: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`
3. Configure as Custom OIDC provider with discovery URL: `https://discord.com/.well-known/openid-configuration`

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
          "data": [
            {
              "scheme": "mystira"
            }
          ],
          "category": ["BROWSABLE", "DEFAULT"]
        }
      ]
    }
  }
}
```

## Key Differences from Azure AD B2C

| Feature | Azure AD B2C | Entra External ID |
|---------|--------------|-------------------|
| **Tenant Type** | Separate B2C tenant | External configuration of Entra tenant |
| **Login Domain** | `*.b2clogin.com` | `*.ciamlogin.com` |
| **User Flows** | Custom policies & user flows | Built-in sign-up/sign-in experiences |
| **Customization** | Identity Experience Framework (XML) | Modern UI customization in portal |
| **Pricing** | Per MAU | Per MAU (different tiers) |
| **Management** | Azure Portal (B2C blade) | Microsoft Entra admin center |

## Migration from Azure AD B2C

If you're migrating from Azure AD B2C:

1. Create new External ID tenant
2. Update this Terraform module configuration
3. Migrate users using Microsoft Graph API
4. Update application configuration
5. Test authentication flows
6. Gradually migrate traffic

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
