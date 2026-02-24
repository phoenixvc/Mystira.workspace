# ADR-0011: Microsoft Entra ID Authentication Integration

## Status

**Implemented** - 2026-02-27

## Context

Building on [ADR-0010: Authentication and Authorization Strategy](./0010-authentication-and-authorization-strategy.md), this ADR documents the implementation of Microsoft Entra ID integration through our centralized Identity API service. The Mystira platform now uses a unified authentication architecture with a dedicated Identity service handling both workforce (Entra ID) and consumer (magic link + optional Entra) authentication flows.

### Current Architecture

The Mystira platform now uses:

- **Centralized Identity API** (`packages/identity/src/Mystira.Identity.Api`) for all authentication
- **UnifiedAuthService** in client applications for dual-path authentication
- **Magic link authentication** as primary consumer flow with optional Entra ID
- **Entra ID integration** for workforce authentication and optional consumer social login
- **JWT token service** for cross-service authentication

### Business Drivers

1. **Unified Auth**: Single authentication authority across all services
2. **Consumer-First**: Magic link authentication with optional social providers
3. **Enterprise Ready**: Entra ID integration for workforce scenarios
4. **Service-to-Service**: Managed identity authentication for Azure services
5. **Flexibility**: Support both B2C and B2E scenarios in the same platform

## Decision

We implement a **Centralized Identity Service** with Microsoft Entra ID integration using the following architecture:

### 1. Authentication Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Centralized Identity                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │              Identity API Service                          │  │
│  │  packages/identity/src/Mystira.Identity.Api               │  │
│  ├─────────────────────────────────────────────────────────────┤  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │  │
│  │  │   Magic     │  │   Entra     │  │   Token Service     │  │  │
│  │  │   Auth      │  │   ID        │  │   (JWT)             │  │  │
│  │  │             │  │             │  │                     │  │  │
│  │  │ • Email     │  │ • Workforce │  │ • Access Tokens     │  │  │
│  │  │ • Magic     │  │ • Social    │  │ • Refresh Tokens    │  │  │
│  │  │ • TTL 30m   │  │ • Optional  │  │ • Validation        │  │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────┘  │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                 Client Applications                         │  │
│  │  UnifiedAuthService (dual-path auth)                       │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │  │
│  │  │    App PWA  │  │ Admin UI    │  │   Publisher UI      │  │  │
│  │  │             │  │             │  │                     │  │  │
│  │  │ • Magic     │  │ • Entra     │  │ • Magic + Entra     │  │  │
│  │  │ • Entra     │  │ • Workforce │  │ • Optional Social   │  │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────┘  │  │
│  └─────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 2. Centralized Identity API

**Location**: `packages/identity/src/Mystira.Identity.Api`

**Core Services**:

- `IdentityAuthController` - Authentication endpoints
- `IIdentityTokenService` - JWT token management
- `IEntraProvisioningService` - Entra ID user provisioning
- `IProvisioningQueue` - Background provisioning queue
- `ProvisioningBackgroundWorker` - Async user provisioning

**Authentication Flows**:

| Scope              | Description           | Admin Consent |
| ------------------ | --------------------- | ------------- |
| `Admin.Read`       | Read admin data       | Yes           |
| `Admin.Write`      | Modify admin data     | Yes           |
| `Users.Manage`     | Manage platform users | Yes           |
| `Content.Moderate` | Moderate content      | Yes           |

### 3. Client-Side Unified Authentication

**UnifiedAuthService** Features:

- Dual-path authentication (Magic + Entra)
- Automatic token refresh and expiry handling
- Provider switching without session loss
- Centralized auth state management
- Event-driven auth state notifications

**Microsoft Entra External ID Tenant: `mystira.ciamlogin.com`**

**Sign-in Experience**:

| Feature          | Description                                                |
| ---------------- | ---------------------------------------------------------- |
| Sign-up/Sign-in  | Combined email/password and social provider authentication |
| Password Reset   | Self-service via email verification                        |
| Profile Edit     | User can update display name and avatar                    |
| Social Providers | Google OAuth 2.0, Discord OpenID Connect                   |

**App Registration: `mystira-public-api`**

