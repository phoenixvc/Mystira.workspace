# Mystira.Admin Migration Guide

**Target**: Migrate Mystira.Admin.Api to use `Mystira.Shared` infrastructure
**Prerequisites**: Mystira.Shared v0.1.0+ published to NuGet feed
**Estimated Effort**: 1 day

---

## Overview

Admin.Api is already well-positioned for migration with:
- Redis caching already configured
- PostgreSQL + Cosmos DB support
- Microsoft Identity Web for auth

Migration focuses on:
1. Replace `Mystira.App.Shared` → `Mystira.Shared`
2. Adopt `Mystira.Shared.Resilience` for HTTP policies
3. Standardize exception handling
4. (Optional) Add Wolverine for future event handling

---

## Current State Analysis

### Package Dependencies (Admin.Api)

| Current Package | Action | Replacement |
|-----------------|--------|-------------|
| `Mystira.App.Shared` | Replace | `Mystira.Shared` |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Keep | Already using Redis |
| `Microsoft.Identity.Web` | Keep | Auth still needed |
| `Mystira.App.Contracts` | Replace | `Mystira.Contracts.App` |

### What Admin.Api Already Has
- Redis caching (line 35)
- PostgreSQL support (line 31)
- Cosmos DB support (line 16)
- Health checks (lines 20-23, 32, 36)
- JWT + Entra ID auth (lines 18, 39)

---

## Phase 1: Update Package References

### 1.1 Update Mystira.App.Admin.Api.csproj

```xml
<!-- Remove -->
<PackageReference Include="Mystira.App.Shared" Version="1.0.0" />
<PackageReference Include="Mystira.App.Contracts" Version="1.0.0" />

<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
<PackageReference Include="Mystira.Contracts" Version="0.1.0" />
```

### 1.2 Update Using Statements

```csharp
// From
using Mystira.App.Shared.Auth;
using Mystira.App.Shared.Extensions;

// To
using Mystira.Shared.Auth;
using Mystira.Shared.Extensions;
```

---

## Phase 2: Caching Integration

Admin.Api already uses Redis. Optionally adopt `ICacheService` for consistency:

### 2.1 Current State

```csharp
// Current: Direct IDistributedCache usage
public class ContentService
{
    private readonly IDistributedCache _cache;

    public async Task<Content?> GetContentAsync(string id)
    {
        var cached = await _cache.GetStringAsync($"content:{id}");
        if (cached != null)
            return JsonSerializer.Deserialize<Content>(cached);
        // ...
    }
}
```

### 2.2 Target State (Optional)

```csharp
// Target: Use ICacheService wrapper
using Mystira.Shared.Caching;

public class ContentService
{
    private readonly ICacheService _cache;

    public async Task<Content?> GetContentAsync(string id, CancellationToken ct)
    {
        return await _cache.GetOrSetAsync(
            $"content:{id}",
            async () => await _repository.GetByIdAsync(id, ct),
            TimeSpan.FromMinutes(10),
            ct);
    }
}
```

### 2.3 Registration

```csharp
// Program.cs
builder.Services.AddMystiraCaching(builder.Configuration);
```

---

## Phase 3: Resilience Policies

### 3.1 Add HTTP Resilience

```csharp
// Program.cs
builder.Services.AddMystiraResilience(builder.Configuration);

// For any HTTP clients
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>()
    .AddMystiraResiliencePolicy("ExternalApi");
```

### 3.2 Configuration

```json
// appsettings.json
{
  "Resilience": {
    "RetryCount": 3,
    "RetryBaseDelayMs": 1000,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "TimeoutSeconds": 30
  }
}
```

---

## Phase 4: Exception Handling

### 4.1 Add Global Exception Handler

```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In middleware pipeline
app.UseExceptionHandler();
```

### 4.2 Use Standard Exceptions

```csharp
using Mystira.Shared.Exceptions;

// Instead of custom exceptions
throw new NotFoundException("Content", id);
throw new ForbiddenException("User lacks permission");
throw new ConflictException("Content already exists");
```

---

## Phase 5: Polyglot Repository (Optional)

Admin.Api already has dual-database support. Optionally adopt PolyglotRepository:

### 5.1 Register Polyglot Persistence

```csharp
// Program.cs
builder.Services.AddPolyglotPersistence<CosmosDbContext, PostgresDbContext>(builder.Configuration);
```

### 5.2 Configuration

```json
// appsettings.json
{
  "Polyglot": {
    "DefaultTarget": "CosmosDb",
    "EnableCaching": true,
    "CacheExpirationSeconds": 300,
    "EntityRouting": {
      "Mystira.App.Domain.Analytics.AnalyticsEvent": "PostgreSql"
    }
  }
}
```

---

## Phase 6: Wolverine (Optional - Future)

If Admin.Api needs to handle events from other services:

```csharp
// Program.cs
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);

    // Listen for events from Azure Service Bus
    opts.ListenToAzureServiceBusQueue("admin-events");
});
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure Mystira.Shared is published
- [ ] Create feature branch
- [ ] Backup current appsettings.json

### Phase 1: Package Update
- [ ] Update csproj references
- [ ] Update using statements
- [ ] Verify build succeeds

### Phase 2: Caching (Optional)
- [ ] Add ICacheService registration
- [ ] Update cache consumers (or keep IDistributedCache)

### Phase 3: Resilience
- [ ] Add resilience configuration
- [ ] Add policies to HTTP clients

### Phase 4: Exceptions
- [ ] Add GlobalExceptionHandler
- [ ] Update exception throws to use Mystira.Shared types

### Phase 5: Polyglot (Optional)
- [ ] Add PolyglotRepository configuration
- [ ] Annotate entities with DatabaseTarget

### Post-Migration
- [ ] Run all tests
- [ ] Test API endpoints
- [ ] Verify health checks
- [ ] Create PR

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| Namespace changes | Using statements | Find/replace |
| Mystira.App.Contracts | Import paths | Update to Mystira.Contracts.App |

---

## Admin-UI Changes

Admin-UI (React) migration is separate:

### Design Tokens Migration

```bash
# In packages/admin-ui
npm install @mystira/design-tokens
```

```javascript
// tailwind.config.js (if using Tailwind)
const mystiraPreset = require('@mystira/design-tokens/tailwind/preset');

module.exports = {
  presets: [mystiraPreset],
  // ...
};
```

```css
/* Or import CSS variables */
@import '@mystira/design-tokens/css/variables.css';
```

---

## Related Documentation

- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [Mystira.App Migration Guide](./mystira-app-migration.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
