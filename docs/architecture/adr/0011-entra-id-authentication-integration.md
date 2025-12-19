# ADR-0011: Microsoft Entra ID Authentication Integration

## Status

**Proposed** - 2025-12-19

## Context

Building on [ADR-0010: Authentication and Authorization Strategy](./0010-authentication-and-authorization-strategy.md), this ADR specifically addresses the integration of Microsoft Entra ID (formerly Azure Active Directory) as the enterprise identity provider for the Mystira platform.

### Current State

The Mystira platform currently uses:

- **Cookie-based authentication** for Admin UI (internal sessions)
- **JWT tokens** for Public API (stateless authentication)
- **Shared JWT services** in `Mystira.App.Shared` NuGet package

### Business Drivers

1. **Enterprise SSO**: Organizations managing Mystira installations need single sign-on
2. **Admin Security**: Admin users require stronger authentication (MFA, conditional access)
3. **Azure Integration**: Platform runs on Azure, making Entra ID a natural choice
4. **Compliance**: Enterprise customers require identity provider integration for audit/compliance

### Requirements

1. **Admin Authentication**: Admin API and Admin UI must support Entra ID sign-in
2. **Service-to-Service**: Azure services must authenticate using Managed Identities
3. **Multi-tenant Support**: Support both single-tenant (enterprise) and multi-tenant (SaaS) scenarios
4. **B2C Option**: Consumer-facing apps may use Azure AD B2C for social login
5. **Backward Compatibility**: Maintain existing auth for non-enterprise deployments

## Decision

We adopt **Microsoft Entra ID** as the enterprise identity provider with the following architecture:

### 1. Authentication Tiers

```
┌─────────────────────────────────────────────────────────────────┐
│                    Authentication Tiers                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────┐  │
│  │   Tier 1    │    │   Tier 2    │    │      Tier 3         │  │
│  │  Internal   │    │  Consumer   │    │    Enterprise       │  │
│  ├─────────────┤    ├─────────────┤    ├─────────────────────┤  │
│  │ Admin API   │    │ Public API  │    │ Enterprise SSO      │  │
│  │ Admin UI    │    │ PWA         │    │ B2B Collaboration   │  │
│  │ DevHub      │    │ Publisher   │    │ Managed Partners    │  │
│  ├─────────────┤    ├─────────────┤    ├─────────────────────┤  │
│  │ Entra ID    │    │ Entra B2C   │    │ Entra ID External   │  │
│  │ (Workforce) │    │ (Consumer)  │    │ (Guests)            │  │
│  └─────────────┘    └─────────────┘    └─────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 2. App Registration Strategy

#### Admin Applications (Tier 1)

**App Registration: `mystira-admin-api`**

```
Display Name: Mystira Admin API
Application ID URI: api://mystira-admin-api
Supported Account Types: Single tenant (organization only)
```

**Exposed API Scopes**:

| Scope | Description | Admin Consent |
|-------|-------------|---------------|
| `Admin.Read` | Read admin data | Yes |
| `Admin.Write` | Modify admin data | Yes |
| `Users.Manage` | Manage platform users | Yes |
| `Content.Moderate` | Moderate content | Yes |

**App Registration: `mystira-admin-ui`**

```
Display Name: Mystira Admin UI
Redirect URIs:
  - https://admin.mystira.app/auth/callback
  - http://localhost:7001/auth/callback (dev)
