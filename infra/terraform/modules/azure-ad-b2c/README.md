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

> **⚠️ CAUTION: Custom Domain Limitations**
>
> Social federation with custom domains (e.g., `mystira.ciamlogin.com`) has provider-specific limitations:
>
> - **Google & Facebook**: Require manual registration of custom redirect URIs with their internal federation services. This **cannot be completed from the Azure portal alone** - you must contact Google/Facebook support to allowlist your custom domain.
> - **Custom OIDC Providers (Discord)**: May require explicit OpenID Connect discovery endpoint configuration and manual validation of the discovery document.
> - **Default Domain Alternative**: If custom domain registration is blocked or delayed, you can temporarily use the default Microsoft domain: `mystira.onmicrosoft.com` (redirect URI: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`)

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
   - Navigate to External ID > Identity providers > Google
   - Enter Client ID and Client Secret from Google Console
   - Map attributes: email → email, given_name → firstName, family_name → lastName

5. **User Consent Settings**:
   - Set OAuth consent screen to "External" for public access
   - Add all required scopes to the consent screen
   - Verify domain ownership in Google Console (required for production)

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
   - Navigate to External ID > Identity providers > Facebook
   - Enter App ID and App Secret from Facebook Developers
   - Map attributes: email → email, first_name → firstName, last_name → lastName

5. **User Consent & Privacy Settings**:
   - Complete App Review for `email` permission (required for production)
   - Add Privacy Policy URL (required)
   - Add Terms of Service URL (required)
   - Set app to "Live" mode after review completion

### Discord (Custom OIDC)

1. **Create application** at [Discord Developer Portal](https://discord.com/developers/applications)
   - Create New Application
   - Navigate to OAuth2 settings
   - Add redirect URI: `https://mystira.ciamlogin.com/mystira.onmicrosoft.com/oauth2/authresp`

2. **Required OAuth2 Scopes**:
   - `identify` (required - basic user info)
   - `email` (required - user email address)

3. **Explicit Discovery Configuration**:
   - Discord uses standard OpenID Connect discovery
   - Discovery URL: `https://discord.com/.well-known/openid-configuration`
   - Verify the discovery document is accessible and valid before configuration
   - Key endpoints to validate:
     - `authorization_endpoint`: `https://discord.com/oauth2/authorize`
     - `token_endpoint`: `https://discord.com/api/oauth2/token`
     - `userinfo_endpoint`: `https://discord.com/api/users/@me`

4. **Configure as Custom OIDC provider in Microsoft Entra admin center**:
   - Navigate to External ID > Identity providers > Custom OIDC
   - Provider name: Discord
   - Metadata URL: `https://discord.com/.well-known/openid-configuration`
   - Enter Client ID and Client Secret from Discord Developer Portal
   - Claims mapping:
     - `sub` → User ID (unique identifier)
     - `email` → Email
     - `username` → Display Name

5. **Additional Configuration**:
   - Enable "Verify email" claim if email verification is required
   - Test the configuration in a non-production tenant first
   - Monitor for any discovery endpoint changes (Discord may update endpoints)

## Rollback Procedure

If you need to revert changes made by this Terraform module or restore previous identity provider configurations, follow this rollback procedure in accordance with the repository's infrastructure change guidelines.

### Prerequisites

- [ ] Verify you have access to Terraform state (Azure Storage or local state file)
- [ ] Document the reason for rollback
- [ ] Notify stakeholders of the rollback
- [ ] Have backup of current configuration (if available)

### Rollback Steps

#### Option 1: Terraform State Rollback (Preferred)

If you need to revert to a previous Terraform state:

```bash
# 1. List available Terraform state versions (if using Azure Storage backend)
az storage blob list \
  --account-name <storage_account> \
  --container-name <container_name> \
  --prefix "terraform.tfstate" \
  --output table

# 2. Download the previous state version
az storage blob download \
  --account-name <storage_account> \
  --container-name <container_name> \
  --name "terraform.tfstate.<version>" \
  --file "terraform.tfstate.backup"

# 3. Replace current state with backup
cp terraform.tfstate.backup terraform.tfstate

# 4. Verify state integrity
terraform state list

# 5. Apply the previous configuration
terraform plan  # Review changes
terraform apply  # Restore previous configuration
```

#### Option 2: Targeted Resource Destruction

If you need to remove specific resources created by this module:

```bash
# 1. List all resources managed by this module
terraform state list

# 2. Remove specific app registrations (example)
terraform destroy \
  -target=azuread_application.pwa \
  -target=azuread_application.public_api \
  -target=azuread_application.mobile_app

# 3. Verify resources are removed
az ad app list --display-name "Mystira" --output table
```

#### Option 3: Complete Module Rollback

If you need to remove all resources created by this module:

```bash
# 1. Navigate to the module directory
cd infra/terraform/environments/<environment>

# 2. Run targeted destroy for the auth module
terraform destroy -target=module.azure_ad_external_id

# 3. Verify all resources are removed
az ad app list --display-name "Mystira" --output table
```

### Restore Identity Provider Configurations

If you need to restore previous social identity provider configurations:

#### Google Provider Rollback

1. **Revert to previous OAuth credentials**:
   - Access [Google Cloud Console](https://console.cloud.google.com/)
   - Navigate to APIs & Services > Credentials
   - Restore previous Client ID and Secret
   - Update redirect URIs to previous values

2. **Update Microsoft Entra configuration**:
   - Navigate to External ID > Identity providers > Google
   - Replace Client ID and Client Secret with previous values
   - Verify claims mapping matches previous configuration

#### Facebook Provider Rollback

1. **Revert to previous app configuration**:
   - Access [Facebook Developers](https://developers.facebook.com/)
   - Navigate to your app settings
   - Restore previous OAuth redirect URIs
   - Revert to previous App ID and Secret

2. **Update Microsoft Entra configuration**:
   - Navigate to External ID > Identity providers > Facebook
   - Replace App ID and App Secret with previous values
   - Verify permissions and claims mapping

#### Discord Provider Rollback

1. **Revert to previous application settings**:
   - Access [Discord Developer Portal](https://discord.com/developers/applications)
   - Navigate to OAuth2 settings
   - Restore previous redirect URIs
   - Revert to previous Client ID and Secret

2. **Update Microsoft Entra configuration**:
   - Navigate to External ID > Identity providers > Custom OIDC
   - Replace Client ID and Client Secret with previous values
   - Verify discovery URL and claims mapping

### Post-Rollback Validation

After completing the rollback, perform these validation steps:

```bash
# 1. Verify Terraform state is consistent
terraform plan  # Should show no changes

# 2. Verify app registrations in Azure
az ad app list --display-name "Mystira" --output table

# 3. Test authentication flows
# - Test web app login
# - Test mobile app login (if applicable)
# - Test social provider login (Google, Facebook, Discord)

# 4. Check application logs for authentication errors
# Review logs for any failed login attempts or misconfigurations

# 5. Monitor authentication metrics
# - Login success rate
# - Token issuance rate
# - Error rates
```

### Emergency Rollback (Production Critical)

If authentication is completely broken in production:

1. **Immediate Actions**:
   - Switch to backup authentication tenant (if available)
   - Update application configurations to use backup tenant
   - Notify users of temporary authentication issues

2. **Quick Restoration**:
   ```bash
   # Restore from last known good state
   terraform state pull > current.tfstate.backup
   terraform state push last-known-good.tfstate
   terraform apply -auto-approve
   ```

3. **Communication**:
   - Post status page update
   - Notify engineering team
   - Document incident for post-mortem

### Rollback Completion Checklist

- [ ] Terraform state restored or resources destroyed
- [ ] Identity provider configurations reverted
- [ ] Application configurations updated (if needed)
- [ ] Authentication flows tested and validated
- [ ] No authentication errors in application logs
- [ ] Monitoring dashboards show normal metrics
- [ ] Stakeholders notified of rollback completion
- [ ] Incident documentation completed
- [ ] Post-rollback review scheduled

### Additional Resources

- [Terraform State Management](https://www.terraform.io/docs/state/index.html)
- [Azure AD Application Management](https://docs.microsoft.com/en-us/azure/active-directory/develop/app-objects-and-service-principals)
- Repository infrastructure change guidelines: See `docs/operations/ROLLBACK_PROCEDURE.md`

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
