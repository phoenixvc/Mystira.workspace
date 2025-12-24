# Mystira.App Migration Guide

**Target**: Migrate Mystira.App to use `Mystira.Shared` infrastructure
**Prerequisites**: Mystira.Shared v0.1.0+ published to NuGet feed
**Estimated Effort**: 2-3 days

---

## Overview

This guide covers migrating Mystira.App from its current infrastructure to the consolidated `Mystira.Shared` package, including:

1. MediatR → Wolverine migration
2. Custom resilience → `Mystira.Shared.Resilience`
3. IMemoryCache → `Mystira.Shared.Caching` (Redis)
4. Custom exceptions → `Mystira.Shared.Exceptions`
5. Repository pattern alignment

---

## Phase 1: Add Package Reference

### 1.1 Update Mystira.App.Application.csproj

```xml
<!-- Remove -->
<PackageReference Include="MediatR" Version="12.4.1" />

<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
```

### 1.2 Update Mystira.App.Api.csproj

```xml
<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
```

### 1.3 Update Mystira.App.PWA.csproj

```xml
<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
```

---

## Phase 2: Wolverine Migration (MediatR Replacement)

### 2.1 Current State

```csharp
// Current: MediatR handler
public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken ct)
    {
        // handler logic
    }
}

// Current: MediatR usage
var result = await _mediator.Send(new GetAccountQuery(id));
```

### 2.2 Target State

```csharp
// Target: Wolverine handler (convention-based)
public static class GetAccountQueryHandler
{
    public static async Task<AccountDto> Handle(
        GetAccountQuery query,
        IAccountRepository repository,
        CancellationToken ct)
    {
        // handler logic - dependencies injected as parameters
    }
}

// Target: Wolverine usage
var result = await _bus.InvokeAsync<AccountDto>(new GetAccountQuery(id));
```

### 2.3 Migration Steps

1. **Update Program.cs**:
```csharp
// Remove
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(GetAccountQuery).Assembly);

    // Optional: Azure Service Bus for distributed messaging
    // opts.UseAzureServiceBus(builder.Configuration.GetConnectionString("ServiceBus"));
});
```

2. **Convert handlers one at a time**:
   - Start with query handlers (read operations)
   - Then command handlers (write operations)
   - Keep both MediatR and Wolverine running during migration

3. **Update marker interfaces**:
```csharp
// From
public record GetAccountQuery(Guid Id) : IRequest<AccountDto>;

// To
public record GetAccountQuery(Guid Id) : IQuery<AccountDto>;
```

### 2.4 Handler Conversion Checklist

| Handler | Type | Status |
|---------|------|--------|
| `GetAccountQueryHandler` | Query | ⬜ Pending |
| `GetAccountsQueryHandler` | Query | ⬜ Pending |
| `CreateAccountCommandHandler` | Command | ⬜ Pending |
| `UpdateAccountCommandHandler` | Command | ⬜ Pending |
| `DeleteAccountCommandHandler` | Command | ⬜ Pending |
| ... | ... | ... |

---

## Phase 3: Resilience Migration

### 3.1 Current State (PWA/Program.cs)

```csharp
// Current: Duplicated policy for each HTTP client
IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy(string clientName)
{
    var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    var circuitBreaker = HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
    return Policy.WrapAsync(timeout, retryPolicy, circuitBreaker);
}
```

### 3.2 Target State

```csharp
// Target: Use Mystira.Shared.Resilience
using Mystira.Shared.Resilience;

// In Program.cs
builder.Services.AddMystiraResilience(builder.Configuration);

// In HTTP client registration
builder.Services.AddHttpClient<IAccountApiClient, AccountApiClient>()
    .AddMystiraResiliencePolicy("AccountApi");
```

### 3.3 Configuration

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

## Phase 4: Caching Migration

### 4.1 Current State

```csharp
// Current: IMemoryCache with MediatR pipeline
public class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IMemoryCache _cache;
    // ...
}
```

### 4.2 Target State

```csharp
// Target: ICacheService with Redis
using Mystira.Shared.Caching;

public class AccountService
{
    private readonly ICacheService _cache;

    public async Task<AccountDto?> GetAccountAsync(Guid id, CancellationToken ct)
    {
        return await _cache.GetOrSetAsync(
            $"account:{id}",
            async () => await _repository.GetByIdAsync(id, ct),
            TimeSpan.FromMinutes(5),
            ct);
    }
}
```

