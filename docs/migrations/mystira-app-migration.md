# Mystira.App Migration Guide

**Target**: Migrate Mystira.App to use `Mystira.Shared` infrastructure
**Prerequisites**: Mystira.Shared v0.4.* published to NuGet feed
**Estimated Effort**: 2-3 days
**Last Updated**: December 2025
**Status**: 🔄 In Progress

---

## Overview

This guide covers migrating Mystira.App from its current infrastructure to the consolidated `Mystira.Shared` package, including:

1. **.NET 9.0 upgrade** (required)
2. MediatR → Wolverine migration (v5.9.2)
3. Custom resilience → `Mystira.Shared.Resilience` (Polly v8.6.5)
4. IMemoryCache → `Mystira.Shared.Caching` (Redis)
5. Custom exceptions → `Mystira.Shared.Exceptions`
6. Repository pattern → Ardalis.Specification 9.3.1
7. **Distributed locking** for concurrent operations
8. **Microsoft Entra External ID** authentication (optional)
9. **Source generators** for repositories and validators

> **Note**: All these components are already implemented in `Mystira.Shared` (v0.4.*). This migration is about adopting the shared package, not building new infrastructure.

---

## Phase 1: .NET 9.0 Upgrade & Package Updates

### 1.1 Update Target Framework (All Projects)

```xml
<!-- Update in all .csproj files -->
<TargetFramework>net9.0</TargetFramework>
```

### 1.2 Update Mystira.App.Application.csproj

```xml
<!-- Remove -->
<PackageReference Include="MediatR" Version="12.4.1" />

<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.4.*" />
<!-- Ardalis.Specification 9.3.1 is included via Mystira.Shared -->
```

### 1.3 Update Mystira.App.Api.csproj

```xml
<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.4.*" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
```

### 1.4 Update Mystira.App.PWA.csproj

```xml
<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.4.*" />

<!-- For WASM caching support -->
<PackageReference Include="Blazored.LocalStorage" Version="4.5.0" />
```

### 1.5 Update Mystira.App.Domain.csproj

```xml
<!-- Ardalis.Specification 9.3.1 is included via Mystira.Shared -->
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
public class GetAccountQueryHandler
{
    public async Task<AccountDto> Handle(
        GetAccountQuery query,
        IAccountRepository repository,
        CancellationToken ct)
    {
        // handler logic - dependencies injected as parameters
        var account = await repository.GetByIdAsync(query.Id, ct);
        if (account == null)
            throw new NotFoundException($"Account {query.Id} not found");
        
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            // ... map other properties
        };
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

## Phase 3: Resilience Migration (Polly v8)

### 3.1 Current State (Polly v7)

```csharp
// Current: Legacy IAsyncPolicy pattern
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

### 3.2 Target State (Polly v8)

```csharp
// Target: Use Mystira.Shared.Resilience with Polly v8
using Mystira.Shared.Resilience;

// In Program.cs - Option 1: Standard resilience handler
builder.Services.AddMystiraResilience(builder.Configuration);

builder.Services.AddHttpClient<IAccountApiClient, AccountApiClient>()
    .AddStandardResilienceHandler(); // Built-in .NET 8+ pattern

// Option 2: Custom resilience pipeline
builder.Services.AddResilientHttpClientV8<IAccountApiClient, AccountApiClient>(
    "AccountApi",
    client => client.BaseAddress = new Uri("https://api.mystira.app"));

// Option 3: For non-HTTP operations
var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>(
    "DatabaseOperation",
    new ResilienceOptions { MaxRetries = 3 });

var result = await pipeline.ExecuteAsync(async ct =>
{
    return await _database.QueryAsync(query, ct);
});
```

### 3.3 Configuration

```json
// appsettings.json
{
  "Resilience": {
    "MaxRetries": 3,
    "BaseDelaySeconds": 2,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "TimeoutSeconds": 30,
    "LongRunningTimeoutSeconds": 300,
    "EnableDetailedLogging": true
  }
}
```

### 3.4 Breaking Changes (Polly v7 → v8)

| Before (v7) | After (v8) |
|-------------|------------|
| `IAsyncPolicy<T>` | `ResiliencePipeline<T>` |
| `Policy.WrapAsync()` | `ResiliencePipelineBuilder` |
| `AddPolicyHandler()` | `AddResilienceHandler()` |
| `PolicyFactory.CreateStandardHttpPolicy()` | `ResiliencePipelineFactory.CreateStandardHttpPipeline()` |

---

## Phase 4: Caching Migration (Redis + WASM)

### 4.1 Current State

```csharp
// Current: IMemoryCache with MediatR pipeline
public class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IMemoryCache _cache;
    // ...
}
```

