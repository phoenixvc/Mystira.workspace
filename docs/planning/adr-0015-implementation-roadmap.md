# ADR-0015: Event-Driven Architecture Implementation Roadmap

## Overview

This roadmap details the implementation of Wolverine as the unified messaging framework for the Mystira platform, replacing MediatR for in-process messaging and adding distributed event capabilities via Azure Service Bus.

**ADR Reference**: [ADR-0015: Event-Driven Architecture Framework Selection](../architecture/adr/0015-event-driven-architecture-framework.md)

---

## Phase Summary

| Phase   | Duration   | Focus                            | Risk Level |
| ------- | ---------- | -------------------------------- | ---------- |
| Phase 1 | Week 1-2   | Infrastructure & Wolverine Setup | Low        |
| Phase 2 | Week 3-4   | New Events with Wolverine        | Low        |
| Phase 3 | Week 5-8   | MediatR Handler Migration        | Medium     |
| Phase 4 | Week 9-10  | Cross-Service Events             | Medium     |
| Phase 5 | Week 11-12 | MediatR Removal & Cleanup        | Low        |

---

## Phase 1: Infrastructure & Wolverine Setup (Week 1-2)

### 1.1 Azure Service Bus Provisioning

**Terraform Resources**:

```hcl
# modules/servicebus/main.tf

resource "azurerm_servicebus_namespace" "mystira" {
  name                = "sb-mystira-${var.environment}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "Standard"  # Premium for production (partitioning, VNET)

  tags = var.tags
}

# Topics for domain events
resource "azurerm_servicebus_topic" "events" {
  name         = "mystira-events"
  namespace_id = azurerm_servicebus_namespace.mystira.id

  enable_partitioning   = true
  max_size_in_megabytes = 5120
}

# Per-service subscriptions
resource "azurerm_servicebus_subscription" "app_api" {
  name               = "app-api"
  topic_id           = azurerm_servicebus_topic.events.id
  max_delivery_count = 10
}

resource "azurerm_servicebus_subscription" "admin_api" {
  name               = "admin-api"
  topic_id           = azurerm_servicebus_topic.events.id
  max_delivery_count = 10
}

resource "azurerm_servicebus_subscription" "analytics" {
  name               = "analytics"
  topic_id           = azurerm_servicebus_topic.events.id
  max_delivery_count = 10
}
```

### 1.2 Add Wolverine Packages

**Mystira.App.csproj**:

```xml
<PackageReference Include="WolverineFx" Version="3.*" />
<PackageReference Include="WolverineFx.AzureServiceBus" Version="3.*" />
<PackageReference Include="WolverineFx.EntityFrameworkCore" Version="3.*" />
```

**Mystira.Admin.Api.csproj**:

```xml
<PackageReference Include="WolverineFx" Version="3.*" />
<PackageReference Include="WolverineFx.AzureServiceBus" Version="3.*" />
<PackageReference Include="WolverineFx.EntityFrameworkCore" Version="3.*" />
```

### 1.3 Wolverine Configuration

**Common/Wolverine/WolverineConfiguration.cs**:

```csharp
namespace Mystira.Common.Wolverine;

public static class WolverineConfiguration
{
    public static IHostBuilder UseWolverineWithAzureServiceBus(
        this IHostBuilder builder,
        string connectionString,
        string serviceName)
    {
        return builder.UseWolverine(opts =>
        {
            // Azure Service Bus transport
            opts.UseAzureServiceBus(connectionString)
                .AutoProvision()
                .AutoPurgeOnStartup(false);  // true only in dev

            // Configure topic publishing
            opts.PublishAllMessages()
                .ToAzureServiceBusTopic("mystira-events")
                .UseDurableOutbox();

            // Configure subscription listening
            opts.ListenToAzureServiceBusSubscription(
                    "mystira-events",
                    serviceName)
                .UseDurableInbox();

            // Durability settings
            opts.Durability.Mode = DurabilityMode.Balanced;
            opts.Durability.ScheduledJobFirstExecution = TimeSpan.FromSeconds(5);
            opts.Durability.ScheduledJobPollingTime = TimeSpan.FromSeconds(30);

            // Handler discovery
            opts.Discovery.IncludeAssembly(typeof(WolverineConfiguration).Assembly);

            // Policies
            opts.Policies.UseDurableLocalQueues();
            opts.Policies.AutoApplyTransactions();
        });
    }
}
```