Supported Account Types: Single tenant
```

#### Consumer Applications (Tier 2)

**Azure AD B2C Tenant: `mystirab2c.onmicrosoft.com`**

**User Flows**:

| Flow | Type | Features |
|------|------|----------|
| `B2C_1_SignUpSignIn` | Combined | Email/password, Social (Google, Discord) |
| `B2C_1_PasswordReset` | Self-service | Email verification |
| `B2C_1_ProfileEdit` | Edit profile | Update display name, avatar |

**App Registration: `mystira-public-api`**

```
Display Name: Mystira Public API
Application ID URI: https://mystirab2c.onmicrosoft.com/mystira-api
Supported Account Types: B2C tenant accounts
```

### 3. Authentication Flows

#### Admin OIDC Flow (PKCE)

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────┐
│Admin UI │     │   Entra ID   │     │  Admin API  │     │ Cosmos DB│
└────┬────┘     └──────┬───────┘     └──────┬──────┘     └────┬─────┘
     │                 │                    │                  │
     │  1. Redirect to /authorize           │                  │
     │────────────────▶│                    │                  │
     │                 │                    │                  │
     │  2. User signs in + MFA              │                  │
     │◀────────────────│                    │                  │
     │                 │                    │                  │
     │  3. Auth code + code_verifier        │                  │
     │────────────────▶│                    │                  │
     │                 │                    │                  │
     │  4. ID token + access token          │                  │
     │◀────────────────│                    │                  │
     │                 │                    │                  │
     │  5. API request with access token    │                  │
     │─────────────────────────────────────▶│                  │
     │                 │                    │                  │
     │                 │  6. Validate token │                  │
     │                 │◀───────────────────│                  │
     │                 │                    │                  │
     │                 │                    │  7. Query data   │
     │                 │                    │─────────────────▶│
     │                 │                    │                  │
     │  8. Response                         │◀─────────────────│
     │◀─────────────────────────────────────│                  │
```

#### Service-to-Service (Managed Identity)

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│  Admin API  │     │   Entra ID   │     │  Cosmos DB  │
│ (App Svc)   │     │   (IMDS)     │     │             │
└──────┬──────┘     └──────┬───────┘     └──────┬──────┘
       │                   │                    │
       │ 1. Request token  │                    │
       │   (Managed ID)    │                    │
       │──────────────────▶│                    │
       │                   │                    │
       │ 2. Access token   │                    │
       │◀──────────────────│                    │
       │                   │                    │
       │ 3. Request with token                  │
       │───────────────────────────────────────▶│
       │                   │                    │
       │ 4. Response       │                    │
       │◀───────────────────────────────────────│
```

#### Azure AD B2C Sign-Up/Sign-In Flow (Consumer)

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────┐
│   PWA   │     │   Azure AD   │     │  Public API │     │ Cosmos DB│
│ (Blazor)│     │     B2C      │     │             │     │          │
└────┬────┘     └──────┬───────┘     └──────┬──────┘     └────┬─────┘
     │                 │                    │                  │
     │  1. User clicks "Sign In"            │                  │
     │────────────────▶│                    │                  │
     │                 │                    │                  │
     │  2. Redirect to B2C login page       │                  │
     │◀────────────────│                    │                  │
     │                 │                    │                  │
     │  ┌──────────────────────────────┐    │                  │
     │  │  B2C Hosted UI               │    │                  │
     │  │  ┌────────────────────────┐  │    │                  │
     │  │  │ Sign in with:         │  │    │                  │
     │  │  │ [Google] [Discord]    │  │    │                  │
     │  │  │ ─────────────────────  │  │    │                  │
     │  │  │ Email: [          ]   │  │    │                  │
     │  │  │ Password: [       ]   │  │    │                  │
     │  │  │ [Sign In] [Sign Up]   │  │    │                  │
     │  │  └────────────────────────┘  │    │                  │
     │  └──────────────────────────────┘    │                  │
     │                 │                    │                  │
     │  3. User authenticates (email/social)│                  │
     │────────────────▶│                    │                  │
     │                 │                    │                  │
     │  4. B2C validates, runs user flow    │                  │
     │                 │                    │                  │
     │  5. Redirect with auth code          │                  │
     │◀────────────────│                    │                  │
     │                 │                    │                  │
     │  6. Exchange code for tokens         │                  │
     │────────────────▶│                    │                  │
     │                 │                    │                  │
     │  7. ID token + access token + refresh│                  │
     │◀────────────────│                    │                  │
     │                 │                    │                  │
     │  8. API request with access token    │                  │
     │─────────────────────────────────────▶│                  │
     │                 │                    │                  │
     │                 │  9. Validate B2C token                │
     │                 │◀───────────────────│                  │
     │                 │                    │                  │
     │                 │                    │ 10. Query user   │
     │                 │                    │─────────────────▶│
     │                 │                    │                  │
     │ 11. Response with user data          │◀─────────────────│
     │◀─────────────────────────────────────│                  │
```

