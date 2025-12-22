# ADR-0010: Authentication and Authorization Strategy

## Status

**Accepted** - 2025-12-19

## Context

The Mystira platform has grown to include multiple services with different authentication needs:

1. **Public API** (`Mystira.App.Api`) - User-facing REST API for game sessions, scenarios, etc.
2. **Admin API** (`Mystira.Admin.Api`) - Internal administration REST/gRPC API
3. **Admin UI** (`Mystira.Admin.UI`) - React-based administration dashboard
4. **PWA** - Blazor WebAssembly client application
5. **Publisher** - TypeScript/React frontend for blockchain operations
6. **Chain Service** - Python gRPC service for Web3 operations
7. **StoryGenerator** - .NET service for AI story generation

### Problem Statement

Authentication has been implemented incrementally across services without a unified strategy document. The current state includes:

- JWT tokens mentioned for Public API (ADR-0005)
- Cookie-based authentication migrated for Admin UI (MIGRATION_PHASES.md)
- Shared JWT services in `Mystira.App.Shared` NuGet package
- OAuth 2.0 / OpenID Connect planned but not implemented

Key questions that need answering:

1. **When to use JWT vs. Cookies?** - No clear guidance exists
2. **How does session management work?** - Token lifecycle undocumented
3. **Service-to-service auth?** - Only "future: mTLS" mentioned
4. **Admin API authentication?** - Mechanism unclear
5. **OAuth/OIDC implementation?** - Planned but undefined

## Decision

We adopt a **layered security and authentication strategy** with clear boundaries:

### Security Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Unified Security Strategy                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Layer 1: Edge Security (Azure Front Door)                                  │
│  ├── DDoS Protection, WAF (OWASP), Rate Limiting, Bot Protection            │
│  └── See: FRONT_DOOR_IMPLEMENTATION_SUMMARY.md                              │
│                                                                              │
│  Layer 2: Identity Providers (Microsoft Entra ID / External ID)             │
│  ├── Admin: Entra ID (MFA, Conditional Access, App Roles)                   │
│  ├── Consumer: External ID (Social login: Google, Discord)                  │
│  └── See: ADR-0011                                                          │
│                                                                              │
│  Layer 3: Application Authentication (This ADR)                             │
│  ├── JWT tokens for APIs, Cookies for browser apps                          │
│  └── Session management, token refresh, RBAC                                │
│                                                                              │
│  Layer 4: Service-to-Service (Managed Identity)                             │
│  └── Passwordless Azure resource access                                     │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1. Authentication Methods by Service Type

#### Browser-Based Applications (Cookie-Based)

**Applies to**: Admin UI, PWA

**Method**: HTTP-only secure cookies with session tokens

**Rationale**:
- Protection against XSS attacks (cookies not accessible via JavaScript)
- Automatic inclusion in requests (no client-side token management)
- Better fit for same-origin web applications
- Simpler logout/session invalidation

**Implementation**:

```csharp
// ASP.NET Core cookie authentication
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
    });
```

**Cookie Configuration**:

| Attribute   | Value            | Reason                                      |
| ----------- | ---------------- | ------------------------------------------- |
| HttpOnly    | `true`           | Prevents XSS-based token theft              |
| Secure      | `true`           | HTTPS only in production                    |
| SameSite    | `Strict`         | CSRF protection                             |
| Domain      | Service-specific | Isolates cookies per service                |
| Path        | `/`              | Available to all routes                     |
| Expiry      | 8 hours          | Balance between security and UX             |
| Sliding     | `true`           | Extends session on activity                 |

#### API Clients (JWT-Based)

**Applies to**: Public API, Mobile clients, Third-party integrations

**Method**: JWT Bearer tokens in Authorization header

**Rationale**:
- Stateless authentication (scalable across servers)
- Self-contained claims (no database lookup required)
- Standard format for API authentication
- Works across different domains/origins

**Implementation**:

```csharp
// ASP.NET Core JWT authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });
```