```
Display Name: Mystira Public API
Application ID URI: api://mystira-api
Supported Account Types: External ID tenant accounts (AzureADandPersonalMicrosoftAccount)
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

#### External ID Sign-Up/Sign-In Flow (Consumer)

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐     ┌──────────┐
│   PWA   │     │  External ID │     │  Public API │     │ Cosmos DB│
│ (Blazor)│     │  (Consumer)  │     │             │     │          │
└────┬────┘     └──────┬───────┘     └──────┬──────┘     └────┬─────┘
     │                 │                    │                  │
     │  1. User clicks "Sign In"            │                  │
     │────────────────▶│                    │                  │
     │                 │                    │                  │
     │  2. Redirect to External ID login    │                  │
     │◀────────────────│                    │                  │
     │                 │                    │                  │
     │  ┌──────────────────────────────┐    │                  │
     │  │  External ID Hosted UI       │    │                  │
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
     │  4. External ID validates user        │                  │
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
     │                 │  9. Validate External ID token        │
     │                 │◀───────────────────│                  │
     │                 │                    │                  │
     │                 │                    │ 10. Query user   │
     │                 │                    │─────────────────▶│
     │                 │                    │                  │
     │ 11. Response with user data          │◀─────────────────│
     │◀─────────────────────────────────────│                  │
```

#### External ID Token Refresh Flow

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐
│   PWA   │     │  External ID │     │  Public API │
│         │     │   (CIAM)     │     │             │
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

#### External ID Social Login Flow (Google/Discord)

```
┌─────────┐     ┌──────────┐     ┌──────────────┐     ┌─────────────┐
│   PWA   │     │ Ext. ID  │     │   Identity   │     │  Public API │
│         │     │   UI     │     │   Provider   │     │             │
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
     │ 7. External ID creates/links user, issues tokens       │
     │◀──────────────│                  │                    │
     │               │                  │                    │
     │ 8. API call with External ID token                    │
     │──────────────────────────────────────────────────────▶│
     │               │                  │                    │
     │ 9. Success                       │                    │
     │◀──────────────────────────────────────────────────────│
```

### 4. Implementation Details

#### Magic Link Authentication Flow (Primary Consumer)

```text
┌─────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌──────────┐
│   PWA   │     │  Identity API   │     │   Email Service │     │ Database │
└────┬────┘     └────────┬────────┘     └────────┬────────┘     └────┬─────┘
     │                  │                        │                    │
     │ 1. Request magic token                     │                    │
     │──────────────────▶│                        │                    │
     │                  │                        │                    │
     │ 2. Generate token + send email             │                    │
     │                  │────────────────────────▶│                    │
     │                  │                        │                    │
     │ 3. Email with magic link                    │                    │
     │◀──────────────────│─────────────────────────│                    │
     │                  │                        │                    │
     │ 4. User clicks link, validates token       │                    │
     │──────────────────▶│                        │                    │
     │                  │ 5. Validate token      │                    │
     │                  │────────────────────────▶│                    │
     │                  │                        │                    │
     │ 6. Return JWT tokens                      │                    │
     │◀──────────────────│                        │                    │
```

#### Entra ID Workforce Authentication Flow

```text
┌─────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌──────────┐
│Admin UI │     │  Identity API   │     │   Entra ID      │     │ Database │
└────┬────┘     └────────┬────────┘     └────────┬────────┘     └────┬─────┘
     │                  │                        │                    │
     │ 1. Redirect to Entra ID                   │                    │
     │──────────────────▶│                        │                    │
     │                  │                        │                    │
     │ 2. User authenticates + MFA               │                    │
     │◀──────────────────│─────────────────────────│                    │
     │                  │                        │                    │
     │ 3. Exchange code for tokens                │                    │
     │──────────────────▶│                        │                    │
     │                  │ 4. Validate with Entra │                    │
     │                  │────────────────────────▶│                    │
     │                  │                        │                    │
     │ 5. Provision user (if needed)              │                    │
     │                  │────────────────────────▶│                    │
     │                  │                        │                    │
     │ 6. Return JWT tokens                      │                    │
     │◀──────────────────│                        │                    │
```

#### Centralized Identity API Configuration