#### B2C Token Refresh Flow

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐
│   PWA   │     │   Azure AD   │     │  Public API │
│         │     │     B2C      │     │             │
└────┬────┘     └──────┬───────┘     └──────┬──────┘
     │                 │                    │
     │  1. Access token expired             │
     │  (401 from API)                      │
     │◀─────────────────────────────────────│
     │                 │                    │
     │  2. POST /token with refresh_token   │
     │────────────────▶│                    │
     │                 │                    │
     │  3. Validate refresh token           │
     │                 │                    │
     │  4. New access token + refresh token │
     │◀────────────────│                    │
     │                 │                    │
     │  5. Retry API request                │
     │─────────────────────────────────────▶│
     │                 │                    │
     │  6. Success response                 │
     │◀─────────────────────────────────────│
```

#### B2C Social Login Flow (Google/Discord)

```
┌─────────┐     ┌──────────┐     ┌──────────────┐     ┌─────────────┐
│   PWA   │     │  B2C UI  │     │   Identity   │     │  Public API │
│         │     │          │     │   Provider   │     │             │
└────┬────┘     └────┬─────┘     └──────┬───────┘     └──────┬──────┘
     │               │                  │                    │
     │ 1. Click social login button     │                    │
     │──────────────▶│                  │                    │
     │               │                  │                    │
     │               │ 2. Redirect to IdP                    │
     │               │─────────────────▶│                    │
     │               │                  │                    │
     │               │ 3. User authenticates with IdP        │
     │               │                  │                    │
     │               │ 4. IdP returns auth code              │
     │               │◀─────────────────│                    │
     │               │                  │                    │
     │               │ 5. Exchange for IdP tokens            │
     │               │─────────────────▶│                    │
     │               │                  │                    │
     │               │ 6. IdP tokens (user info)             │
     │               │◀─────────────────│                    │
     │               │                  │                    │
     │ 7. B2C creates/links user, issues tokens              │
     │◀──────────────│                  │                    │
     │               │                  │                    │
     │ 8. API call with B2C token       │                    │
     │──────────────────────────────────────────────────────▶│
     │               │                  │                    │
     │ 9. Success                       │                    │
     │◀──────────────────────────────────────────────────────│
```

### 4. Implementation Details

#### ASP.NET Core Configuration

**Admin API (appsettings.json)**:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "Audience": "api://mystira-admin-api",
    "CallbackPath": "/signin-oidc"
  }
}
```

**Program.cs Configuration**:

```csharp
// Add Microsoft Identity Platform authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));

    options.AddPolicy("ContentModerator", policy =>
        policy.RequireClaim("extension_role", "Moderator", "Admin"));
});
```

#### React Admin UI Configuration

**MSAL Configuration (authConfig.ts)**:

```typescript
import { Configuration, LogLevel } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_REDIRECT_URI,
    postLogoutRedirectUri: import.meta.env.VITE_POST_LOGOUT_URI,
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
    },
  },
};

export const loginRequest = {
  scopes: ['api://mystira-admin-api/Admin.Read'],
};

export const apiScopes = {
  admin: ['api://mystira-admin-api/Admin.Read', 'api://mystira-admin-api/Admin.Write'],
  users: ['api://mystira-admin-api/Users.Manage'],
  content: ['api://mystira-admin-api/Content.Moderate'],
};
```

