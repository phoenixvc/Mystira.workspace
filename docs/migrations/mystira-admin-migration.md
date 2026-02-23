# Mystira.Admin Migration Guide

**Target**: Migrate Mystira.Admin.Api to use `Mystira.Shared` infrastructure patterns
**Prerequisites**: Monorepo with ProjectReferences to shared packages (done)
**Estimated Effort**: 1 day
**Last Updated**: February 2026
**Status**: 📋 Ready to start

> **Note**: The monorepo migration is complete. Admin.Api already targets .NET 10 and uses ProjectReferences to all shared packages (`Mystira.Shared`, `Mystira.Domain`, `Mystira.Application`, `Mystira.Contracts`, `Mystira.Infrastructure.*`). This migration covers adopting the shared infrastructure patterns (resilience, specifications, exception handling, etc.).

---

## Overview

Admin.Api is already well-positioned for migration with:

- .NET 10.0 and ProjectReferences (done via monorepo migration)
- Redis caching already configured
- PostgreSQL + Cosmos DB support
- Microsoft Identity Web for auth

Remaining migration focuses on:

1. ~~.NET 10.0 upgrade~~ — ✅ DONE (monorepo migration)
2. ~~ProjectReferences to shared packages~~ — ✅ DONE (monorepo migration)
3. Adopt `Mystira.Shared.Resilience` for HTTP policies (Polly v8)
4. Adopt Ardalis.Specification 9.3.1 for data access
5. Standardize exception handling → `Mystira.Shared.Exceptions`
6. (Optional) Add Wolverine for event handling
7. (Optional) Add distributed locking for concurrent admin operations

---

## Current State Analysis

### Package Dependencies (Admin.Api)

All internal Mystira packages are already referenced via `<ProjectReference>`:

```xml
<ProjectReference Include="../../../domain/Mystira.Domain/Mystira.Domain.csproj" />
<ProjectReference Include="../../../application/Mystira.Application/Mystira.Application.csproj" />
<ProjectReference Include="../../../contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj" />
<ProjectReference Include="../../../infrastructure/Mystira.Infrastructure.Azure/Mystira.Infrastructure.Azure.csproj" />
<ProjectReference Include="../../../infrastructure/Mystira.Infrastructure.Data/Mystira.Infrastructure.Data.csproj" />
<ProjectReference Include="../../../shared/Mystira.Shared/Mystira.Shared.csproj" />
```

### What Admin.Api Already Has

- .NET 10.0 target framework
- ProjectReferences to all shared packages
- Redis caching
- PostgreSQL support
- Cosmos DB support
- Health checks
- JWT + Entra ID auth

---

## Phase 1: Resilience Policies (Polly v8)

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

| Before (v7)          | After (v8)               |
| -------------------- | ------------------------ |
| `IAsyncPolicy<T>`    | `ResiliencePipeline<T>`  |
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

- [x] .NET 10.0 upgrade (done via monorepo migration)
- [x] ProjectReferences to shared packages (done via monorepo migration)
- [ ] Create feature branch
- [ ] Backup current appsettings.json

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

| Change                                        | Impact                 | Mitigation                                       |
| --------------------------------------------- | ---------------------- | ------------------------------------------------ |
| Polly v7 → v8                                 | Policy API changes     | Use new ResiliencePipeline API                   |
| Custom queries → Specifications               | Query patterns change  | Gradual migration                                |
| Custom exceptions → Mystira.Shared.Exceptions | Exception types change | Use NotFoundException, ValidationException, etc. |

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
const mystiraPreset = require("@mystira/design-tokens/tailwind/preset");

module.exports = {
  presets: [mystiraPreset],
  // ...
};
```

```css
/* Or import CSS variables */
@import "@mystira/design-tokens/css/variables.css";
@import "@mystira/design-tokens/css/dark-mode.css";
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