### 1.4 Outbox Table Migration

**Migrations/AddWolverineOutbox.cs**:

```csharp
public partial class AddWolverineOutbox : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Wolverine requires specific tables for durability
        migrationBuilder.Sql(@"
            CREATE SCHEMA IF NOT EXISTS wolverine;

            CREATE TABLE wolverine.incoming_envelopes (
                id uuid PRIMARY KEY,
                status varchar(25) NOT NULL,
                owner_id int NOT NULL,
                execution_time timestamptz,
                attempts int NOT NULL DEFAULT 0,
                body bytea NOT NULL,
                message_type varchar(500) NOT NULL,
                received_at timestamptz NOT NULL,
                keep_until timestamptz
            );

            CREATE TABLE wolverine.outgoing_envelopes (
                id uuid PRIMARY KEY,
                owner_id int NOT NULL,
                destination varchar(500) NOT NULL,
                deliver_by timestamptz,
                body bytea NOT NULL,
                message_type varchar(500) NOT NULL,
                sent_at timestamptz NOT NULL
            );

            CREATE INDEX ix_incoming_owner ON wolverine.incoming_envelopes(owner_id);
            CREATE INDEX ix_outgoing_owner ON wolverine.outgoing_envelopes(owner_id);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP SCHEMA wolverine CASCADE;");
    }
}
```

### 1.5 Deliverables Checklist

- [ ] Azure Service Bus namespace provisioned via Terraform
- [ ] Topics and subscriptions created for each service
- [ ] Wolverine packages added to all API projects
- [ ] Common Wolverine configuration created
- [ ] Outbox/Inbox tables migration added
- [ ] Connection strings configured in appsettings
- [ ] Local development uses in-memory transport

---

## Phase 2: New Events with Wolverine (Week 3-4)

### 2.1 Define Domain Events

**Mystira.Domain/Events/AccountEvents.cs**:

```csharp
namespace Mystira.Domain.Events;

/// <summary>
/// Published when a new account is created.
/// Subscribers: Analytics, Notifications
/// </summary>
public sealed record AccountCreatedEvent
{
    public required string AccountId { get; init; }
    public required string Email { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public string? ReferralCode { get; init; }
}

/// <summary>
/// Published when an account is updated.
/// Subscribers: Analytics, Cache Invalidation
/// </summary>
public sealed record AccountUpdatedEvent
{
    public required string AccountId { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
    public required IReadOnlyList<string> ChangedProperties { get; init; }
}

/// <summary>
/// Published when an account is soft-deleted.
/// Subscribers: Analytics, Cleanup Services
/// </summary>
public sealed record AccountDeletedEvent
{
    public required string AccountId { get; init; }
    public required DateTimeOffset DeletedAt { get; init; }
}
```

**Mystira.Domain/Events/SessionEvents.cs**:

```csharp
namespace Mystira.Domain.Events;

public sealed record SessionStartedEvent
{
    public required string SessionId { get; init; }
    public required string AccountId { get; init; }
    public required string ScenarioId { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
}

public sealed record SessionCompletedEvent
{
    public required string SessionId { get; init; }
    public required string AccountId { get; init; }
    public required TimeSpan Duration { get; init; }
    public required int ChoicesMade { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
}
```

**Mystira.Domain/Events/ContentEvents.cs**:

```csharp
namespace Mystira.Domain.Events;

public sealed record ScenarioPublishedEvent
{
    public required string ScenarioId { get; init; }
    public required string Title { get; init; }
    public required string PublishedBy { get; init; }
    public required DateTimeOffset PublishedAt { get; init; }
}

public sealed record ScenarioUnpublishedEvent
{
    public required string ScenarioId { get; init; }
    public required string UnpublishedBy { get; init; }
    public required DateTimeOffset UnpublishedAt { get; init; }
}
```