**Auth Provider (App.tsx)**:

```typescript
import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from './authConfig';

const msalInstance = new PublicClientApplication(msalConfig);

function App() {
  return (
    <MsalProvider instance={msalInstance}>
      <AuthenticatedApp />
    </MsalProvider>
  );
}
```

### 5. Role Mapping

#### Entra ID App Roles

Define in Azure Portal → App Registration → App roles:

| Role | Value | Description | Allowed Members |
|------|-------|-------------|-----------------|
| Admin | `Admin` | Full admin access | Users, Groups |
| SuperAdmin | `SuperAdmin` | System-level access | Users |
| Moderator | `Moderator` | Content moderation | Users, Groups |
| Viewer | `Viewer` | Read-only access | Users, Groups |

#### Group-to-Role Mapping

| Entra ID Group | App Role | Description |
|----------------|----------|-------------|
| `mystira-admins` | Admin | Platform administrators |
| `mystira-superadmins` | SuperAdmin | System administrators |
| `mystira-moderators` | Moderator | Content moderators |
| `mystira-viewers` | Viewer | Read-only staff |

### 6. Conditional Access Policies

**Policy: Admin MFA Requirement**

```yaml
Name: Require MFA for Mystira Admin
Assignments:
  Users: mystira-admins, mystira-superadmins
  Cloud apps: mystira-admin-api, mystira-admin-ui
Conditions:
  Locations: All locations
Access controls:
  Grant: Require MFA
Session:
  Sign-in frequency: 8 hours
```

**Policy: Block Legacy Auth**

```yaml
Name: Block Legacy Authentication
Assignments:
  Users: All users
  Cloud apps: mystira-admin-api
Conditions:
  Client apps: Legacy authentication clients
Access controls:
  Block: Yes
```

### 7. Token Configuration

#### Claims Mapping

| Claim | Source | Usage |
|-------|--------|-------|
| `sub` | Object ID | User identifier |
| `name` | Display name | UI display |
| `email` | UPN or mail | Contact/notifications |
| `roles` | App roles | Authorization |
| `groups` | Group membership | Fine-grained access |
| `tid` | Tenant ID | Multi-tenant routing |

#### Token Lifetimes

| Token | Lifetime | Refresh |
|-------|----------|---------|
| Access Token | 1 hour | Yes |
| ID Token | 1 hour | No |
| Refresh Token | 14 days | Rolling |
| Session (Browser) | 8 hours | Sliding |

### 8. Environment Variables

```bash
# Admin API
AZURE_AD_INSTANCE=https://login.microsoftonline.com/
AZURE_AD_TENANT_ID=your-tenant-id
AZURE_AD_CLIENT_ID=your-client-id
AZURE_AD_CLIENT_SECRET=@Microsoft.KeyVault(...)
AZURE_AD_AUDIENCE=api://mystira-admin-api

# Admin UI
VITE_AZURE_CLIENT_ID=your-ui-client-id
VITE_AZURE_TENANT_ID=your-tenant-id
VITE_REDIRECT_URI=https://admin.mystira.app/auth/callback
VITE_POST_LOGOUT_URI=https://admin.mystira.app

# B2C (Public API)
AZURE_B2C_INSTANCE=https://mystirab2c.b2clogin.com
AZURE_B2C_TENANT=mystirab2c.onmicrosoft.com
AZURE_B2C_POLICY=B2C_1_SignUpSignIn
AZURE_B2C_CLIENT_ID=your-b2c-client-id
```

## Rationale

### Why Entra ID for Admin?

1. **Security**: MFA, Conditional Access, Identity Protection
2. **Integration**: Native Azure service integration
3. **Compliance**: Enterprise-grade audit logs
4. **Management**: Centralized identity governance

### Why B2C for Consumer?

1. **Social Login**: Google, Discord, custom providers
2. **Branding**: Custom login experience
3. **Self-Service**: User registration and password reset
4. **Scalability**: Consumer-scale identity management

