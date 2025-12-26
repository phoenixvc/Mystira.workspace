# Migration 002: App API Migration Guide

## Overview

This document provides guidance for migrating to the new unified App API endpoints and domain structure.

## Domain Changes

### Before (Legacy)

```
# Various inconsistent domains were used
https://mystira.io/api/v1
https://api.mystira.io/v1
https://app.mystira.app/api
```

### After (Unified)

| Environment | Domain | Description |
|-------------|--------|-------------|
| Production | `https://api.mystira.app/v1` | Main production API |
| Staging | `https://staging.api.mystira.app/v1` | Staging environment |
| Development | `https://dev.api.mystira.app/v1` | Development environment |
| Local | `http://localhost:5000/v1` | Local development |

## API Contract Changes

### Error Responses

All error responses now use the unified `ErrorResponse` schema from `@mystira/contracts`:

```typescript
import { ErrorResponse, ValidationErrorResponse } from '@mystira/contracts';

// TypeScript types are now generated from OpenAPI
interface ErrorResponse {
  code: string;
  message: string;
  details?: string;
  requestId?: string;
  timestamp: string;
  metadata?: Record<string, unknown>;
}
```

### C# Integration

```csharp
// Use the generated contracts from OpenAPI
using Mystira.Contracts.Common;

// ErrorResponse is now defined in packages/api-spec/openapi/common/errors.yaml
// and generated to Mystira.Contracts.Generated namespace
```

### Health Endpoints

The health check endpoints are standardized:

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Full health check with dependencies |
| `GET /health/ready` | Kubernetes readiness probe |
| `GET /health/live` | Kubernetes liveness probe |

## SDK Updates

### TypeScript/JavaScript

```typescript
// Update your API client configuration
import { createClient } from '@mystira/contracts';

const client = createClient({
  baseUrl: process.env.NODE_ENV === 'production'
    ? 'https://api.mystira.app/v1'
    : 'https://dev.api.mystira.app/v1',
});
```

### C# (.NET)

```csharp
// Update your HttpClient configuration
services.AddHttpClient<IMystiraApiClient>(client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("MYSTIRA_API_URL")
        ?? "https://api.mystira.app/v1";
    client.BaseAddress = new Uri(baseUrl);
});
```

## Environment Variable Updates

Update your environment configuration:

```bash
# .env.development
MYSTIRA_API_URL=https://dev.api.mystira.app/v1

# .env.staging
MYSTIRA_API_URL=https://staging.api.mystira.app/v1

# .env.production
MYSTIRA_API_URL=https://api.mystira.app/v1
```

## Terraform/Infrastructure Changes

If you manage your own infrastructure, update the custom domain configuration:

```hcl
# infra/terraform/environments/dev/mystira-app.tf
module "mystira_app" {
  source = "../../modules/mystira-app"

  api_custom_domain = "dev.api.mystira.app"  # Updated pattern
  # ...
}
```

## Breaking Changes

1. **Domain Change**: All API calls must use the new `{env}.api.mystira.app` pattern
2. **Error Response Schema**: Error responses now include `requestId` and `timestamp` fields
3. **Health Check Response**: Health check returns structured `HealthCheckResult` with dependency status

## Migration Checklist

- [ ] Update API base URLs in all clients
- [ ] Update environment variables
- [ ] Update CORS configuration to allow new domains
- [ ] Update API documentation references
- [ ] Test health endpoints work with new format
- [ ] Update monitoring/alerting URLs
- [ ] Update CI/CD deployment configurations

## Rollback Plan

If issues arise, the legacy domains will be maintained for a transition period:

1. Legacy endpoints will redirect to new domains (HTTP 301)
2. Set `X-Mystira-Legacy-Domain: true` header to bypass redirect temporarily
3. Contact eben@phoenixvc.tech for extended legacy support

## Timeline

| Phase | Date | Description |
|-------|------|-------------|
| Announcement | 2026-Q1 | Migration guide published |
| New Domains Active | 2026-Q1 | New domains available alongside legacy |
| Legacy Redirect | 2026-Q2 | Legacy domains redirect to new |
| Legacy Sunset | 2026-Q3 | Legacy domains removed |

## Support

For migration assistance:
- Technical: jurie@phoenixvc.tech
- Business/Admin: eben@phoenixvc.tech
- Issues: https://github.com/phoenixvc/Mystira.workspace/issues