**appsettings.json**:

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "${ENTRA_TENANT_ID}",
    "ClientId": "${ENTRA_CLIENT_ID}",
    "ClientSecret": "${ENTRA_CLIENT_SECRET}",
    "Domain": "${ENTRA_DOMAIN}"
  },
  "MagicAuth": {
    "TokenExpirationMinutes": 30,
    "PendingSignupExpirationDays": 7
  },
  "Jwt": {
    "Issuer": "Mystira.Identity",
    "Audience": "Mystira.Services",
    "ExpirationMinutes": 60,
    "RefreshExpirationDays": 7
  }
}
```

#### UnifiedAuthService Client Implementation

**Key Features**:

````csharp
public class UnifiedAuthService : IAuthService
{
    // Dual authentication providers
    private readonly EntraExternalIdAuthService _entraAuthService;
    private readonly IMagicAuthApiClient _magicAuthClient;

    // Automatic token management
    public async Task<bool> EnsureTokenValidAsync();
    public event EventHandler<bool>? AuthenticationStateChanged;
    public event EventHandler? TokenExpiryWarning;

#### React Admin UI Configuration

**MSAL Configuration (authConfig.ts)**:

```typescript
import { Configuration, LogLevel } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_REDIRECT_URI,
    postLogoutRedirectUri: import.meta.env.VITE_POST_LOGOUT_URI,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
    },
  },
};

export const loginRequest = {
  scopes: ["api://mystira-admin-api/Admin.Read"],
};

export const apiScopes = {
  admin: [
    "api://mystira-admin-api/Admin.Read",
    "api://mystira-admin-api/Admin.Write",
  ],
  users: ["api://mystira-admin-api/Users.Manage"],
  content: ["api://mystira-admin-api/Content.Moderate"],
};
````

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

| Role       | Value        | Description         | Allowed Members |
| ---------- | ------------ | ------------------- | --------------- |
| Admin      | `Admin`      | Full admin access   | Users, Groups   |
| SuperAdmin | `SuperAdmin` | System-level access | Users           |
| Moderator  | `Moderator`  | Content moderation  | Users, Groups   |
| Viewer     | `Viewer`     | Read-only access    | Users, Groups   |

#### Group-to-Role Mapping

| Entra ID Group        | App Role   | Description             |
| --------------------- | ---------- | ----------------------- |
| `mystira-admins`      | Admin      | Platform administrators |
| `mystira-superadmins` | SuperAdmin | System administrators   |
| `mystira-moderators`  | Moderator  | Content moderators      |
| `mystira-viewers`     | Viewer     | Read-only staff         |

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

| Claim    | Source           | Usage                 |
| -------- | ---------------- | --------------------- |
| `sub`    | Object ID        | User identifier       |
| `name`   | Display name     | UI display            |
| `email`  | UPN or mail      | Contact/notifications |
| `roles`  | App roles        | Authorization         |
| `groups` | Group membership | Fine-grained access   |
| `tid`    | Tenant ID        | Multi-tenant routing  |

#### Token Lifetimes

| Token             | Lifetime | Refresh |
| ----------------- | -------- | ------- |
| Access Token      | 1 hour   | Yes     |
| ID Token          | 1 hour   | No      |
| Refresh Token     | 14 days  | Rolling |
| Session (Browser) | 8 hours  | Sliding |

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