### Why Managed Identity for Services?

1. **No Secrets**: No credentials to manage or rotate
2. **Automatic**: Azure handles token refresh
3. **Secure**: Tokens scoped to specific resources
4. **Auditable**: All access logged in Entra ID

## Consequences

### Positive

1. **Enterprise Ready**: SSO integration for organizations
2. **Secure**: MFA, Conditional Access, Passwordless options
3. **Compliant**: Audit logs, identity governance
4. **Scalable**: Handles millions of users (B2C)
5. **Integrated**: Native Azure service authentication

### Negative

1. **Complexity**: Multiple tenants (workforce + B2C)
2. **Cost**: B2C pricing per MAU
3. **Azure Lock-in**: Tight coupling to Azure identity
4. **Learning Curve**: Entra ID configuration

### Mitigations

1. **Documentation**: Comprehensive setup guides
2. **Terraform**: Infrastructure as Code for reproducibility
3. **Abstraction**: Auth middleware abstracts provider details
4. **Fallback**: Keep existing auth for non-enterprise deployments

## Implementation Checklist

### Phase 1: Admin Authentication

- [ ] Create Entra ID App Registration for Admin API
- [ ] Create Entra ID App Registration for Admin UI
- [ ] Configure MSAL in Admin UI
- [ ] Add Microsoft.Identity.Web to Admin API
- [ ] Define App Roles
- [ ] Create Conditional Access policies
- [ ] Test MFA flow

### Phase 2: Service Authentication

- [ ] Enable Managed Identity on App Services
- [ ] Configure Cosmos DB for Entra ID auth
- [ ] Update Key Vault access policies
- [ ] Remove connection string authentication
- [ ] Test service-to-service auth

### Phase 3: Consumer Authentication (B2C)

- [ ] Create Azure AD B2C tenant
- [ ] Configure user flows
- [ ] Set up social identity providers
- [ ] Create App Registration for Public API
- [ ] Update PWA for B2C authentication
- [ ] Test consumer sign-up/sign-in

## Azure AD B2C Setup Guide

### 1. Create B2C Tenant

```bash
# Via Azure CLI
az ad b2c tenant create \
  --display-name "Mystira B2C" \
  --domain-name "mystirab2c.onmicrosoft.com" \
  --country-code "US"
```

### 2. Configure Identity Providers

#### Google Identity Provider

