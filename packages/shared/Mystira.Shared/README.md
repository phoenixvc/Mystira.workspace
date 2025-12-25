# Mystira.Shared

Shared infrastructure for Mystira platform services.

## Overview

This package provides cross-cutting concerns for all Mystira .NET services:

- **Authentication**: JWT token generation, validation, and middleware
- **Authorization**: Role-based and permission-based access control
- **Resilience**: Polly v8 resilience pipelines with retry, circuit breaker, and timeout
- **Exceptions**: Standardized error handling, Result<T> pattern, ProblemDetails
- **Caching**: Distributed caching with Redis support
- **Messaging**: Wolverine integration for unified in-process and distributed messaging
- **Locking**: Redis-based distributed locking for concurrency control
- **Repository Pattern**: Generic repository + Ardalis.Specification (recommended)
- **Observability**: Circuit breaker events with OpenTelemetry metrics
- **Middleware**: Telemetry, logging, and request/response handling
- **Extensions**: Dependency injection helpers for easy integration

## Installation

```bash
dotnet add package Mystira.Shared
```

Or via NuGet Package Manager:
```
Install-Package Mystira.Shared
```

## Usage

### Configure Services

```csharp
// Program.cs
using Mystira.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Mystira authentication (Microsoft Entra ID)
builder.Services.AddMystiraAuthentication(builder.Configuration);

// Add Mystira authorization policies
builder.Services.AddMystiraAuthorization();

// Add telemetry and logging
builder.Services.AddMystiraTelemetry(builder.Configuration);

var app = builder.Build();

// Use authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseMystiraTelemetry();
app.UseMystiraExceptionHandling();
```

### Resilience (HTTP Clients)

```csharp
using Mystira.Shared.Resilience;

// Add resilient HTTP client with retry, circuit breaker, and timeout
builder.Services.AddResilientHttpClient<IScenarioApiClient, ScenarioApiClient>(
    "ScenarioApi",
    client => client.BaseAddress = new Uri("https://api.mystira.app"));

// For long-running operations (e.g., LLM calls)
builder.Services.AddLongRunningHttpClient<ILlmApiClient, LlmApiClient>(
    "LlmApi");
```

### Caching

```csharp
using Mystira.Shared.Extensions;

// Add distributed caching (Redis or in-memory fallback)
builder.Services.AddMystiraCaching(builder.Configuration);

// Use ICacheService
public class ScenarioService
{
    private readonly ICacheService _cache;

    public async Task<Scenario> GetScenarioAsync(string id)
    {
        return await _cache.GetOrCreateAsync(
            $"scenario:{id}",
            async ct => await _repository.GetByIdAsync(id, ct));
    }
}
```

### Messaging (Wolverine)

```csharp
using Mystira.Shared.Extensions;

// Add Wolverine messaging
builder.AddMystiraMessaging();

// Handler (convention-based, no interfaces needed)
public static class CreateAccountHandler
{
    public static async Task<AccountDto> HandleAsync(
        CreateAccountCommand command,
        IAccountRepository repo)
    {
        // Handler implementation
    }
}
```

### Repository Pattern

```csharp
using Mystira.Shared.Data.Repositories;

// Create a repository implementation
public class AccountRepository : RepositoryBase<Account>, IAccountRepository
{
    public AccountRepository(MyDbContext context) : base(context) { }
}

// Use typed key repository
public class OrderRepository : RepositoryBase<Order, Guid>, IOrderRepository
{
    public OrderRepository(MyDbContext context) : base(context) { }
}

// Query with specifications (Ardalis.Specification)
public class ActiveAccountsSpec : Specification<Account>
{
    public ActiveAccountsSpec()
    {
        Query.Where(a => a.IsActive)
             .OrderBy(a => a.CreatedAt);
    }
}

// Use in service
public class AccountService
{
    private readonly IRepository<Account> _repo;

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
    {
        return await _repo.ListAsync(new ActiveAccountsSpec());
    }

    // Stream large datasets without loading all into memory
    public async IAsyncEnumerable<Account> StreamAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var account in _repo.StreamAllAsync(ct))
        {
            yield return account;
        }
    }
}
```

### Unit of Work Pattern

```csharp
using Mystira.Shared.Data.Repositories;

public class OrderService
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<OrderItem> _items;
    private readonly IUnitOfWork _unitOfWork;

    public async Task CreateOrderAsync(CreateOrderDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = new Order { CustomerId = dto.CustomerId };
            await _orders.AddAsync(order);

            foreach (var item in dto.Items)
            {
                await _items.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId
                });
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### Options Validation

```csharp
using Mystira.Shared.Extensions;

// Add options with validation (fails fast on startup if invalid)
builder.Services.AddMystiraCaching(configuration)
    .AddMystiraOptionsValidation(); // Validates CacheOptions

builder.Services.AddMystiraResilience(configuration)
    .AddMystiraOptionsValidation(); // Validates ResilienceOptions