**Token Structure**:

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id-guid",
    "email": "user@example.com",
    "name": "User Name",
    "roles": ["Player", "Creator"],
    "iat": 1702987200,
    "exp": 1703073600,
    "iss": "mystira.app",
    "aud": "mystira-public-api"
  }
}
```

**Token Lifetimes**:

| Token Type    | Lifetime     | Storage                     | Use Case                    |
| ------------- | ------------ | --------------------------- | --------------------------- |
| Access Token  | 15 minutes   | Memory (not localStorage)   | API authentication          |
| Refresh Token | 7 days       | HTTP-only cookie            | Obtain new access tokens    |
| ID Token      | 1 hour       | Memory                      | User info display           |

#### Service-to-Service (API Keys & Managed Identities)

**Applies to**: Chain → Publisher, StoryGenerator → Public API, internal services

**Method**: API keys for external services, Azure Managed Identity for Azure services

**API Key Pattern**:

```bash
# Environment variable
CHAIN_API_KEY=myst_live_xxxxxxxxxxxx

# Header
X-API-Key: myst_live_xxxxxxxxxxxx
```

**Managed Identity Pattern** (Azure services):

```csharp
// Azure SDK with Managed Identity
var credential = new DefaultAzureCredential();
var cosmosClient = new CosmosClient(endpoint, credential);
```

### 2. Authentication Flows

#### Public API Login Flow (JWT)

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐
│  Client │────▶│  Public API  │────▶│   Cosmos DB │
│  (PWA)  │◀────│   /auth      │◀────│   (Users)   │
└─────────┘     └──────────────┘     └─────────────┘
     │                 │
     │  1. POST /auth/login
     │     { email, password }
     │                 │
     │  2. Validate credentials
     │                 │
     │  3. Generate tokens
     │     - Access Token (JWT)
     │     - Refresh Token (cookie)
     │                 │
     │◀─────────────────
     │  Response:
     │  { accessToken, expiresIn }
     │  Set-Cookie: refreshToken=...
```

#### Admin UI Login Flow (Cookie)

```
┌─────────┐     ┌──────────────┐     ┌─────────────┐
│Admin UI │────▶│  Admin API   │────▶│   Cosmos DB │
│ (React) │◀────│   /auth      │◀────│   (Admins)  │
└─────────┘     └──────────────┘     └─────────────┘
     │                 │
     │  1. POST /auth/login
     │     { username, password }
     │                 │
     │  2. Validate credentials
     │                 │
     │  3. Create session
     │                 │
     │◀─────────────────
     │  Set-Cookie: .AspNetCore.Auth=...
     │  { user: { id, name, roles } }
```

#### Token Refresh Flow

```
┌─────────┐     ┌──────────────┐
│  Client │────▶│  Public API  │
│         │◀────│   /auth      │
└─────────┘     └──────────────┘
     │                 │
     │  1. POST /auth/refresh
     │     Cookie: refreshToken=...
     │                 │
     │  2. Validate refresh token
     │                 │
     │  3. Generate new access token
     │     (optionally rotate refresh)
     │                 │
     │◀─────────────────
     │  { accessToken, expiresIn }
```

### 3. Authorization Model

#### Role-Based Access Control (RBAC)

**User Roles** (Public API):

| Role      | Description                          | Permissions                              |
| --------- | ------------------------------------ | ---------------------------------------- |
| Player    | Standard user                        | Read scenarios, manage own sessions      |
| Creator   | Content creator                      | Create/edit own scenarios                |
| Moderator | Content moderation                   | Review/approve/reject content            |
| Admin     | Full access                          | All operations                           |

**Admin Roles** (Admin API):

| Role         | Description                       | Permissions                              |
| ------------ | --------------------------------- | ---------------------------------------- |
| Viewer       | Read-only access                  | View dashboards, reports                 |
| Operator     | Day-to-day operations             | Manage content, users                    |
| Admin        | Full admin access                 | All admin operations                     |
| SuperAdmin   | System-level access               | Infrastructure, secrets, dangerous ops   |

**Claims-Based Authorization**:

```csharp
[Authorize(Roles = "Creator,Admin")]
public async Task<IActionResult> CreateScenario()

[Authorize(Policy = "CanModerateContent")]
public async Task<IActionResult> ApproveScenario()
```

### 4. Session Management

#### Session Lifecycle