### 4.2 Target State (Server-Side)

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

### 4.3 Target State (WASM/PWA)

```csharp
// For Blazor WASM: IndexedDB-backed cache
using Mystira.Shared.Caching.Wasm;

public class WasmAccountService
{
    private readonly IWasmCacheService _cache;

    public async Task<AccountDto?> GetAccountAsync(Guid id)
    {
        return await _cache.GetOrSetAsync(
            $"account:{id}",
            async () => await _apiClient.GetAccountAsync(id),
            TimeSpan.FromMinutes(30)); // Longer TTL for offline support
    }
}
```

### 4.4 Configuration

```json
// appsettings.json (Server)
{
  "Cache": {
    "Provider": "Redis",
    "ConnectionString": "your-redis-connection",
    "InstanceName": "Mystira:",
    "DefaultExpirationMinutes": 60
  }
}
```

```json
// wwwroot/appsettings.json (WASM)
{
  "Cache": {
    "Provider": "IndexedDB",
    "DatabaseName": "MystiraCache",
    "DefaultExpirationMinutes": 30,
    "MaxCacheSizeMB": 50
  }
}
```

### 4.5 Registration

```csharp
// Program.cs (Server)
builder.Services.AddMystiraCaching(builder.Configuration);

// Program.cs (WASM)
builder.Services.AddMystiraWasmCaching(builder.Configuration);
```

### 4.6 Cache Compression

```csharp
// Enable compression for large cached objects
builder.Services.AddMystiraCaching(builder.Configuration, options =>
{
    options.EnableCompression = true;
    options.CompressionThresholdBytes = 1024; // Compress items > 1KB
});
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

## Phase 6: Ardalis.Specification 9.3.1 Migration

### 6.1 Create Specification Classes

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

// Single result specification
public sealed class AccountByIdSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByIdSpec(string accountId)
    {
        Query
            .Where(a => a.Id == accountId)
            .AsNoTracking();
    }
}

// Paginated list specification
public sealed class ActiveAccountsSpec : Specification<Account>
{
    public ActiveAccountsSpec(int page = 1, int pageSize = 20)
    {
        Query
            .Where(a => a.Status == AccountStatus.Active)
            .OrderByDescending(a => a.LastLoginAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
```

### 6.2 Update Repository Interfaces

```csharp
using Ardalis.Specification;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
    // Inherits from Ardalis.Specification
}

public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class
{
    // Inherits from Ardalis.Specification
}
```

### 6.3 Update Service Layer

```csharp
public class AccountService
{
    private readonly IReadRepository<Account> _repository;

    public async Task<Account?> GetByIdAsync(string id, CancellationToken ct)
    {
        var spec = new AccountByIdSpec(id);
        return await _repository.SingleOrDefaultAsync(spec, ct);
    }

    public async Task<List<Account>> GetActiveAccountsAsync(int page, int pageSize, CancellationToken ct)
    {
        var spec = new ActiveAccountsSpec(page, pageSize);
        return await _repository.ListAsync(spec, ct);
    }
}
```

---

## Phase 7: Polyglot Repository (Optional)

If you want to use the new `PolyglotRepository`:

### 7.1 Entity Annotation

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

### 7.2 Repository Registration

```csharp
// Program.cs
builder.Services.AddPolyglotPersistence<CosmosDbContext, PostgresDbContext>(builder.Configuration);
builder.Services.AddPolyglotRepository<Scenario>();
builder.Services.AddPolyglotRepository<AnalyticsEvent>();
```

---

## Phase 8: Distributed Locking

### 8.1 Setup

```csharp
// Program.cs
builder.Services.AddMystiraCaching(builder.Configuration); // Requires Redis
builder.Services.AddMystiraDistributedLocking(builder.Configuration);
```

### 8.2 Configuration

```json
{
  "DistributedLock": {
    "DefaultExpirySeconds": 30,
    "DefaultWaitSeconds": 10,
    "RetryIntervalMs": 100,
    "KeyPrefix": "lock:",
    "EnableDetailedLogging": true
  }
}
```

### 8.3 Usage

```csharp
public class GameSessionService
{
    private readonly IDistributedLockService _lockService;

    public async Task ProcessSessionAsync(Guid sessionId, CancellationToken ct)
    {
        await _lockService.ExecuteWithLockAsync(
            $"session:{sessionId}",
            async token =>
            {
                // Only one instance processes this session at a time
                await DoProcessingAsync(sessionId, token);
            },
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(10),
            ct);
    }
}
```

---

## Phase 9: Source Generators (Optional)

### 9.1 Repository Generation

```csharp
using Mystira.Shared.Data;

[GenerateRepository]
public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);
}

// Extend with partial class
public partial class AccountRepositoryGenerated
{
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.Email == email, ct);
    }
}
```