# External ID (Public API)
AZURE_EXTERNAL_ID_AUTHORITY=https://mystira.ciamlogin.com/your-tenant-id/v2.0
AZURE_EXTERNAL_ID_CLIENT_ID=your-external-id-client-id
```

## Rationale

### Why Entra ID for Admin?

1. **Security**: MFA, Conditional Access, Identity Protection
2. **Integration**: Native Azure service integration
3. **Compliance**: Enterprise-grade audit logs
4. **Management**: Centralized identity governance

### Why External ID for Consumer?

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
4. **Scalable**: Handles millions of users (External ID)
5. **Integrated**: Native Azure service authentication

### Negative

1. **Complexity**: Multiple tenants (workforce + External ID)
2. **Cost**: External ID pricing per MAU
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

### Phase 3: Consumer Authentication (External ID)

- [ ] Create Microsoft Entra External ID tenant
- [ ] Configure user flows
- [ ] Set up social identity providers
- [ ] Create App Registration for Public API
- [ ] Update PWA for External ID authentication
- [ ] Test consumer sign-up/sign-in

## Microsoft Entra External ID Setup Guide

### 1. Create External ID Tenant

```bash
# Via Azure Portal (CLI not fully supported)
# Azure Portal → Create a resource → Microsoft Entra External ID → Create
# - Display Name: Mystira External ID
# - Domain name: mystirab2c.onmicrosoft.com
# - Country: US
```

### 2. Configure Identity Providers

#### Google Identity Provider

1. Create OAuth credentials at [Google Cloud Console](https://console.cloud.google.com/)
2. Configure in Azure Portal:

```
Azure Portal → External Identities → All identity providers → Google
├── Client ID: [from Google Console]
├── Client Secret: [from Google Console]
└── Scope: openid profile email
```

**Redirect URI for Google**: `https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/oauth2/authresp`

#### Discord Identity Provider

1. Create application at [Discord Developer Portal](https://discord.com/developers/applications)
2. Configure as OpenID Connect provider:

```
Azure Portal → External Identities → All identity providers → OpenID Connect
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

### 4. External ID ASP.NET Core Configuration

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

### 5. External ID UI Customization

Microsoft Entra External ID supports custom branding and page layouts to match the Mystira visual identity.

#### Option 1: Built-in Page Layouts

```
Azure Portal → Azure AD B2C → User flows → B2C_1_SignUpSignIn → Page layouts
├── Sign-up or sign-in page
│   ├── Template: Ocean Blue / Slate Gray / Classic
│   └── Custom page content: [Upload HTML]
├── Local account sign-up page
├── Social account sign-up page
└── Error page
```

#### Option 2: Company Branding

```
Azure Portal → Azure AD B2C → Company branding → Default branding
├── Sign-in page background image: [Upload 1920x1080 JPG/PNG]
├── Banner logo: [Upload 280x60 PNG]
├── Square logo: [Upload 240x240 PNG]
├── Username hint text: "Email address"
├── Sign-in page text: "Welcome to Mystira"
└── CSS: [Custom stylesheet]
```

#### Option 3: Custom HTML Templates

Create custom HTML templates for full control over the UI.

**Custom Sign-in Page Template** (`signin.html`):

```html
<!DOCTYPE html>
<html>
  <head>
    <title>Sign in to Mystira</title>
    <link
      href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap"
      rel="stylesheet"
    />
    <style>
      * {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
      }
      body {
        font-family: "Inter", sans-serif;
        background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
      }
      .container {
        background: white;
        border-radius: 16px;
        padding: 48px;
        box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
        max-width: 420px;
        width: 100%;
      }
      .logo {
        text-align: center;
        margin-bottom: 32px;
      }
      .logo img {
        height: 48px;
      }
      h1 {
        font-size: 24px;
        font-weight: 600;
        text-align: center;
        margin-bottom: 8px;
      }
      .subtitle {
        color: #6b7280;
        text-align: center;
        margin-bottom: 32px;
      }
      .social-buttons {
        display: flex;
        gap: 12px;
        margin-bottom: 24px;
      }
      .social-btn {
        flex: 1;
        padding: 12px;
        border: 1px solid #e5e7eb;
        border-radius: 8px;
        background: white;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        transition: all 0.2s;
      }
      .social-btn:hover {
        background: #f9fafb;
        border-color: #d1d5db;
      }
      .divider {
        display: flex;
        align-items: center;
        margin: 24px 0;
        color: #9ca3af;
      }
      .divider::before,
      .divider::after {
        content: "";
        flex: 1;
        height: 1px;
        background: #e5e7eb;
      }
      .divider span {
        padding: 0 16px;
        font-size: 14px;
      }

      /* B2C injects form here */
      #api {
        /* B2C form container */
      }
      #api input {
        width: 100%;
        padding: 12px 16px;
        border: 1px solid #e5e7eb;
        border-radius: 8px;
        font-size: 16px;
        margin-bottom: 16px;
      }
      #api input:focus {
        outline: none;
        border-color: #6366f1;
      }
      #api button {
        width: 100%;
        padding: 12px;
        background: #6366f1;
        color: white;
        border: none;
        border-radius: 8px;
        font-size: 16px;
        font-weight: 500;
        cursor: pointer;
        transition: background 0.2s;
      }
      #api button:hover {
        background: #4f46e5;
      }
    </style>
  </head>
  <body>
    <div class="container">
      <div class="logo">
        <img src="https://mystira.app/logo.svg" alt="Mystira" />
      </div>
      <h1>Welcome back</h1>
      <p class="subtitle">Sign in to continue your adventure</p>

      <div class="social-buttons">
        <button class="social-btn" id="GoogleExchange">
          <img src="https://www.google.com/favicon.ico" width="20" /> Google
        </button>
        <button class="social-btn" id="DiscordExchange">
          <img src="https://discord.com/assets/favicon.ico" width="20" />
          Discord
        </button>
      </div>

      <div class="divider"><span>or continue with email</span></div>

      <!-- B2C injects the form here -->
      <div id="api"></div>
    </div>
  </body>
</html>
```