```
┌────────────────────────────────────────────────────────────────┐
│                      Session Lifecycle                          │
├────────────────────────────────────────────────────────────────┤
│  Login ──▶ Active ──▶ Idle ──▶ Expired ──▶ Logout              │
│                │                                                 │
│                └──▶ Refresh ──▶ Active                          │
└────────────────────────────────────────────────────────────────┘
```

**Session States**:

| State    | Duration                | Action Required                          |
| -------- | ----------------------- | ---------------------------------------- |
| Active   | While using app         | None                                     |
| Idle     | 15 min inactivity       | Sliding refresh extends session          |
| Expired  | After max lifetime      | Re-authenticate required                 |
| Revoked  | Manual logout/revoke    | Re-authenticate required                 |

#### Session Storage

**Cookie Sessions** (Admin UI):
- Session stored server-side (distributed cache/Redis)
- Cookie contains session ID only
- Server can revoke sessions immediately

**JWT Sessions** (Public API):
- Token is self-contained (stateless)
- Refresh tokens tracked in database for revocation
- Access tokens valid until expiry (use short lifetime)

**Revocation Strategy**:

```csharp
// For refresh tokens - database lookup
public class RefreshToken
{
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedReason { get; set; }
}

// For access tokens - short lifetime + refresh token revocation
// No individual access token revocation (too expensive)
```

### 5. Security Considerations

#### Edge Security (Azure Front Door)

Azure Front Door provides the first line of defense before requests reach authentication endpoints:

```
User Request
     ↓
┌─────────────────────────────────────────┐
│        Azure Front Door (Edge)          │
├─────────────────────────────────────────┤
│  ✓ DDoS Protection (L3/L4/L7)          │
│  ✓ WAF (OWASP 3.2 + Custom Rules)      │
│  ✓ Edge Rate Limiting (per IP)          │
│  ✓ Bot Protection                       │
│  ✓ SSL/TLS Termination (1.2+)          │
│  ✓ Geo-blocking (if needed)            │
└─────────────────────────────────────────┘
     ↓
Application Layer (Auth Endpoints)
```

**Front Door WAF Rules for Auth Protection**:

| Rule | Type | Action | Purpose |
|------|------|--------|---------|
| OWASP 3.2 | Managed | Block | SQL injection, XSS, etc. |
| Bot Manager | Managed | Block | Bad bots, scanners |
| Rate Limit | Custom | Block | 500 req/min per IP (prod) |
| Method Filter | Custom | Block | Allow only GET, POST, PUT, DELETE |

**Defense in Depth**: Front Door rate limiting + application-level rate limiting provide layered protection.

For Front Door implementation details, see:
- [Front Door Implementation Summary](../../../FRONT_DOOR_IMPLEMENTATION_SUMMARY.md)
- [Front Door Deployment Guide](../../../infra/FRONT_DOOR_DEPLOYMENT_GUIDE.md)

#### Password Requirements

```csharp
options.Password = new PasswordOptions
{
    RequiredLength = 12,
    RequireDigit = true,
    RequireLowercase = true,
    RequireUppercase = true,
    RequireNonAlphanumeric = true,
    RequiredUniqueChars = 4
};
```

#### Rate Limiting (Application Layer)

**Note**: Front Door provides edge-level rate limiting (500 req/min per IP). The limits below are application-level rate limits for additional protection on auth endpoints specifically.

| Endpoint           | Limit                 | Window    | Action on Exceed        |
| ------------------ | --------------------- | --------- | ----------------------- |
| `/auth/login`      | 5 attempts            | 15 min    | Temporary lockout       |
| `/auth/register`   | 3 attempts            | 1 hour    | CAPTCHA required        |
| `/auth/refresh`    | 10 attempts           | 1 hour    | Force re-login          |
| `/auth/forgot`     | 3 attempts            | 1 hour    | Delay response          |

#### Security Headers