### 9.2 Options Validation Generation

```csharp
using Mystira.Shared.Validation;

[GenerateValidator]
public class GameSessionOptions
{
    [ValidatePositive]
    public int MaxConcurrentSessions { get; set; } = 10;

    [ValidateRange(1, 3600)]
    public int SessionTimeoutSeconds { get; set; } = 300;

    [ValidateNotEmpty]
    public string DefaultScenarioId { get; set; } = "";
}

// Registration (auto-generated extension method)
builder.Services.AddGameSessionOptionsValidation();
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure Mystira.Shared v0.4.* is published to NuGet feed
- [ ] Create feature branch for migration
- [ ] Review current MediatR handlers count
- [ ] Review current HTTP clients count
- [ ] Backup Key Vault secrets

### Phase 1: .NET 9.0 Upgrade
- [ ] Update all .csproj files to net9.0
- [ ] Update package references to latest compatible versions
- [ ] Add Ardalis.Specification 9.3.1 (via Mystira.Shared)
- [ ] Verify build succeeds

### Phase 2: Wolverine
- [ ] Add Wolverine to Program.cs (alongside MediatR)
- [ ] Convert 1-2 query handlers as pilot
- [ ] Verify pilot handlers work
- [ ] Convert remaining query handlers
- [ ] Convert command handlers
- [ ] Remove MediatR package reference

### Phase 3: Resilience (Polly v8)
- [ ] Add resilience configuration
- [ ] Replace `IAsyncPolicy` with `ResiliencePipeline`
- [ ] Replace `AddPolicyHandler` with `AddResilienceHandler`
- [ ] Remove custom CreateResiliencePolicy method
- [ ] Verify circuit breaker observability

### Phase 4: Caching
- [ ] Add Redis caching configuration
- [ ] Add WASM caching configuration (for PWA)
- [ ] Replace IMemoryCache with ICacheService
- [ ] Remove QueryCachingBehavior
- [ ] Test cache compression

### Phase 5: Exceptions
- [ ] Add global exception handler
- [ ] Replace custom exceptions with Mystira.Shared types
- [ ] Update API responses to use ProblemDetails

### Phase 6: Specification Pattern
- [ ] Create specification classes for all query operations
- [ ] Update repository interfaces to use Ardalis.Specification
- [ ] Update service layer to use specifications
- [ ] Mark old query handlers as obsolete

### Phase 7: Polyglot (Optional)
- [ ] Add database target annotations to entities
- [ ] Configure polyglot persistence
- [ ] Test dual-database operations

### Phase 8: Distributed Locking
- [ ] Add distributed locking configuration
- [ ] Identify concurrent operations requiring locks
- [ ] Implement lock patterns for game sessions

### Phase 9: Source Generators (Optional)
- [ ] Annotate repository interfaces
- [ ] Annotate options classes for validation
- [ ] Register generated validators

### Post-Migration
- [ ] Run all unit tests
- [ ] Run integration tests
- [ ] Run specification tests
- [ ] Verify API responses unchanged
- [ ] Performance testing
- [ ] Load testing for distributed locks
- [ ] Create PR

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| .NET 8 → .NET 9 | Runtime upgrade required | Test thoroughly in staging |
| MediatR → Wolverine | Handler signatures change | Gradual migration with both running |
| Polly v7 → v8 | Policy API changes | Use new ResiliencePipeline API |
| IMemoryCache → Redis | Requires Redis in production | Use Memory provider for dev |
| Custom Specifications → Ardalis | Query patterns change | Migrate specification by specification |
| Exception types | API error responses may change | Use GlobalExceptionHandler for consistency |

---

## Rollback Plan

If migration causes issues:

1. Keep MediatR handlers intact during migration
2. Feature flag Wolverine handlers: `UseWolverineHandlers: true/false`
3. Feature flag Polly v8: `UsePollyV8: true/false`
4. Redis cache can fall back to memory cache
5. Global exception handler can be disabled
6. Specification pattern can coexist with old queries

---

## Performance Considerations

1. **Polly v8**: Lower memory allocation than v7
2. **Ardalis.Specification**: Compiled queries for better performance
3. **Source Generators**: Zero runtime reflection overhead
4. **Distributed Locking**: Use short lock durations to avoid contention
5. **WASM Caching**: IndexedDB is async; batch operations when possible

---

## Related Documentation

- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0017: Resource Group Organization](../architecture/adr/0017-resource-group-organization-strategy.md)
- [Ardalis.Specification 9.3.1 Guide](../architecture/specifications/ardalis-specification-migration.md)
- [Mystira.Shared Migration Guide](../guides/mystira-shared-migration.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
