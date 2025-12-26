# Migration 004: Unified API Domains

## Overview

This document outlines the standardized domain naming convention for all Mystira platform services.

## Domain Naming Convention

All Mystira services follow the pattern:

```
{environment}.{service}.mystira.app
```

Where:
- **environment**: `dev`, `staging`, or omitted for production
- **service**: The service identifier (e.g., `api`, `story-api`, `publisher`)
- **domain**: Always `mystira.app`

## Complete Domain Reference

### Production (no environment prefix)

| Service | Domain | Description |
|---------|--------|-------------|
| Main App | `mystira.app` | Main web application |
| App API | `api.mystira.app` | Main application API |
| Admin UI | `admin.mystira.app` | Admin dashboard |
| Admin API | `admin-api.mystira.app` | Admin API |
| Publisher | `publisher.mystira.app` | Content publishing service |
| Chain | `chain.mystira.app` | Blockchain service |
| Story API | `story-api.mystira.app` | Story generator API |
| Story Web | `story.mystira.app` | Story generator SWA |

### Staging

| Service | Domain |
|---------|--------|
| Main App | `staging.mystira.app` |
| App API | `staging.api.mystira.app` |
| Admin UI | `staging.admin.mystira.app` |
| Admin API | `staging.admin-api.mystira.app` |
| Publisher | `staging.publisher.mystira.app` |
| Chain | `staging.chain.mystira.app` |
| Story API | `staging.story-api.mystira.app` |
| Story Web | `staging.story.mystira.app` |

### Development

| Service | Domain |
|---------|--------|
| Main App | `dev.mystira.app` |
| App API | `dev.api.mystira.app` |
| Admin UI | `dev.admin.mystira.app` |
| Admin API | `dev.admin-api.mystira.app` |
| Publisher | `dev.publisher.mystira.app` |
| Chain | `dev.chain.mystira.app` |
| Story API | `dev.story-api.mystira.app` |
| Story Web | `dev.story.mystira.app` |

## SSL/TLS Certificates

All domains use Let's Encrypt certificates:

| Environment | Certificate Type | Notes |
|-------------|-----------------|-------|
| Production | Let's Encrypt Production | Fully trusted |
| Staging | Let's Encrypt Staging | May show warnings |
| Development | Let's Encrypt Staging | May show warnings |

## DNS Configuration

All DNS records are managed in Azure DNS Zone `mystira.app`.

### Wildcard Pattern

```
*.dev.mystira.app     → Azure Front Door (dev)
*.staging.mystira.app → Azure Front Door (staging)
*.mystira.app         → Azure Front Door (prod)
```

### Terraform Configuration

```hcl
# infra/terraform/environments/{env}/front-door.tf

module "front_door" {
  source = "../../modules/front-door"

  environment = var.environment  # "dev", "staging", "prod"

  # Service domains
  custom_domain_publisher = "${local.env_prefix}publisher.mystira.app"
  custom_domain_chain     = "${local.env_prefix}chain.mystira.app"
  custom_domain_story_api = "${local.env_prefix}story-api.mystira.app"
}

locals {
  # Empty for production, "dev." or "staging." for others
  env_prefix = var.environment == "prod" ? "" : "${var.environment}."
}
```

## CORS Configuration

Update CORS to include all environment domains:

```typescript
// config/cors.ts
const allowedOrigins = [
  // Production
  'https://mystira.app',
  'https://admin.mystira.app',
  'https://story.mystira.app',

  // Staging
  'https://staging.mystira.app',
  'https://staging.admin.mystira.app',
  'https://staging.story.mystira.app',

  // Development
  'https://dev.mystira.app',
  'https://dev.admin.mystira.app',
  'https://dev.story.mystira.app',

  // Local
  'http://localhost:3000',
  'http://localhost:5173',
];
```

## API Version Strategy

All APIs use URL-based versioning:

```
https://api.mystira.app/v1/...
https://api.mystira.app/v2/...  # Future
```

Current API version: **v1**

## Health Check Endpoints

All services expose standard health endpoints:

| Endpoint | Purpose | Response |
|----------|---------|----------|
| `/health` | Full health check | `HealthCheckResult` |
| `/health/ready` | K8s readiness | 200/503 |
| `/health/live` | K8s liveness | 200 |

Example:
```bash
# Check production API health
curl https://api.mystira.app/health

# Check dev story API
curl https://dev.story-api.mystira.app/health
```

## Environment Detection

### Backend (C#)

```csharp
public static class EnvironmentDetector
{
    public static string GetEnvironment()
    {
        var host = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME") ?? "";

        if (host.StartsWith("dev.")) return "Development";
        if (host.StartsWith("staging.")) return "Staging";
        return "Production";
    }

    public static string GetApiUrl(string service = "api")
    {
        var env = GetEnvironment();
        var prefix = env switch
        {
            "Development" => "dev.",
            "Staging" => "staging.",
            _ => ""
        };

        return $"https://{prefix}{service}.mystira.app";
    }
}
```

### Frontend (TypeScript)

```typescript
export function getApiUrl(service: string = 'api'): string {
  const hostname = window.location.hostname;

  let prefix = '';
  if (hostname.startsWith('dev.')) prefix = 'dev.';
  else if (hostname.startsWith('staging.')) prefix = 'staging.';

  return `https://${prefix}${service}.mystira.app`;
}
```

## CI/CD Updates

### GitHub Actions

```yaml
# .github/workflows/deploy.yml
jobs:
  deploy:
    environment:
      name: ${{ matrix.environment }}
      url: https://${{ matrix.prefix }}mystira.app

    strategy:
      matrix:
        include:
          - environment: development
            prefix: 'dev.'
          - environment: staging
            prefix: 'staging.'
          - environment: production
            prefix: ''
```

## Migration Checklist

### Infrastructure
- [ ] Update Azure DNS zone records
- [ ] Update Azure Front Door custom domains
- [ ] Update Let's Encrypt certificate configurations
- [ ] Update Kubernetes ingress configurations
- [ ] Update Terraform domain variables

### Applications
- [ ] Update all hardcoded URLs in codebase
- [ ] Update environment variables
- [ ] Update CORS configurations
- [ ] Update OAuth redirect URIs
- [ ] Update webhook URLs

### Documentation
- [ ] Update API documentation
- [ ] Update README files
- [ ] Update developer onboarding guides
- [ ] Update deployment runbooks

### Monitoring
- [ ] Update health check URLs
- [ ] Update uptime monitoring
- [ ] Update alerting configurations
- [ ] Update logging configurations

## Contact

For questions or assistance:
- Engineering: engineering@mystira.app
- DevOps: devops@mystira.app
- Issues: https://github.com/phoenixvc/Mystira.workspace/issues