// Custom options validation
public class MyOptionsValidator : IValidateOptions<MyOptions>
{
    public ValidateOptionsResult Validate(string? name, MyOptions options)
    {
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}
```

### Configuration

```json
// appsettings.json
{
  "Mystira": {
    "Authentication": {
      "Authority": "https://login.microsoftonline.com/{tenant-id}",
      "ClientId": "{client-id}",
      "Audience": "api://{client-id}",
      "ValidateIssuer": true,
      "ValidateAudience": true
    },
    "Telemetry": {
      "ServiceName": "mystira-api",
      "EnableTracing": true,
      "EnableMetrics": true
    }
  }
}
```

### Protect Controllers

```csharp
using Microsoft.AspNetCore.Authorization;
using Mystira.Shared.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScenariosController : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ScenariosRead)]
    public async Task<IActionResult> GetScenarios() { ... }

    [HttpPost]
    [RequirePermission(Permissions.ScenariosWrite)]
    public async Task<IActionResult> CreateScenario() { ... }

    [HttpDelete("{id}")]
    [RequireRole(Roles.Admin)]
    public async Task<IActionResult> DeleteScenario(Guid id) { ... }
}
```

## Migration from Mystira.App.Shared

This package replaces `Mystira.App.Shared`. To migrate:

1. Update package reference:
   ```xml
   <!-- Before -->
   <PackageReference Include="Mystira.App.Shared" Version="x.x.x" />

   <!-- After -->
   <PackageReference Include="Mystira.Shared" Version="0.1.0" />
   ```

2. Update using statements:
   ```csharp
   // Before
   using Mystira.App.Shared.Authentication;
   using Mystira.App.Shared.Authorization;

   // After
   using Mystira.Shared.Authentication;
   using Mystira.Shared.Authorization;
   ```

3. Update service registration (method names unchanged):
   ```csharp
   // Same API, just new namespace
   services.AddMystiraAuthentication(configuration);
   services.AddMystiraAuthorization();
   ```

### Distributed Locking

```csharp
using Mystira.Shared.Locking;

// Setup
builder.Services.AddMystiraCaching(configuration); // Redis required
builder.Services.AddMystiraDistributedLocking(configuration);

// Usage
public class PaymentService
{
    private readonly IDistributedLockService _lockService;

    public async Task ProcessPaymentAsync(Guid paymentId, CancellationToken ct)
    {
        // Execute with automatic lock management
        await _lockService.ExecuteWithLockAsync(
            $"payment:{paymentId}",
            async token => await DoPaymentProcessing(paymentId, token),
            expiry: TimeSpan.FromSeconds(30),
            wait: TimeSpan.FromSeconds(10),
            ct);
    }
}
```

### Polly v8 Resilience Pipelines

```csharp
using Mystira.Shared.Extensions;
using Mystira.Shared.Resilience;

// Standard resilience with Polly v8 pipelines
builder.Services.AddResilientHttpClientV8<IMyApiClient, MyApiClient>(
    "MyApi",
    client => client.BaseAddress = new Uri("https://api.example.com"));

// Long-running operations (e.g., LLM calls)
builder.Services.AddLongRunningHttpClientV8<ILlmClient, LlmClient>(
    "LlmApi");

// Direct pipeline usage for non-HTTP operations
var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>(
    "DatabaseQuery",
    new ResilienceOptions { MaxRetries = 3 });

var result = await pipeline.ExecuteAsync(async ct => await db.QueryAsync(ct));
```

### Circuit Breaker Events

```csharp
using Mystira.Shared.Resilience;

// Register metrics
builder.Services.AddSingleton<CircuitBreakerMetrics>();

// Subscribe to events
public class CircuitMonitor
{
    public CircuitMonitor(IObservableCircuitBreaker circuit)
    {
        circuit.StateChanged += (s, e) =>
        {
            if (e.NewState == CircuitState.Open)
                _alertService.SendAlert($"Circuit {e.CircuitName} opened!");
        };
    }
}
```

## Package Contents

```
Mystira.Shared/
├── Authentication/
│   ├── JwtOptions.cs              # JWT configuration options
│   └── JwtService.cs              # Token generation and validation
├── Authorization/
│   ├── Permissions.cs             # Permission constants
│   ├── RequirePermissionAttribute.cs
│   └── PermissionHandler.cs       # Authorization handler
├── Resilience/
│   ├── ResilienceOptions.cs       # Retry/circuit breaker config
│   ├── PolicyFactory.cs           # Legacy Polly v7 policies
│   ├── ResiliencePipelineFactory.cs  # Polly v8 pipelines (NEW)
│   ├── ResiliencePipelineExtensions.cs  # v8 DI helpers (NEW)
│   └── CircuitBreakerEvents.cs    # Observable circuit breaker (NEW)
├── Exceptions/
│   ├── ErrorResponse.cs           # Standard error responses
│   ├── Result.cs                  # Result<T> pattern
│   ├── MystiraException.cs        # Base exception types
│   └── GlobalExceptionHandler.cs  # IExceptionHandler
├── Caching/
│   ├── CacheOptions.cs            # Cache configuration
│   ├── ICacheService.cs           # Cache abstraction
│   └── DistributedCacheService.cs # Redis/memory implementation
├── Messaging/                     # Wolverine
│   ├── MessagingOptions.cs        # Messaging configuration
│   └── ICommand.cs                # Message marker interfaces
├── Locking/                       # NEW
│   ├── IDistributedLock.cs        # Lock interfaces
│   ├── RedisDistributedLockService.cs  # Redis implementation
│   └── DistributedLockExtensions.cs    # DI helpers
├── Data/
│   └── Repositories/
│       ├── IRepository.cs         # Repository + UnitOfWork interfaces
│       └── RepositoryBase.cs      # EF Core implementation with streaming
├── Middleware/
│   └── TelemetryMiddleware.cs     # OpenTelemetry integration
└── Extensions/
    ├── AuthenticationExtensions.cs
    ├── AuthorizationExtensions.cs
    ├── CachingExtensions.cs
    ├── ExceptionExtensions.cs
    ├── HealthCheckExtensions.cs
    ├── MessagingExtensions.cs
    ├── OptionsValidationExtensions.cs
    ├── ResilienceExtensions.cs
    └── TelemetryExtensions.cs
```

## Related Documentation

- [Migration Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/mystira-shared-migration.md) - Detailed migration and feature adoption guide
- [ADR-0020: Package Consolidation Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md)
- [Authentication & Authorization Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/authentication-authorization-guide.md)

## License

MIT