### 2.2 Event Publishers

**Mystira.App/Services/AccountService.cs**:

```csharp
public class AccountService(
    IAccountRepository accountRepository,
    IMessageBus messageBus,
    TimeProvider timeProvider)
{
    public async Task<Account> CreateAccountAsync(
        CreateAccountCommand command,
        CancellationToken ct)
    {
        var account = new Account
        {
            Id = Ulid.NewUlid().ToString(),
            Email = command.Email,
            // ... other properties
        };

        await accountRepository.AddAsync(account, ct);

        // Publish event - Wolverine handles outbox automatically
        await messageBus.PublishAsync(new AccountCreatedEvent
        {
            AccountId = account.Id,
            Email = account.Email,
            CreatedAt = timeProvider.GetUtcNow(),
            ReferralCode = command.ReferralCode
        });

        return account;
    }
}
```

### 2.3 Event Handlers

**Mystira.Analytics/Handlers/AccountEventHandlers.cs**:

```csharp
namespace Mystira.Analytics.Handlers;

/// <summary>
/// Wolverine handlers are discovered by convention.
/// Static class with Handle/HandleAsync methods.
/// </summary>
public static class AccountEventHandlers
{
    public static async Task HandleAsync(
        AccountCreatedEvent @event,
        IAnalyticsService analytics,
        ILogger logger)
    {
        logger.LogInformation(
            "Recording account creation: {AccountId}",
            @event.AccountId);

        await analytics.RecordEventAsync(new AnalyticsEvent
        {
            Type = "account.created",
            EntityId = @event.AccountId,
            Timestamp = @event.CreatedAt,
            Properties = new Dictionary<string, object?>
            {
                ["hasReferral"] = @event.ReferralCode is not null
            }
        });
    }

    public static async Task HandleAsync(
        SessionCompletedEvent @event,
        IAnalyticsService analytics,
        ILogger logger)
    {
        logger.LogInformation(
            "Recording session completion: {SessionId}",
            @event.SessionId);

        await analytics.RecordEventAsync(new AnalyticsEvent
        {
            Type = "session.completed",
            EntityId = @event.SessionId,
            Timestamp = @event.CompletedAt,
            Properties = new Dictionary<string, object?>
            {
                ["accountId"] = @event.AccountId,
                ["duration"] = @event.Duration.TotalSeconds,
                ["choicesMade"] = @event.ChoicesMade
            }
        });
    }
}
```

### 2.4 Cache Invalidation Handler

**Mystira.App/Handlers/CacheInvalidationHandler.cs**:

```csharp
namespace Mystira.App.Handlers;

public static class CacheInvalidationHandler
{
    public static async Task HandleAsync(
        AccountUpdatedEvent @event,
        IDistributedCache cache,
        CacheOptions options,
        ILogger logger)
    {
        var cacheKey = $"{options.InstanceName}account:{@event.AccountId}";

        logger.LogDebug("Invalidating cache for account: {AccountId}", @event.AccountId);

        await cache.RemoveAsync(cacheKey);
    }

    public static async Task HandleAsync(
        AccountDeletedEvent @event,
        IDistributedCache cache,
        CacheOptions options,
        ILogger logger)
    {
        var cacheKey = $"{options.InstanceName}account:{@event.AccountId}";

        await cache.RemoveAsync(cacheKey);
    }
}
```

### 2.5 Deliverables Checklist

- [ ] Domain events defined as immutable records
- [ ] Events use `required` modifier for mandatory properties
- [ ] AccountCreatedEvent published from AccountService
- [ ] SessionCompletedEvent published from SessionService
- [ ] ScenarioPublishedEvent published from Admin API
- [ ] Analytics handlers created for key events
- [ ] Cache invalidation handlers created
- [ ] Integration tests for event flow

---

## Phase 3: MediatR Handler Migration (Week 5-8)

### 3.1 Migration Strategy

