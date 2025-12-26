# Mystira.Admin Migration Guide

**Target**: Migrate Mystira.Admin.Api to use `Mystira.Shared` infrastructure
**Prerequisites**: Mystira.Shared v0.2.0-alpha published to NuGet feed
**Estimated Effort**: 1 day
**Last Updated**: December 2025
**Status**: 🔄 In Progress

> **Note**: All infrastructure components (Polly v8.6.5, Wolverine 5.9.2, Ardalis.Specification 9.3.1) are already implemented in `Mystira.Shared`. This migration is about adopting the shared package.

---

## Overview

Admin.Api is already well-positioned for migration with:
- Redis caching already configured
- PostgreSQL + Cosmos DB support
- Microsoft Identity Web for auth

Migration focuses on:
1. **.NET 9.0 upgrade** (required)
2. Replace `Mystira.App.Shared` → `Mystira.Shared` (v0.2.0-alpha)
3. Adopt `Mystira.Shared.Resilience` for HTTP policies (Polly v8.6.5)
4. Adopt Ardalis.Specification 9.3.1 for data access (included in Mystira.Shared)
5. Standardize exception handling
6. (Optional) Add Wolverine 5.9.2 for future event handling (included in Mystira.Shared)
7. (Optional) Add distributed locking for concurrent admin operations
8. Migrate to Microsoft Entra External ID (if applicable)

---

## Current State Analysis

### Package Dependencies (Admin.Api)

| Current Package | Action | Replacement |
|-----------------|--------|-------------|
| `Mystira.App.Shared` | Replace | `Mystira.Shared` |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Keep | Already using Redis |
| `Microsoft.Identity.Web` | Keep/Update | Auth still needed (update for Entra External ID) |
| `Mystira.App.Contracts` | Replace | `Mystira.Contracts.App` |

### What Admin.Api Already Has
- Redis caching
- PostgreSQL support
- Cosmos DB support
- Health checks
- JWT + Entra ID auth

---

## Phase 1: .NET 9.0 Upgrade & Package Updates

### 1.1 Update Target Framework

```xml
<!-- Update in all .csproj files -->
<TargetFramework>net9.0</TargetFramework>
```

### 1.2 Update Mystira.App.Admin.Api.csproj

```xml
<!-- Remove -->
<PackageReference Include="Mystira.App.Shared" Version="1.0.0" />
<PackageReference Include="Mystira.App.Contracts" Version="1.0.0" />

<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.2.0" />
<PackageReference Include="Mystira.Contracts" Version="0.2.0" />
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
<PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
```

### 1.3 Update Using Statements

```csharp
// From
using Mystira.App.Shared.Auth;
using Mystira.App.Shared.Extensions;

// To
using Mystira.Shared.Auth;
using Mystira.Shared.Extensions;
using Mystira.Shared.Resilience;
using Ardalis.Specification;
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

## Phase 3: Resilience Policies (Polly v8)

### 3.1 Add HTTP Resilience

```csharp
// Program.cs
builder.Services.AddMystiraResilience(builder.Configuration);

// Option 1: Standard resilience handler (recommended)
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>()
    .AddStandardResilienceHandler();

// Option 2: Custom resilience pipeline
builder.Services.AddResilientHttpClientV8<IExternalApiClient, ExternalApiClient>(
    "ExternalApi",
    client => client.BaseAddress = new Uri("https://external-api.example.com"));
```

### 3.2 Configuration

```json
// appsettings.json
{
  "Resilience": {
    "MaxRetries": 3,
    "BaseDelaySeconds": 2,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "TimeoutSeconds": 30,
    "EnableDetailedLogging": true
  }
}
```

### 3.3 Breaking Changes (Polly v7 → v8)

| Before (v7) | After (v8) |
|-------------|------------|
| `IAsyncPolicy<T>` | `ResiliencePipeline<T>` |
| `AddPolicyHandler()` | `AddResilienceHandler()` |

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

## Phase 5: Ardalis.Specification 8.0.0 Migration

### 5.1 Create Specification Classes

```csharp
using Ardalis.Specification;

namespace Mystira.Admin.Domain.Specifications;

public sealed class ContentByIdSpec : Specification<Content>, ISingleResultSpecification<Content>
{
    public ContentByIdSpec(string contentId)
    {
        Query
            .Where(c => c.Id == contentId)
            .AsNoTracking();
    }
}