### 4.3 Configuration

```json
// appsettings.json
{
  "Cache": {
    "Provider": "Redis",  // or "Memory" for development
    "Redis": {
      "ConnectionString": "your-redis-connection"
    },
    "DefaultExpirationMinutes": 5
  }
}
```

### 4.4 Registration

```csharp
// Program.cs
builder.Services.AddMystiraCaching(builder.Configuration);
```

---

## Phase 5: Exception/Error Handling Migration

### 5.1 Current State

```csharp
// Current: Custom exception types scattered across layers
public class AccountNotFoundException : Exception { }
public class ValidationException : Exception { }
```

### 5.2 Target State

```csharp
// Target: Use Mystira.Shared.Exceptions
using Mystira.Shared.Exceptions;

// Throw domain exceptions
throw new NotFoundException("Account", id);
throw new ValidationException("Email is required");
throw new ConflictException("Account already exists");

// Or use Result<T> pattern
public async Task<Result<AccountDto>> GetAccountAsync(Guid id)
{
    var account = await _repository.GetByIdAsync(id);
    if (account is null)
        return Result<AccountDto>.Failure(Error.NotFound("Account", id));

    return Result<AccountDto>.Success(account.ToDto());
}
```

### 5.3 Global Exception Handler

```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// In pipeline
app.UseExceptionHandler();
```

---

## Phase 6: Repository Alignment (Optional)

If you want to use the new `PolyglotRepository`:

### 6.1 Entity Annotation

```csharp
using Mystira.Shared.Data.Polyglot;

[DatabaseTarget(DatabaseTarget.CosmosDb, Rationale = "Complex nested document")]
public class Scenario : AuditableEntity
{
    // ...
}

[DatabaseTarget(DatabaseTarget.PostgreSql, Rationale = "Relational analytics")]
public class AnalyticsEvent : Entity
{
    // ...
}
```

### 6.2 Repository Registration

```csharp
// Program.cs
builder.Services.AddPolyglotPersistence<CosmosDbContext, PostgresDbContext>(builder.Configuration);
builder.Services.AddPolyglotRepository<Scenario>();
builder.Services.AddPolyglotRepository<AnalyticsEvent>();
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure Mystira.Shared is published to NuGet feed
- [ ] Create feature branch for migration
- [ ] Review current MediatR handlers count
- [ ] Review current HTTP clients count

### Phase 1: Package Setup
- [ ] Add Mystira.Shared reference to all projects
- [ ] Verify build succeeds

### Phase 2: Wolverine
- [ ] Add Wolverine to Program.cs (alongside MediatR)
- [ ] Convert 1-2 query handlers as pilot
- [ ] Verify pilot handlers work
- [ ] Convert remaining query handlers
- [ ] Convert command handlers
- [ ] Remove MediatR package reference

### Phase 3: Resilience
- [ ] Add resilience configuration
- [ ] Replace custom policies with AddMystiraResiliencePolicy
- [ ] Remove custom CreateResiliencePolicy method

### Phase 4: Caching
- [ ] Add caching configuration
- [ ] Replace IMemoryCache with ICacheService
- [ ] Remove QueryCachingBehavior

### Phase 5: Exceptions
- [ ] Add global exception handler
- [ ] Replace custom exceptions with Mystira.Shared types
- [ ] Update API responses to use ProblemDetails

### Post-Migration
- [ ] Run all tests
- [ ] Verify API responses unchanged
- [ ] Performance testing
- [ ] Create PR

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| MediatR → Wolverine | Handler signatures change | Gradual migration with both running |
| IMemoryCache → Redis | Requires Redis in production | Use Memory provider for dev |
| Exception types | API error responses may change | Use GlobalExceptionHandler for consistency |

---

## Rollback Plan

If migration causes issues:

1. Keep MediatR handlers intact during migration
2. Feature flag Wolverine handlers: `UseWolverineHandlers: true/false`
3. Redis cache can fall back to memory cache
4. Global exception handler can be disabled

---

## Related Documentation

- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