Migrate handlers in this order:

1. **Simple queries** (no side effects)
2. **Simple commands** (single write)
3. **Complex commands** (multiple writes, transactions)

### 3.2 Before/After Examples

**Query Handler Migration**:

```csharp
// BEFORE: MediatR
public class GetAccountByIdQuery : IRequest<AccountDto?>
{
    public string Id { get; init; } = string.Empty;
}

public class GetAccountByIdHandler(IAccountRepository repo)
    : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    public async Task<AccountDto?> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var account = await repo.GetByIdAsync(request.Id, cancellationToken);
        return account?.ToDto();
    }
}

// AFTER: Wolverine
public record GetAccountById(string Id);

public static class GetAccountByIdHandler
{
    public static async Task<AccountDto?> HandleAsync(
        GetAccountById query,
        IAccountRepository repo,
        CancellationToken ct)
    {
        var account = await repo.GetByIdAsync(query.Id, ct);
        return account?.ToDto();
    }
}
```

**Command Handler Migration**:

```csharp
// BEFORE: MediatR
public class CreateAccountCommand : IRequest<AccountDto>
{
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public class CreateAccountHandler(
    IAccountRepository repo,
    IValidator<CreateAccountCommand> validator)
    : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var account = new Account { /* ... */ };
        await repo.AddAsync(account, cancellationToken);

        return account.ToDto();
    }
}

// AFTER: Wolverine
public record CreateAccount(string Email, string DisplayName);

public static class CreateAccountHandler
{
    // Wolverine chains - validation runs first
    public static async Task<ProblemDetails?> ValidateAsync(
        CreateAccount command,
        IAccountRepository repo)
    {
        if (await repo.ExistsByEmailAsync(command.Email))
        {
            return new ProblemDetails
            {
                Status = 409,
                Title = "Email already exists"
            };
        }
        return null;  // Null means valid, continue
    }

    public static async Task<AccountDto> HandleAsync(
        CreateAccount command,
        IAccountRepository repo,
        IMessageBus bus,
        TimeProvider time)
    {
        var account = new Account
        {
            Id = Ulid.NewUlid().ToString(),
            Email = command.Email,
            DisplayName = command.DisplayName
        };

        await repo.AddAsync(account);

        await bus.PublishAsync(new AccountCreatedEvent
        {
            AccountId = account.Id,
            Email = account.Email,
            CreatedAt = time.GetUtcNow()
        });

        return account.ToDto();
    }
}
```

### 3.3 Controller Updates

**Before (MediatR)**:

```csharp
[ApiController]
[Route("api/accounts")]
public class AccountsController(ISender sender) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(string id, CancellationToken ct)
    {
        var result = await sender.Send(new GetAccountByIdQuery { Id = id }, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
```

**After (Wolverine)**:

```csharp
[ApiController]
[Route("api/accounts")]
public class AccountsController(IMessageBus bus) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(string id, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<AccountDto?>(new GetAccountById(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
```

### 3.4 Validation Migration

**FluentValidation Integration**:

```csharp
// Configure Wolverine to use FluentValidation
builder.Host.UseWolverine(opts =>
{
    opts.UseFluentValidation();
});

// Validator still works the same
public class CreateAccountValidator : AbstractValidator<CreateAccount>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

### 3.5 Migration Tracking

| Handler                  | Type    | Migrated | Tested |
| ------------------------ | ------- | -------- | ------ |
| GetAccountByIdHandler    | Query   | [ ]      | [ ]    |
| GetAccountByEmailHandler | Query   | [ ]      | [ ]    |
| ListAccountsHandler      | Query   | [ ]      | [ ]    |
| CreateAccountHandler     | Command | [ ]      | [ ]    |
| UpdateAccountHandler     | Command | [ ]      | [ ]    |
| DeleteAccountHandler     | Command | [ ]      | [ ]    |
| GetProfileHandler        | Query   | [ ]      | [ ]    |
| UpdateProfileHandler     | Command | [ ]      | [ ]    |
| StartSessionHandler      | Command | [ ]      | [ ]    |
| EndSessionHandler        | Command | [ ]      | [ ]    |
| GetScenarioHandler       | Query   | [ ]      | [ ]    |
| PublishScenarioHandler   | Command | [ ]      | [ ]    |

### 3.6 Deliverables Checklist

- [ ] All query handlers migrated to Wolverine
- [ ] All command handlers migrated to Wolverine
- [ ] FluentValidation integrated with Wolverine
- [ ] Controllers updated to use IMessageBus
- [ ] Unit tests updated for new handler signatures
- [ ] Integration tests passing
- [ ] No MediatR handlers remaining (except deprecated)

---

## Phase 4: Cross-Service Events (Week 9-10)

### 4.1 Service Topology

```
┌─────────────────────────────────────────────────────────────────┐
│                    Cross-Service Event Flow                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐                     ┌──────────────────────┐  │
│  │  App API     │                     │  Admin API           │  │
│  │              │                     │                      │  │
│  │ Publishes:   │                     │ Publishes:           │  │
│  │ • AccountCreated                   │ • ScenarioPublished  │  │
│  │ • SessionCompleted                 │ • ContentUpdated     │  │
│  │              │                     │                      │  │
│  │ Subscribes:  │                     │ Subscribes:          │  │
│  │ • ScenarioPublished                │ • AccountCreated     │  │
│  └──────────────┘                     └──────────────────────┘  │
│         │                                      │                 │
│         └──────────────┬───────────────────────┘                 │
│                        ▼                                         │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Azure Service Bus                              │ │
│  │  Topic: mystira-events                                     │ │
│  └────────────────────────────────────────────────────────────┘ │
│                        │                                         │
│         ┌──────────────┴───────────────────────┐                 │
│         ▼                                      ▼                 │
│  ┌──────────────┐                     ┌──────────────────────┐  │
│  │  Publisher   │                     │  Story Generator     │  │
│  │              │                     │                      │  │
│  │ Subscribes:  │                     │ Subscribes:          │  │
│  │ • ScenarioPublished                │ • GenerateRequested  │  │
│  │              │                     │                      │  │
│  │ Publishes:   │                     │ Publishes:           │  │
│  │ • Published  │                     │ • GenerateCompleted  │  │
│  └──────────────┘                     └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Event Contracts Package

Create shared package for event contracts:

**Mystira.Contracts/Events/IIntegrationEvent.cs**:

```csharp
namespace Mystira.Contracts.Events;

/// <summary>
/// Marker interface for events published across service boundaries.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique event ID for idempotency.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    string? CorrelationId { get; }
}
```

**Mystira.Contracts/Events/AccountEvents.cs**:

```csharp
namespace Mystira.Contracts.Events;

public sealed record AccountCreatedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; init; }

    public required string AccountId { get; init; }
    public required string Email { get; init; }
}
```

### 4.3 Subscription Filters

**Admin API - Only account events**:

```csharp
opts.ListenToAzureServiceBusSubscription("mystira-events", "admin-api")
    .AddRule("AccountEventsOnly", new SqlRuleFilter(
        "sys.Label LIKE 'Mystira.Contracts.Events.Account%'"));
```

**Publisher - Only content events**:

```csharp
opts.ListenToAzureServiceBusSubscription("mystira-events", "publisher")
    .AddRule("ContentEventsOnly", new SqlRuleFilter(
        "sys.Label LIKE 'Mystira.Contracts.Events.Scenario%'"));
```

### 4.4 Idempotency Handling

```csharp
public static class IdempotentEventHandler
{
    public static async Task<bool> HandleOnceAsync(
        IIntegrationEvent @event,
        IProcessedEventStore store,
        Func<Task> handler,
        CancellationToken ct)
    {
        if (await store.WasProcessedAsync(@event.EventId, ct))
        {
            return false;  // Already processed
        }

        await handler();
        await store.MarkProcessedAsync(@event.EventId, ct);

        return true;
    }
}
```

### 4.5 Deliverables Checklist

