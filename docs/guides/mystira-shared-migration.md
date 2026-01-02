# Mystira.Shared Migration Guide

This guide covers migrating to `Mystira.Shared` and adopting its new features.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Migration from Mystira.App.Shared](#migration-from-mystiraappshared)
- [Adopting New Features](#adopting-new-features)
  - [Wave 1: Source Generators](#wave-1-source-generators)
  - [Wave 2: Polly v8 Resilience Pipelines](#wave-2-polly-v8-resilience-pipelines)
  - [Wave 3: Distributed Locking](#wave-3-distributed-locking)
  - [Wave 4: Circuit Breaker Events](#wave-4-circuit-breaker-events)
- [Configuration Reference](#configuration-reference)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

- .NET 9.0 SDK
- Redis (for caching and distributed locking)
- Update NuGet package reference:

```xml
<PackageReference Include="Mystira.Shared" Version="0.4.*" />
```

---

## Important: Polyglot Deprecation Notice

The polyglot persistence interfaces in `Mystira.Shared.Data.Polyglot` are **deprecated** as of January 2026.

### What's Deprecated

| Deprecated Interface | Location |
|---------------------|----------|
| `IPolyglotRepository<T>` | `Mystira.Shared.Data.Polyglot` |
| `DatabaseTargetAttribute` | `Mystira.Shared.Data.Polyglot` |
| `DatabaseTarget` enum | `Mystira.Shared.Data.Polyglot` |

### Migration Path

Use the new consolidated packages instead:

| Use Instead | Package |
|-------------|---------|
| `Mystira.Application.Ports.Data.IPolyglotRepository<T>` | `Mystira.Application` |
| `Mystira.Infrastructure.Data.Polyglot.PolyglotRepository<T>` | `Mystira.Infrastructure.Data` |

### Code Migration

```csharp
// Before (deprecated)
using Mystira.Shared.Data.Polyglot;

[DatabaseTarget(DatabaseTarget.CosmosDb)]
public class Scenario : AuditableEntity { }

// After
using Mystira.Application.Ports.Data;

[DatabaseTarget(DatabaseTarget.CosmosDb)]
public class Scenario : AuditableEntity { }
```

For full infrastructure migration details, see the [Infrastructure Migration Guide](../migrations/mystira-infrastructure-migration.md).

---

## Migration from Mystira.App.Shared

### Step 1: Update Package Reference

```xml
<!-- Before -->
<PackageReference Include="Mystira.App.Shared" Version="x.x.x" />

<!-- After -->
<PackageReference Include="Mystira.Shared" Version="0.2.0" />
```

### Step 2: Update Using Statements

```csharp
// Before
using Mystira.App.Shared.Authentication;
using Mystira.App.Shared.Authorization;
using Mystira.App.Shared.Resilience;

// After
using Mystira.Shared.Authentication;
using Mystira.Shared.Authorization;
using Mystira.Shared.Resilience;
using Mystira.Shared.Extensions; // For DI extensions
```

### Step 3: Update Service Registration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Authentication & Authorization (unchanged API)
builder.Services.AddMystiraAuthentication(builder.Configuration);
builder.Services.AddMystiraAuthorization();

// Caching with Redis
builder.Services.AddMystiraCaching(builder.Configuration);

// Resilience (now uses Polly v8 pipelines)
builder.Services.AddResilientHttpClientV8<IMyApiClient, MyApiClient>(
    "MyApi",
    client => client.BaseAddress = new Uri("https://api.example.com"));

// Distributed Locking (new)
builder.Services.AddMystiraDistributedLocking(builder.Configuration);

// Messaging (Wolverine)
builder.Host.AddMystiraMessaging();
```

---

## Adopting New Features

### Wave 1: Source Generators

Source generators automatically create repository implementations and options validators at compile time.

#### Repository Generation

1. Add the `[GenerateRepository]` attribute to your interface:

```csharp
using Mystira.Shared.Data;

[GenerateRepository]
public interface IAccountRepository : IRepository<Account>
{
    // Custom methods (implement in partial class)
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);
}
```

2. The generator creates `AccountRepositoryGenerated.g.cs`:

```csharp
// Auto-generated - do not edit
public partial class AccountRepositoryGenerated : RepositoryBase<Account>, IAccountRepository
{
    public AccountRepositoryGenerated(DbContext context) : base(context) { }
}
```

3. Extend with a partial class for custom methods:

```csharp
public partial class AccountRepositoryGenerated
{
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.Email == email, ct);
    }
}
```

#### Options Validation Generation

1. Add `[GenerateValidator]` and validation attributes:

```csharp
using Mystira.Shared.Validation;

[GenerateValidator]
public class MyServiceOptions
{
    [ValidatePositive]
    public int MaxRetries { get; set; } = 3;

    [ValidateRange(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    [ValidateNotEmpty]
    public string Endpoint { get; set; } = "";

    [ValidateUrl]
    public string? WebhookUrl { get; set; }
}
```

2. Register the generated validator:

```csharp
builder.Services.AddMyServiceOptionsValidation(); // Generated extension method
```

### Wave 2: Polly v8 Resilience Pipelines

The new resilience system uses Polly v8's `ResiliencePipeline` instead of legacy policies.

#### Breaking Changes

| Before (Polly v7) | After (Polly v8) |
|-------------------|------------------|
| `IAsyncPolicy<T>` | `ResiliencePipeline<T>` |
| `Policy.WrapAsync()` | `ResiliencePipelineBuilder` |
| `AddPolicyHandler()` | `AddResilienceHandler()` / `AddStandardResilienceHandler()` |
| `PolicyFactory.CreateStandardHttpPolicy()` | `ResiliencePipelineFactory.CreateStandardHttpPipeline()` |

#### Migration Example

```csharp
// Before (Polly v7)
builder.Services.AddHttpClient<IMyApiClient, MyApiClient>()
    .AddPolicyHandler(PolicyFactory.CreateStandardHttpPolicy("MyApi"));

// After (Polly v8 - Option 1: Standard resilience)
builder.Services.AddResilientHttpClientV8<IMyApiClient, MyApiClient>(
    "MyApi",
    client => client.BaseAddress = new Uri("https://api.example.com"));

// After (Polly v8 - Option 2: Custom pipeline)
builder.Services.AddHttpClient<IMyApiClient, MyApiClient>()
    .AddCustomResiliencePipeline("MyApi", new ResilienceOptions
    {
        MaxRetries = 5,
        TimeoutSeconds = 60
    });
```

#### Using ResiliencePipelineFactory Directly

```csharp
// For non-HTTP operations
var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>(
    "DatabaseOperation",
    new ResilienceOptions { MaxRetries = 3 });

var result = await pipeline.ExecuteAsync(async ct =>
{
    return await _database.QueryAsync(query, ct);
});
```

### Wave 3: Distributed Locking

Prevent concurrent modifications with Redis-based distributed locks.

#### Setup

```csharp
// Program.cs
builder.Services.AddMystiraCaching(builder.Configuration); // Requires Redis
builder.Services.AddMystiraDistributedLocking(builder.Configuration);
```

```json
// appsettings.json
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

#### Usage Examples

```csharp
public class OrderService
{
    private readonly IDistributedLockService _lockService;

    public OrderService(IDistributedLockService lockService)
    {
        _lockService = lockService;
    }

    // Option 1: ExecuteWithLock (recommended)
    public async Task ProcessOrderAsync(Guid orderId, CancellationToken ct)
    {
        await _lockService.ExecuteWithLockAsync(
            $"order:{orderId}",
            async token =>
            {
                // Only one instance can process this order at a time
                await DoProcessingAsync(orderId, token);
            },
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(10),
            ct);
    }

    // Option 2: Manual lock management
    public async Task UpdateInventoryAsync(string sku, int quantity, CancellationToken ct)
    {
        await using var lockHandle = await _lockService.AcquireAsync(
            $"inventory:{sku}",
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(5),
            cancellationToken: ct);

        try
        {
            // Lock acquired - safe to modify inventory
            await _inventory.UpdateAsync(sku, quantity, ct);

            // Extend lock if operation takes longer than expected
            await lockHandle.ExtendAsync(TimeSpan.FromSeconds(30), ct);

            await _inventory.SaveChangesAsync(ct);
        }
        catch
        {
            // Lock will be released automatically via IAsyncDisposable
            throw;
        }
    }

    // Option 3: Try-acquire pattern
    public async Task<bool> TryProcessPaymentAsync(Guid paymentId, CancellationToken ct)
    {
        var lockHandle = await _lockService.TryAcquireAsync(
            $"payment:{paymentId}",
            TimeSpan.FromMinutes(5),
            ct);

        if (lockHandle == null)
        {
            // Another instance is already processing this payment
            return false;
        }

        await using (lockHandle)
        {
            await ProcessPaymentInternalAsync(paymentId, ct);
            return true;
        }
    }
}
```

### Wave 4: Circuit Breaker Events

Monitor circuit breaker state changes with observability integration.

#### Setup

```csharp
// Program.cs
builder.Services.AddSingleton<CircuitBreakerMetrics>();

// For each circuit breaker you want to observe
builder.Services.AddSingleton<IObservableCircuitBreaker>(sp =>
    new CircuitBreakerEventPublisher(
        "MyApiCircuit",
        sp.GetRequiredService<CircuitBreakerMetrics>(),
        sp.GetRequiredService<ILogger<CircuitBreakerEventPublisher>>()));
```

#### Subscribing to Events

```csharp
public class CircuitBreakerMonitor
{
    private readonly IObservableCircuitBreaker _circuitBreaker;
    private readonly ILogger<CircuitBreakerMonitor> _logger;

    public CircuitBreakerMonitor(
        IObservableCircuitBreaker circuitBreaker,
        ILogger<CircuitBreakerMonitor> logger)
    {
        _circuitBreaker = circuitBreaker;
        _logger = logger;

        // Subscribe to state changes
        _circuitBreaker.StateChanged += OnCircuitStateChanged;
        _circuitBreaker.RequestRejected += OnRequestRejected;
    }

    private void OnCircuitStateChanged(object? sender, CircuitBreakerStateChangedEventArgs e)
    {
        _logger.LogWarning(
            "Circuit {CircuitName} changed from {Previous} to {New}",
            e.CircuitName,
            e.PreviousState,
            e.NewState);

        if (e.NewState == CircuitState.Open)
        {
            // Alert operations team
            // _alertService.SendAlertAsync($"Circuit {e.CircuitName} is open");
        }
    }

    private void OnRequestRejected(object? sender, CircuitBreakerRejectionEventArgs e)
    {
        _logger.LogDebug(
            "Request rejected by circuit {CircuitName}, opens in {TimeRemaining}",
            e.CircuitName,
            e.TimeUntilHalfOpen);
    }
}
```

#### Metrics (OpenTelemetry)

The following metrics are automatically exported:

| Metric | Type | Description |
|--------|------|-------------|
| `mystira.circuit_breaker.state_changes` | Counter | Number of state transitions |
| `mystira.circuit_breaker.rejections` | Counter | Requests rejected due to open circuit |
| `mystira.circuit_breaker.successes` | Counter | Successful requests |
| `mystira.circuit_breaker.failures` | Counter | Failed requests |
| `mystira.circuit_breaker.state` | Gauge | Current state (0=Closed, 1=Open, 2=HalfOpen) |

---

## Configuration Reference

### appsettings.json

```json
{
  "Resilience": {
    "MaxRetries": 3,
    "BaseDelaySeconds": 2,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "TimeoutSeconds": 30,
    "LongRunningTimeoutSeconds": 300,
    "EnableDetailedLogging": true
  },
  "Cache": {
    "DefaultExpirationMinutes": 60,
    "ConnectionString": "localhost:6379",
    "InstanceName": "Mystira:"
  },
  "DistributedLock": {
    "DefaultExpirySeconds": 30,
    "DefaultWaitSeconds": 10,
    "RetryIntervalMs": 100,
    "KeyPrefix": "lock:"
  }
}
```

---

## Troubleshooting

### Source Generators Not Working

1. Ensure the generator project is referenced correctly:
```xml
<ProjectReference Include="...\Mystira.Shared.Generators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

2. Check for build errors in the generator project
3. Clean and rebuild the solution

### Polly v8 Migration Issues

**Error: `IAsyncPolicy` not found**
- Update using statements to use `ResiliencePipeline<T>`
- Replace `PolicyFactory` with `ResiliencePipelineFactory`

**Error: `AddPolicyHandler` not found**
- Replace with `AddStandardResilienceHandler()` or `AddResilienceHandler()`

### Distributed Lock Timeouts
**DistributedLockException: Could not acquire lock**
#### DistributedLockException: Could not acquire lock

- Increase `DefaultWaitSeconds` in configuration
- Check Redis connectivity
- Verify no deadlocks in lock acquisition order
### Circuit Breaker Events Not Firing

- Ensure `CircuitBreakerMetrics` is registered as singleton
- Verify event subscriptions are set up before first request
- Check that `EnableDetailedLogging` is true

---

## Related Documentation

- [ADR-0020: Package Consolidation Strategy](../architecture/adr/0020-package-consolidation-strategy.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
- [Polly v8 Documentation](https://github.com/App-vNext/Polly)
