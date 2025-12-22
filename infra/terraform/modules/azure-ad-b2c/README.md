# Azure AD B2C Consumer Authentication Module

This Terraform module manages Azure AD B2C app registrations for consumer-facing authentication in the Mystira platform.

## Overview

This module creates and configures:

- **Public API** app registration with exposed scopes
- **PWA/SPA** app registration with API permissions
- **API Scopes** for consumer operations

## Prerequisites

**IMPORTANT**: Azure AD B2C tenant creation is not supported via Terraform. You must:

1. Create the B2C tenant manually in Azure Portal
2. Configure user flows (Sign-up/Sign-in, Password Reset, Profile Edit)
3. Set up identity providers (Google, Discord) in the B2C tenant
4. Then run this Terraform module to create app registrations

### Manual B2C Setup Steps

1. **Create B2C Tenant**:
   ```
   Azure Portal → Create a resource → Azure Active Directory B2C → Create
   - Organization name: Mystira B2C
   - Domain name: mystirab2c.onmicrosoft.com
   ```

2. **Create User Flows**:
   ```
   Azure Portal → Azure AD B2C → User flows → New user flow
   - B2C_1_SignUpSignIn (Sign up and sign in)
   - B2C_1_PasswordReset (Password reset)
   - B2C_1_ProfileEdit (Profile editing)
   ```

3. **Configure Identity Providers**:

   **Google**:
   - Create OAuth credentials at Google Cloud Console
   - Azure Portal → Azure AD B2C → Identity providers → Google
   - Add Client ID and Secret

   **Discord**:
   - Create application at Discord Developer Portal
   - Azure Portal → Azure AD B2C → Identity providers → OpenID Connect
   - Metadata URL: https://discord.com/.well-known/openid-configuration

## Usage

```hcl
module "azure_ad_b2c" {
  source = "../../modules/azure-ad-b2c"

  environment     = "dev"
  b2c_tenant_name = "mystirab2c"

  pwa_redirect_uris = [
    "http://localhost:5173/auth/callback",
    "https://mystira.app/auth/callback"
  ]
}
```

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| environment | Deployment environment | `string` | n/a | yes |
| b2c_tenant_name | B2C tenant name | `string` | `"mystirab2c"` | no |
| pwa_redirect_uris | PWA redirect URIs | `list(string)` | `[]` | no |
| sign_up_sign_in_policy | Sign-up/sign-in policy name | `string` | `"B2C_1_SignUpSignIn"` | no |
| password_reset_policy | Password reset policy name | `string` | `"B2C_1_PasswordReset"` | no |

## Outputs

| Name | Description |
|------|-------------|
| public_api_client_id | Public API application (client) ID |
| pwa_client_id | PWA application (client) ID |
| api_scopes | Map of API scopes |
| public_api_config | Configuration for Public API |
| pwa_config | Configuration for PWA |
| user_flow_urls | B2C user flow URLs |

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
  "AzureAdB2C": {
    "Instance": "https://mystirab2c.b2clogin.com",
    "Domain": "mystirab2c.onmicrosoft.com",
    "ClientId": "<public_api_client_id>",
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn"
  }
}
```

### Blazor WASM PWA

```csharp
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(
        "https://mystirab2c.onmicrosoft.com/mystira-api/API.Access");
});
```

### React/Vite PWA (.env)

```bash
VITE_B2C_CLIENT_ID=<pwa_client_id>
VITE_B2C_TENANT_NAME=mystirab2c
VITE_B2C_AUTHORITY=https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/B2C_1_SignUpSignIn
VITE_B2C_API_SCOPE=https://mystirab2c.onmicrosoft.com/mystira-api/API.Access
```

## Social Login Redirect URIs

Configure these in your identity providers:

- **Google**: `https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/oauth2/authresp`
- **Discord**: `https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/oauth2/authresp`

## Related Documentation

- [ADR-0011: Entra ID Authentication Integration](../../../docs/architecture/adr/0011-entra-id-authentication-integration.md)
- [Azure AD B2C Documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)
- [B2C User Flows](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview)