- [ ] Mystira.Contracts package created with event definitions
- [ ] All services reference shared contracts
- [ ] Subscription filters configured per service
- [ ] Idempotency handling implemented
- [ ] Correlation ID propagated across services
- [ ] Dead letter queue monitoring configured
- [ ] Cross-service integration tests passing

---

## Phase 5: MediatR Removal & Cleanup (Week 11-12)

### 5.1 Deprecation Verification

```csharp
// Run this analyzer to find remaining MediatR usages
dotnet list package --include-transitive | grep MediatR
```

### 5.2 Package Removal

**Remove from all .csproj files**:

```xml
<!-- REMOVE these -->
<PackageReference Include="MediatR" Version="*" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="*" />
```

### 5.3 Code Cleanup

- Remove `IRequest`, `IRequestHandler` interfaces
- Remove `ISender`, `IPublisher` (MediatR) injections
- Remove pipeline behaviors (replace with Wolverine policies)
- Update all using statements

### 5.4 Pipeline Behavior Migration

**Before (MediatR Pipeline Behavior)**:

```csharp
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}
```

**After (Wolverine Middleware)**:

```csharp
public class LoggingMiddleware : IWolverineMiddleware
{
    public async Task InvokeAsync(
        MessageContext context,
        ILogger logger,
        MessageHandler next)
    {
        logger.LogInformation("Handling {Message}", context.Envelope.MessageType);
        await next(context);
        logger.LogInformation("Handled {Message}", context.Envelope.MessageType);
    }
}

// Register
opts.Policies.AddMiddleware<LoggingMiddleware>();
```

### 5.5 Final Verification

```bash
# Ensure no MediatR references remain
grep -r "MediatR" --include="*.cs" --include="*.csproj" .

# Ensure all handlers are Wolverine-style
grep -r "IRequestHandler" --include="*.cs" .  # Should return nothing
```

### 5.6 Deliverables Checklist

- [ ] All MediatR packages removed
- [ ] All MediatR interfaces removed from code
- [ ] Pipeline behaviors migrated to Wolverine middleware
- [ ] No compilation errors
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Team trained on Wolverine patterns

---

## Monitoring & Observability

### OpenTelemetry Integration

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("Wolverine")
            .AddAspNetCoreInstrumentation()
            .AddAzureServiceBusInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("Wolverine")
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    });
```

### Key Metrics

| Metric                         | Description             | Alert Threshold |
| ------------------------------ | ----------------------- | --------------- |
| `wolverine_messages_received`  | Total messages received | N/A             |
| `wolverine_messages_succeeded` | Successfully processed  | N/A             |
| `wolverine_messages_failed`    | Failed processing       | > 10/min        |
| `wolverine_inbox_count`        | Pending inbox messages  | > 1000          |
| `wolverine_outbox_count`       | Pending outbox messages | > 500           |

---

## Risk Mitigation

| Risk                                   | Probability | Impact | Mitigation                                  |
| -------------------------------------- | ----------- | ------ | ------------------------------------------- |
| Handler migration breaks functionality | Medium      | High   | Comprehensive test coverage, phased rollout |
| Azure Service Bus connectivity issues  | Low         | High   | Retry policies, fallback to local queue     |
| Message ordering issues                | Low         | Medium | Use session IDs for ordered processing      |
| Performance regression                 | Low         | Medium | Load testing before production              |

---

## Success Criteria

- [ ] Zero MediatR dependencies in codebase
- [ ] All handlers use Wolverine patterns
- [ ] Cross-service events working via Azure Service Bus
- [ ] Message processing latency < 100ms (P95)
- [ ] Zero message loss (outbox guarantees)
- [ ] OpenTelemetry traces visible in Azure Monitor

---

## References

- [ADR-0015: Event-Driven Architecture](../architecture/adr/0015-event-driven-architecture-framework.md)
- [Wolverine Documentation](https://wolverine.netlify.app/)
- [Azure Service Bus Best Practices](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-exceptions)
- [Remaining Issues](../architecture/migrations/remaining-issues-and-opportunities.md)