**Host Custom Pages**:

1. Upload HTML to Azure Blob Storage
2. Enable CORS for `*.b2clogin.com`
3. Configure in User Flow:

```
Azure Portal → User flows → Page layouts → Custom page URI
├── Sign-up or sign-in: https://mystiracdn.blob.core.windows.net/b2c/signin.html
├── Sign-up: https://mystiracdn.blob.core.windows.net/b2c/signup.html
├── Password reset: https://mystiracdn.blob.core.windows.net/b2c/reset.html
└── Error: https://mystiracdn.blob.core.windows.net/b2c/error.html
```

#### Custom CSS Variables

```css
/* B2C Custom CSS - upload via Company Branding */
:root {
  --mystira-primary: #6366f1;
  --mystira-primary-hover: #4f46e5;
  --mystira-bg: #1a1a2e;
  --mystira-text: #1f2937;
  --mystira-text-muted: #6b7280;
  --mystira-border: #e5e7eb;
  --mystira-radius: 8px;
}

/* Override B2C defaults */
.entry-item {
  margin-bottom: 16px;
}
.entry-item input {
  border-radius: var(--mystira-radius);
  border-color: var(--mystira-border);
  padding: 12px 16px;
}
.buttons button {
  background: var(--mystira-primary);
  border-radius: var(--mystira-radius);
}
.buttons button:hover {
  background: var(--mystira-primary-hover);
}
.divider {
  margin: 24px 0;
}
.social button {
  border-radius: var(--mystira-radius);
}
```

#### JavaScript Customization

```javascript
// B2C Custom JavaScript
document.addEventListener("DOMContentLoaded", function () {
  // Add loading states
  const buttons = document.querySelectorAll('button[type="submit"]');
  buttons.forEach((btn) => {
    btn.addEventListener("click", function () {
      this.classList.add("loading");
      this.disabled = true;
    });
  });

  // Password strength indicator
  const passwordInput = document.getElementById("newPassword");
  if (passwordInput) {
    passwordInput.addEventListener("input", function () {
      const strength = calculatePasswordStrength(this.value);
      updateStrengthIndicator(strength);
    });
  }

  // Auto-focus first input
  const firstInput = document.querySelector('input:not([type="hidden"])');
  if (firstInput) firstInput.focus();
});

function calculatePasswordStrength(password) {
  let strength = 0;
  if (password.length >= 8) strength++;
  if (password.length >= 12) strength++;
  if (/[A-Z]/.test(password)) strength++;
  if (/[a-z]/.test(password)) strength++;
  if (/[0-9]/.test(password)) strength++;
  if (/[^A-Za-z0-9]/.test(password)) strength++;
  return Math.min(strength, 4);
}
```

## Implementation Status

### ✅ Completed

- **Centralized Identity API** - Implemented in `packages/identity/src/Mystira.Identity.Api`
- **UnifiedAuthService** - Client-side dual-path authentication (Magic + Entra)
- **Magic Link Authentication** - Primary consumer flow with 30min TTL, 7-day pending signup validity
- **Entra ID Integration** - Workforce authentication with provisioning service
- **JWT Token Service** - Cross-service authentication with refresh tokens
- **Background Provisioning** - Async user provisioning via `ProvisioningBackgroundWorker`