**Note**: Front Door handles SSL/TLS and can inject security headers at the edge. The application should also set these headers for defense in depth.

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'
```

### 6. Shared Authentication Services

**`Mystira.App.Shared` Package** provides:

```
Mystira.App.Shared/
├── Authentication/
│   ├── JwtService.cs           # Token generation/validation
│   ├── JwtOptions.cs           # Configuration options
│   └── JwtMiddleware.cs        # ASP.NET Core middleware
├── Authorization/
│   ├── RoleRequirements.cs     # Role-based policies
│   ├── PermissionHandler.cs    # Custom authorization handlers
│   └── ClaimsTransformer.cs    # Claims enrichment
└── Extensions/
    └── AuthServiceExtensions.cs # DI registration helpers
```

**Usage**:

```csharp
// Program.cs
services.AddMystiraJwtAuthentication(configuration);
services.AddMystiraAuthorization();

// In controllers
[Authorize]
public class ScenariosController : ControllerBase
```

### 7. Secrets Management

**JWT Keys**:
- Stored in Azure Key Vault
- Rotated every 90 days
- Retrieved via Managed Identity

**Configuration**:

```yaml
# appsettings.Production.json
{
  "Jwt": {
    "KeyVaultSecretName": "jwt-signing-key",
    "Issuer": "mystira.app",
    "Audience": "mystira-public-api"
  }
}
```

**Key Rotation**:

1. Generate new key in Key Vault
2. Deploy services with dual-key validation (old + new)
3. Wait for old tokens to expire (max 15 min)
4. Remove old key validation
5. Delete old key from Key Vault

## Rationale

### Cookie vs. JWT Decision Matrix

| Factor              | Cookies                    | JWT                        | Decision                   |
| ------------------- | -------------------------- | -------------------------- | -------------------------- |
| XSS Protection      | HttpOnly (excellent)       | Vulnerable if in storage   | Cookies for browsers       |
| CSRF Protection     | Needs SameSite/tokens      | Not vulnerable             | JWT for APIs               |
| Cross-Origin        | Complex (CORS)             | Simple (header)            | JWT for cross-origin       |
| Session Revocation  | Immediate                  | Wait for expiry            | Cookies for admin          |
| Scalability         | Needs shared session store | Stateless                  | JWT for public API         |
| Mobile/Third-party  | Not supported              | Works everywhere           | JWT for external clients   |

### OAuth/OIDC and External Identity Providers

OAuth 2.0 / OpenID Connect is now implemented via Microsoft Entra ID and External ID:

| Provider | Use Case | Features |
|----------|----------|----------|
| **Microsoft Entra ID** | Admin users, Enterprise SSO | MFA, Conditional Access, App Roles |
| **Microsoft Entra External ID** | Consumer users (Public API, PWA) | Social login (Google, Discord), Self-service registration |
| **Managed Identity** | Service-to-service | Passwordless Azure resource access |

**For complete implementation details**, see [ADR-0011: Microsoft Entra ID Authentication Integration](./0011-entra-id-authentication-integration.md).

#### Social Login Support (via External ID)

Microsoft Entra External ID enables social identity providers for consumer applications:

- **Google**: OAuth 2.0 integration for Google accounts
- **Discord**: OpenID Connect custom provider for gamers
- **Email/Password**: Local accounts with self-service registration

**External ID User Flows**:
- `B2C_1_SignUpSignIn`: Combined sign-up and sign-in
- `B2C_1_PasswordReset`: Self-service password reset
- `B2C_1_ProfileEdit`: User profile management

## Consequences

### Positive

1. **Clear Guidance**: Developers know when to use cookies vs. JWT
2. **Security**: Different auth methods optimized for each context
3. **Scalability**: Stateless JWT for high-volume public API
4. **Control**: Cookie-based admin sessions can be instantly revoked
5. **Shared Code**: Common auth services reduce duplication

### Negative

1. **Dual Implementation**: Must maintain both cookie and JWT auth
2. **Complexity**: More code paths to test and secure
3. **Learning Curve**: Team must understand both patterns
4. **Session Store**: Cookie auth requires Redis/distributed cache

### Mitigations

1. **Shared Library**: `Mystira.App.Shared` provides tested implementations
2. **Documentation**: This ADR and code comments explain patterns
3. **Testing**: Automated security tests for both auth flows
4. **Infrastructure**: Redis already deployed for caching

## Authentication Matrix

| Service/Client         | Auth Method         | Token Type      | Storage               | Revocation          |
| ---------------------- | ------------------- | --------------- | --------------------- | ------------------- |
| Admin UI               | Cookie              | Session ID      | HTTP-only cookie      | Immediate           |
| PWA (Blazor)           | Cookie + JWT        | Both            | Cookie + Memory       | Hybrid              |
| Public API (Browser)   | JWT                 | Access + Refresh| Memory + Cookie       | Refresh revocation  |
| Public API (Mobile)    | JWT                 | Access + Refresh| Secure storage        | Refresh revocation  |
| Publisher              | API Key             | API Key         | Environment var       | Key rotation        |
| Chain Service          | API Key             | API Key         | Environment var       | Key rotation        |
| Service-to-Azure       | Managed Identity    | AAD Token       | Runtime               | AAD revocation      |

## Implementation Checklist

### Core Authentication
- [x] JWT services in `Mystira.App.Shared`
- [x] Cookie-based auth for Admin UI
- [ ] Refresh token implementation for Public API
- [ ] Rate limiting on auth endpoints
- [ ] Session management dashboard (Admin)
- [ ] Audit logging for auth events

### Microsoft Entra ID Integration (see [ADR-0011](./0011-entra-id-authentication-integration.md))
- [ ] Entra ID App Registration for Admin API
- [ ] Entra ID App Registration for Admin UI
- [ ] MSAL configuration in Admin UI (React)
- [ ] Microsoft.Identity.Web in Admin API
- [ ] MFA via Conditional Access policies
- [ ] App Roles and group mapping

### Microsoft Entra External ID Integration (see [ADR-0011](./0011-entra-id-authentication-integration.md))
- [ ] External ID tenant creation
- [ ] User flow configuration (SignUpSignIn, PasswordReset, ProfileEdit)
- [ ] Google identity provider setup
- [ ] Discord identity provider setup (OpenID Connect)
- [ ] External ID App Registration for Public API
- [ ] External ID authentication in PWA (Blazor WASM)
- [ ] Custom External ID UI branding

### Service-to-Service Authentication
- [ ] Managed Identity on App Services/AKS
- [ ] Managed Identity access to Cosmos DB
- [ ] Managed Identity access to Key Vault

### Edge Security (Azure Front Door)
- [ ] Deploy Front Door in dev environment
- [ ] Configure WAF with OWASP 3.2 managed rules
- [ ] Configure Bot Manager rules
- [ ] Set up rate limiting (500 req/min prod, 100 req/min dev)
- [ ] Configure custom domain and SSL certificates
- [ ] Test WAF in Detection mode before switching to Prevention
- [ ] Deploy to production with Prevention mode

## Related ADRs

- [ADR-0011: Microsoft Entra ID Authentication Integration](./0011-entra-id-authentication-integration.md) - **Entra ID, B2C, and social login implementation details**
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md) - Network-level security
- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md) - Admin service architecture
- [ADR-0007: NuGet Feed Strategy](./0007-nuget-feed-strategy-for-shared-libraries.md) - Shared auth library distribution

## Related Documentation

- [Front Door Implementation Summary](../../../FRONT_DOOR_IMPLEMENTATION_SUMMARY.md) - **Edge security, WAF, DDoS protection**
- [Front Door Deployment Guide](../../../infra/FRONT_DOOR_DEPLOYMENT_GUIDE.md) - Deployment instructions
- [Azure Front Door Plan](../../../infra/AZURE_FRONT_DOOR_PLAN.md) - Architecture and cost analysis
- [Kubernetes Secrets Management](../infrastructure/kubernetes-secrets-management.md) - Auth secrets in K8s

## References

- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [OWASP Session Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html)
- [JWT Best Practices (RFC 8725)](https://datatracker.ietf.org/doc/html/rfc8725)
- [OAuth 2.0 Security Best Current Practice](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics)
- [ASP.NET Core Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Azure Front Door Documentation](https://docs.microsoft.com/en-us/azure/frontdoor/)
- [Azure Front Door WAF](https://docs.microsoft.com/en-us/azure/web-application-firewall/afds/afds-overview)