1. Create OAuth credentials at [Google Cloud Console](https://console.cloud.google.com/)
2. Configure in Azure Portal:

```
Azure Portal → Azure AD B2C → Identity providers → Google
├── Client ID: [from Google Console]
├── Client Secret: [from Google Console]
└── Scope: openid profile email
```

**Redirect URI for Google**: `https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/oauth2/authresp`

#### Discord Identity Provider

1. Create application at [Discord Developer Portal](https://discord.com/developers/applications)
2. Configure as OpenID Connect provider:

```
Azure Portal → Azure AD B2C → Identity providers → OpenID Connect
├── Name: Discord
├── Metadata URL: https://discord.com/.well-known/openid-configuration
├── Client ID: [from Discord]
├── Client Secret: [from Discord]
├── Scope: identify email
├── Response type: code
└── Claims mapping:
    ├── User ID: sub
    ├── Display name: username
    └── Email: email
```

**Redirect URI for Discord**: `https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/oauth2/authresp`

### 3. User Flow Configuration

#### Sign-up/Sign-in Flow (B2C_1_SignUpSignIn)

```yaml
Name: B2C_1_SignUpSignIn
Identity providers:
  - Local Account (Email)
  - Google
  - Discord
User attributes to collect:
  - Email Address (required)
  - Display Name (required)
Application claims to return:
  - User's Object ID
  - Email Addresses
  - Display Name
  - Identity Provider
  - Identity Provider Access Token
Page layouts:
  - Sign-up or sign-in page: Ocean Blue (or custom)
  - Local account sign-up page: Ocean Blue
```

#### Password Reset Flow (B2C_1_PasswordReset)

```yaml
Name: B2C_1_PasswordReset
Identity providers:
  - Local Account (Email)
User attributes:
  - Email Address
Application claims:
  - User's Object ID
  - Email Addresses
```

### 4. B2C ASP.NET Core Configuration

**appsettings.json**:

```json
{
  "AzureAdB2C": {
    "Instance": "https://mystirab2c.b2clogin.com",
    "Domain": "mystirab2c.onmicrosoft.com",
    "TenantId": "YOUR_B2C_TENANT_ID",
    "ClientId": "YOUR_B2C_CLIENT_ID",
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn",
    "ResetPasswordPolicyId": "B2C_1_PasswordReset",
    "EditProfilePolicyId": "B2C_1_ProfileEdit"
  }
}
```

**Program.cs**:

```csharp
// Add B2C authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.TokenValidationParameters.NameClaimType = "name";
    },
    options => { builder.Configuration.Bind("AzureAdB2C", options); });

// Configure CORS for B2C
builder.Services.AddCors(options =>
{
    options.AddPolicy("B2CPolicy", policy =>
    {
        policy.WithOrigins(
            "https://mystira.app",
            "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

### 5. Blazor WASM B2C Configuration

**wwwroot/appsettings.json**:

```json
{
  "AzureAdB2C": {
    "Authority": "https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/B2C_1_SignUpSignIn",
    "ClientId": "YOUR_B2C_CLIENT_ID",
    "ValidateAuthority": false
  }
}
```

**Program.cs**:

```csharp
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(
        "https://mystirab2c.onmicrosoft.com/mystira-api/API.Access");
    options.ProviderOptions.LoginMode = "redirect";
});
```

## Setup Documentation Links

### Secrets Management

- **[Secrets Management Guide](../../packages/app/docs/setup/secrets-management.md)** - How to manage secrets locally and in production
- **[Quick Secrets Reference](../../packages/app/docs/setup/quick-secrets-reference.md)** - Quick lookup for common secrets
- **[Kubernetes Secrets](../infrastructure/kubernetes-secrets-management.md)** - K8s secret management for deployed services
- **[GitHub Secrets/Variables](../../packages/app/docs/setup/github-secrets-variables.md)** - CI/CD secrets configuration

### Infrastructure Setup

- **[Infrastructure Quick Start](../infrastructure/QUICK_START_DEPLOY.md)** - Quick deployment guide
- **[Terraform Backend Setup](../infrastructure/TERRAFORM_BACKEND_SETUP.md)** - IaC backend configuration
- **[Shared Resources](../infrastructure/shared-resources.md)** - Shared Azure resources

### Application Setup

- **[Environment Configuration](../ENVIRONMENT.md)** - Environment variable reference
- **[Database Setup](../../packages/app/docs/setup/database-setup.md)** - Cosmos DB configuration
- **[Email Setup](../../packages/app/docs/setup/email-setup.md)** - Azure Communication Services

### Security

- **[Security Policy](../../SECURITY.md)** - Security guidelines and contacts
- **[ADR-0010: Auth Strategy](./0010-authentication-and-authorization-strategy.md)** - Overall authentication strategy

## Related ADRs

- [ADR-0010: Authentication and Authorization Strategy](./0010-authentication-and-authorization-strategy.md) - Overall auth strategy
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md) - Service security
- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md) - Admin service architecture

## References

- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [MSAL.js for React](https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-react)
- [Microsoft.Identity.Web](https://docs.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [Azure AD B2C Documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)
- [B2C Custom Policies](https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview)
- [B2C Identity Providers](https://docs.microsoft.com/en-us/azure/active-directory-b2c/add-identity-provider)
- [Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
- [Conditional Access](https://docs.microsoft.com/en-us/azure/active-directory/conditional-access/)