### 🔄 In Progress

- **Admin UI Entra Integration** - MSAL configuration for workforce authentication
- **Conditional Access Policies** - MFA requirements for admin users
- **Service-to-Service Auth** - Managed identity configuration for Azure services

### 📋 Pending

- **Social Provider Integration** - Google/Discord via Entra External ID (optional)
- **Advanced Role Mapping** - Group-to-role mapping for enterprise scenarios

## Consequences

### Positive

1. **Unified Authentication**: Single authority across all services and applications
2. **Consumer-First**: Magic link authentication reduces friction for primary users
3. **Enterprise Ready**: Entra ID integration supports workforce scenarios
4. **Flexible**: Optional Entra social login while maintaining email-first approach
5. **Scalable**: Background provisioning handles user creation asynchronously
6. **Secure**: JWT tokens with proper expiration and refresh mechanisms

### Negative

1. **Complexity**: Dual-path authentication increases implementation complexity
2. **Dependency**: Centralized Identity API is a critical dependency for all services
3. **Migration**: Existing authentication methods need to be migrated
4. **Provisioning Lag**: Background provisioning may cause temporary user state inconsistencies

### Mitigations

1. **Fallback Mechanisms**: Graceful degradation when Identity API is unavailable
2. **Health Checks**: Service health monitoring for authentication endpoints
3. **Documentation**: Comprehensive setup and troubleshooting guides
4. **Testing**: End-to-end testing for all authentication flows

## Technical Implementation

### Core Components

```csharp
// Identity API Controller
[ApiController]
[Route("api/auth")]
public class IdentityAuthController : ControllerBase
{
    [HttpPost("magic/request")]
    public async Task<IActionResult> RequestMagicToken(MagicTokenRequest request);

    [HttpPost("magic/verify")]
    public async Task<IActionResult> VerifyMagicToken(MagicTokenVerification request);

    [HttpPost("entra/callback")]
    public async Task<IActionResult> EntraCallback(EntraCallbackRequest request);
}

// Unified Client Service
public class UnifiedAuthService : IAuthService
{
    public async Task<bool> SignInWithMagicAsync(string email);
    public async Task<bool> SignInWithEntraAsync();
    public async Task<bool> EnsureTokenValidAsync();
    public async Task SignOutAsync();
}
```

### Configuration

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "${ENTRA_TENANT_ID}",
    "ClientId": "${ENTRA_CLIENT_ID}",
    "ClientSecret": "${ENTRA_CLIENT_SECRET}"
  },
  "MagicAuth": {
    "TokenExpirationMinutes": 30,
    "PendingSignupExpirationDays": 7
  }
}
```

## Related Documentation

- **[BACKLOG.md](../../../BACKLOG.md)** - Current implementation status and open work
- **[Identity API README](../../../packages/identity/README.md)** - Service-specific documentation
- **[ENTRA_MAGIC_AUTH_SETUP.md](../../../docs/ENTRA_MAGIC_AUTH_SETUP.md)** - Setup guide
- **[ADR-0010: Auth Strategy](./0010-authentication-and-authorization-strategy.md)** - Overall authentication strategy

  aad_auth_enabled = true
  aad_admin_identities = {
  "admin-api" = {
  principal_id = module.admin_api.identity_principal_id
  principal_name = "mys-dev-admin-api-identity-san"
  principal_type = "ServicePrincipal"
  }
  }
  }

```

**Connection String** (no password):

```

Host=<server>.postgres.database.azure.com;Database=adminapi;Username=mys-dev-admin-api-identity-san;Ssl Mode=Require

```

See the [PostgreSQL Module README](../../../infra/terraform/modules/shared/postgresql/README.md) for detailed configuration.

## References

- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [MSAL.js for React](https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-react)
- [Microsoft.Identity.Web](https://docs.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/en-us/entra/external-id/)
- [External ID Custom Policies](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-custom-extensions)
- [External ID Identity Providers](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-google-federation-customers)
- [Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
- [Conditional Access](https://docs.microsoft.com/en-us/azure/active-directory/conditional-access/)
```