public sealed class PublishedContentSpec : Specification<Content>
{
    public PublishedContentSpec(int page = 1, int pageSize = 20)
    {
        Query
            .Where(c => c.Status == ContentStatus.Published)
            .OrderByDescending(c => c.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
```

### 5.2 Update Repository Usage

```csharp
public class ContentService
{
    private readonly IReadRepository<Content> _repository;

    public async Task<Content?> GetByIdAsync(string id, CancellationToken ct)
    {
        var spec = new ContentByIdSpec(id);
        return await _repository.SingleOrDefaultAsync(spec, ct);
    }
}
```

---

## Phase 6: Polyglot Repository (Optional)

Admin.Api already has dual-database support. Optionally adopt PolyglotRepository:

### 6.1 Register Polyglot Persistence

```csharp
// Program.cs
builder.Services.AddPolyglotPersistence<CosmosDbContext, PostgresDbContext>(builder.Configuration);
```

### 6.2 Configuration

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

## Phase 7: Distributed Locking (Optional)

For admin operations that require exclusive access:

### 7.1 Setup

```csharp
// Program.cs
builder.Services.AddMystiraDistributedLocking(builder.Configuration);
```

### 7.2 Usage

```csharp
public class ContentPublishService
{
    private readonly IDistributedLockService _lockService;

    public async Task PublishContentAsync(string contentId, CancellationToken ct)
    {
        await _lockService.ExecuteWithLockAsync(
            $"content:publish:{contentId}",
            async token =>
            {
                // Only one admin can publish this content at a time
                await DoPublishAsync(contentId, token);
            },
            expiry: TimeSpan.FromMinutes(2),
            wait: TimeSpan.FromSeconds(30),
            ct);
    }
}
```

---

## Phase 8: Wolverine (Optional - Future)

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
- [ ] Ensure Mystira.Shared v0.2.0+ is published
- [ ] Create feature branch
- [ ] Backup current appsettings.json
- [ ] Backup Key Vault secrets

### Phase 1: .NET 9.0 Upgrade
- [ ] Update target framework to net9.0
- [ ] Update csproj references
- [ ] Add Ardalis.Specification packages
- [ ] Update using statements
- [ ] Verify build succeeds

### Phase 2: Caching
- [ ] Add ICacheService registration
- [ ] Update cache consumers to use ICacheService

### Phase 3: Resilience (Polly v8)
- [ ] Add resilience configuration
- [ ] Replace `AddPolicyHandler` with `AddResilienceHandler`
- [ ] Add policies to HTTP clients

### Phase 4: Exceptions
- [ ] Add GlobalExceptionHandler
- [ ] Update exception throws to use Mystira.Shared types

### Phase 5: Specification Pattern
- [ ] Create specification classes for data access
- [ ] Update repositories to use Ardalis.Specification
- [ ] Update service layer to use specifications

### Phase 6: Polyglot (Optional)
- [ ] Add PolyglotRepository configuration
- [ ] Annotate entities with DatabaseTarget

### Phase 7: Distributed Locking (Optional)
- [ ] Add distributed locking configuration
- [ ] Implement locks for concurrent admin operations

### Phase 8: Wolverine (Optional)
- [ ] Add Wolverine for event handling
- [ ] Configure Azure Service Bus integration

### Post-Migration
- [ ] Run all tests
- [ ] Test API endpoints
- [ ] Verify health checks
- [ ] Performance testing
- [ ] Create PR

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| .NET 8 → .NET 9 | Runtime upgrade required | Test thoroughly in staging |
| Namespace changes | Using statements | Find/replace |
| Mystira.App.Contracts | Import paths | Update to Mystira.Contracts.App |
| Polly v7 → v8 | Policy API changes | Use new ResiliencePipeline API |
| Custom queries → Specifications | Query patterns change | Gradual migration |

---

## Admin-UI Changes

Admin-UI (React) migration is separate - see [mystira-admin-ui-migration.md](./mystira-admin-ui-migration.md):

### Design Tokens Migration

```bash
# In packages/admin-ui
npm install @mystira/design-tokens@0.2.0
npm install @mystira/shared-utils@0.2.0
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
@import '@mystira/design-tokens/css/dark-mode.css';
```

---

## Related Documentation

- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0017: Resource Group Organization](../architecture/adr/0017-resource-group-organization-strategy.md)
- [Ardalis.Specification 8.0.0 Guide](../architecture/specifications/ardalis-specification-migration.md)
- [Mystira.App Migration Guide](./mystira-app-migration.md)
- [Mystira.Shared Migration Guide](../guides/mystira-shared-migration.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
