# Authentication & Authorization Standardization

## Summary

Standardized authentication and authorization across all Mystira API projects using shared extensions in `Mystira.Shared`.

## Changes

### 1. Shared Authentication Extensions (`Mystira.Shared`)

**New files:**

- `packages/shared/Mystira.Shared/Configuration/AuthenticationExtensions.cs`
- `packages/shared/Mystira.Shared/Configuration/AuthorizationExtensions.cs`

**Features:**

- `AddMystiraAuthentication()` - JWT authentication with configurable options
- `AddMystiraEntraIdAuthentication()` - Microsoft Entra ID (Azure AD) integration
- `AddMystiraAuthorizationPolicies()` - Standard authorization policies

**Options:**

- `SkipAuthenticationPaths` - Paths that bypass JWT validation (defaults include `/api/auth/*`)
- `EnableSecurityMetrics` - Track auth events in security metrics

**Security:**

- Log sanitization (`SanitizeForLog()`) - Redacts PII (emails, IPs) from logs
- Hash IDs for privacy-safe logging (`HashId()`)
- JWKS endpoint support for key rotation
- RSA asymmetric key support (recommended)
- Symmetric key fallback (legacy)

### 2. Authorization Policies

Standard policies available to all APIs:

| Policy           | Roles                                |
| ---------------- | ------------------------------------ |
| `AdminOnly`      | Admin, SuperAdmin                    |
| `CanModerate`    | Moderator, Admin, SuperAdmin         |
| `ReadOnly`       | Viewer, Moderator, Admin, SuperAdmin |
| `SuperAdminOnly` | SuperAdmin                           |

### 3. API Projects Updated

| Project                | Changes                                          |
| ---------------------- | ------------------------------------------------ |
| **App.Api**            | Uses shared auth extensions                      |
| **Admin.Api**          | Migrated from inline auth (~145 lines) to shared |
| **StoryGenerator.Api** | Uses shared auth extensions                      |
| **Publisher.Api**      | New API with shared auth (see below)             |

### 4. New Publisher.Api

Created new API at `apps/publisher/api/src/Mystira.Publisher.Api/`

**Structure:**

- `Program.cs` - Entry point with shared authentication
- `Controllers/StoriesController.cs` - Story CRUD
- `Controllers/ContributorsController.cs` - Attribution, approvals, royalty splits
- `Controllers/UsersController.cs` - User search

**Endpoints:**

- `GET /api/stories` - List stories
- `GET /api/stories/{id}` - Get story
- `POST /api/stories` - Create story
- `PATCH /api/stories/{id}` - Update story
- `DELETE /api/stories/{id}` - Delete story
- `GET /api/contributors/story/{storyId}` - Get contributors
- `POST /api/contributors` - Add contributor
- `PATCH /api/contributors/{id}` - Update contributor
- `DELETE /api/contributors/{id}` - Remove contributor
- `POST /api/contributors/approve` - Submit approval
- `POST /api/contributors/override` - Override contributor
- `GET /api/contributors/validate/{storyId}` - Validate royalty splits
- `GET /api/users/search` - Search users

## Usage

```csharp
// In Program.cs
builder.Services.AddMystiraAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddMystiraEntraIdAuthentication(builder.Configuration);
builder.Services.AddMystiraAuthorizationPolicies();

// Use on controllers
[Authorize(Policy = "AdminOnly")]
```

## Configuration

```json
{
  "JwtSettings": {
    "Issuer": "mystira-identity-api",
    "Audience": "mystira-platform",
    "JwksEndpoint": "https://auth.mystira.app/.well-known/jwks.json"
  },
  "AzureAd": {
    "TenantId": "...",
    "ClientId": "..."
  }
}
```

## Environment Variables

| Variable         | Purpose                      |
| ---------------- | ---------------------------- |
| `DEV_JWT_SECRET` | Fallback key for development |
