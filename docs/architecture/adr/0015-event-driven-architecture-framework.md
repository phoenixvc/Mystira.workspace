# ADR-0015: Event-Driven Architecture Framework Selection

## Status

**Proposed** - 2025-12-22

## Context

The Mystira platform requires event-driven communication for:

1. **Cross-Service Communication**: App API ↔ Admin API ↔ Story-Generator ↔ Analytics
2. **Domain Events**: AccountCreated, ProfileUpdated, ScenarioPublished, etc.
3. **Data Synchronization**: Dual-write verification, cache invalidation
4. **Async Processing**: Background jobs, batch operations
5. **Integration Events**: External webhooks, notifications

### Current State

| Pattern | Current Implementation | Issue |
|---------|----------------------|-------|
| In-Process Messaging | MediatR | Good for commands/queries, not for events |
| Cross-Service | HTTP REST calls | Tight coupling, no retry |
| Background Jobs | None | Missing |
| Event Bus | None | Missing |

### Integration Points

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Event Communication Requirements                   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────────────┐ │
│  │  App API     │     │  Admin API   │     │  Story-Generator    │ │
│  │              │     │              │     │                      │ │
│  │ • Account    │     │ • Scenario   │     │ • Generate          │ │
│  │   Created    │────►│   Published  │────►│   Completed         │ │
│  │ • Session    │     │ • Content    │     │ • Training          │ │
│  │   Completed  │     │   Updated    │     │   Started           │ │
│  └──────────────┘     └──────────────┘     └──────────────────────┘ │
│         │                    │                        │             │
│         │                    │                        │             │
│         ▼                    ▼                        ▼             │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Event Bus / Message Broker                 │   │
│  │  (Azure Service Bus / RabbitMQ / In-Memory)                  │   │
│  └──────────────────────────────────────────────────────────────┘   │
│         │                    │                        │             │
│         ▼                    ▼                        ▼             │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────────────┐ │
│  │  Analytics   │     │  Publisher   │     │  Notification       │ │
│  │  Service     │     │  Service     │     │  Service            │ │
│  └──────────────┘     └──────────────┘     └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

## Decision Drivers

| Driver | Weight | Description |
|--------|--------|-------------|
| **Azure Integration** | 25% | Works well with Azure Service Bus |
| **Developer Experience** | 20% | Easy to understand and use |
| **Cost** | 20% | Licensing and operational costs |
| **Reliability** | 15% | At-least-once delivery, outbox pattern |
| **Performance** | 10% | Low latency, high throughput |
| **Community/Support** | 10% | Active development, documentation |

---

## Options Analysis

### Option 1: MassTransit (Current Industry Standard)

**Description**: [MassTransit](https://masstransit.io/) is a mature, feature-rich distributed application framework supporting Azure Service Bus, RabbitMQ, Amazon SQS.

```
┌─────────────────────────────────────────────────────────────────┐
│                    MassTransit Architecture                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Application Layer                        │ │
│  │  IPublishEndpoint, ISendEndpointProvider                   │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    MassTransit Core                         │ │
│  │  • Message serialization                                   │ │
│  │  • Retry policies                                          │ │
│  │  • Saga orchestration                                      │ │
│  │  • Transactional outbox (EF Core)                         │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                 Transport Abstraction                       │ │
│  │  Azure Service Bus | RabbitMQ | Amazon SQS | In-Memory    │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Key Features**:
- Saga state machines for complex workflows
- Transactional outbox with EF Core
- Competing consumer support
- Message scheduling
- Monitoring with OpenTelemetry

**Implementation**:
```csharp
// Configure MassTransit with Azure Service Bus
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AccountCreatedConsumer>();
    x.AddConsumer<ScenarioPublishedConsumer>();

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(configuration["ServiceBus:ConnectionString"]);
        cfg.ConfigureEndpoints(context);
    });

    // Transactional outbox
    x.AddEntityFrameworkOutbox<MystiraAppDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });
});

// Publish event
public class AccountService
{
    private readonly IPublishEndpoint _publisher;

    public async Task CreateAccountAsync(CreateAccountCommand cmd, CancellationToken ct)
    {
        var account = // ... create account

        await _publisher.Publish(new AccountCreatedEvent
        {
            AccountId = account.Id,
            Email = account.Email,
            OccurredAt = DateTimeOffset.UtcNow
        }, ct);
    }
}
```

**Licensing Changes (2025-2026)**:
- Q3 2025: MassTransit v9 prerelease (commercial)
- Q1 2026: Official v9 release under commercial license
- Post-2026: End of v8 maintenance
- Pricing: $400/month (SMB) to $1,200/month (Enterprise)

**Pros**:
- Battle-tested, production-ready
- Excellent Azure Service Bus integration
- Rich feature set (sagas, outbox, scheduling)
- Strong community and documentation
- MediatR-like in-process support

**Cons**:
- Going commercial in 2026
- Large dependency
- Learning curve for advanced features
- May be overkill for simpler use cases

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Azure Integration | 5 | 1.25 |
| Developer Experience | 4 | 0.80 |
| Cost | 2 | 0.40 |
| Reliability | 5 | 0.75 |
| Performance | 4 | 0.40 |
| Community/Support | 5 | 0.50 |
| **Total** | | **4.10** |

---

### Option 2: Wolverine

**Description**: [Wolverine](https://wolverine.netlify.app/) is a next-generation .NET messaging framework supporting both in-process and distributed messaging.

```
┌─────────────────────────────────────────────────────────────────┐
│                     Wolverine Architecture                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Message Handlers                         │ │
│  │  public static async Task Handle(AccountCreated evt)       │ │
│  │  (Convention-based, minimal boilerplate)                   │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Wolverine Runtime                        │ │
│  │  • Compiled handlers (fast startup)                        │ │
│  │  • Built-in transactional outbox                          │ │
│  │  • Durable inbox                                           │ │
│  │  • Saga support                                            │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│              ┌───────────────┼───────────────┐                   │
│              ▼               ▼               ▼                   │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ │
│  │  Azure Service   │ │    RabbitMQ      │ │    In-Memory     │ │
│  │  Bus             │ │                  │ │    (Local Dev)   │ │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Key Features**:
- Both in-process AND distributed messaging (replaces MediatR)
- Compiled message handlers (minimal reflection)
- Built-in transactional outbox (even for in-memory)
- Convention-based handler discovery
- Interoperability with MassTransit and NServiceBus messages

**Implementation**:
```csharp
// Configure Wolverine
builder.Host.UseWolverine(opts =>
{
    // Azure Service Bus
    opts.UseAzureServiceBus(configuration["ServiceBus:ConnectionString"])
        .AutoProvision();

    // Transactional outbox with EF Core
    opts.UseEntityFrameworkCoreTransactions();

    // Configure durability
    opts.Durability.Mode = DurabilityMode.Balanced;
});

// Handler (convention-based, no interfaces)
public static class AccountCreatedHandler
{
    public static async Task HandleAsync(
        AccountCreatedEvent @event,
        IAccountRepository repo,
        ILogger<AccountCreatedHandler> logger)
    {
        logger.LogInformation("Processing account created: {Id}", @event.AccountId);
        // Handle event...
    }
}

// Publish event
public class AccountService
{
    private readonly IMessageBus _bus;

    public async Task CreateAccountAsync(CreateAccountCommand cmd, CancellationToken ct)
    {
        var account = // ... create account

        await _bus.PublishAsync(new AccountCreatedEvent
        {
            AccountId = account.Id
        });
    }
}
```

**Pros**:
- Free and open source (MIT license)
- Replaces both MediatR and distributed messaging
- Faster startup (compiled handlers)
- Modern, clean API
- Growing interoperability

**Cons**:
- Younger framework (less battle-tested)
- Smaller community than MassTransit
- Documentation still evolving
- Fewer transport options

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Azure Integration | 4 | 1.00 |
| Developer Experience | 5 | 1.00 |
| Cost | 5 | 1.00 |
| Reliability | 4 | 0.60 |
| Performance | 5 | 0.50 |
| Community/Support | 3 | 0.30 |
| **Total** | | **4.40** |

---

### Option 3: CAP (DotNetCore.CAP)

**Description**: [CAP](https://github.com/dotnetcore/CAP) is an EventBus with local persistent message functionality for microservices.

```
┌─────────────────────────────────────────────────────────────────┐
│                        CAP Architecture                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    CAP Publisher                            │ │
│  │  ICapPublisher.PublishAsync(name, data)                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Local Message Store (Outbox)                   │ │
│  │  SQL Server | PostgreSQL | MongoDB | In-Memory             │ │
│  │  (Guarantees at-least-once delivery)                       │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                 Message Broker Transport                    │ │
│  │  RabbitMQ | Kafka | Azure Service Bus | Amazon SQS        │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    CAP Subscriber                           │ │
│  │  [CapSubscribe("account.created")]                         │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Key Features**:
- Built-in outbox pattern
- Dashboard for monitoring
- Retry with exponential backoff
- Message idempotency support
- Multi-database support

**Implementation**:
```csharp
// Configure CAP
builder.Services.AddCap(x =>
{
    // Database storage
    x.UsePostgreSql(configuration.GetConnectionString("PostgreSQL"));

    // Message transport
    x.UseAzureServiceBus(opt =>
    {
        opt.ConnectionString = configuration["ServiceBus:ConnectionString"];
        opt.TopicPath = "mystira-events";
    });

    x.UseDashboard();
});

// Publish
public class AccountService
{
    private readonly ICapPublisher _publisher;

    public async Task CreateAccountAsync(CreateAccountCommand cmd, CancellationToken ct)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var account = // ... create account
        await _dbContext.SaveChangesAsync(ct);

        await _publisher.PublishAsync("account.created", new AccountCreatedEvent
        {
            AccountId = account.Id
        }, cancellationToken: ct);

        await transaction.CommitAsync(ct);
    }
}

// Subscribe
public class AccountCreatedSubscriber : ICapSubscribe
{
    [CapSubscribe("account.created")]
    public async Task HandleAsync(AccountCreatedEvent @event)
    {
        // Handle event
    }
}
```

**Pros**:
- Simple, focused on event bus
- Built-in outbox pattern
- Good dashboard
- Free and open source
- Lightweight

**Cons**:
- No saga support
- Less feature-rich than MassTransit
- Smaller Western community
- String-based event names

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Azure Integration | 4 | 1.00 |
| Developer Experience | 4 | 0.80 |
| Cost | 5 | 1.00 |
| Reliability | 4 | 0.60 |
| Performance | 4 | 0.40 |
| Community/Support | 3 | 0.30 |
| **Total** | | **4.10** |

---

### Option 4: Azure Service Bus SDK + Custom Abstraction

**Description**: Use Azure Service Bus SDK directly with a thin custom abstraction layer.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Direct Azure Service Bus                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                Custom Event Bus Abstraction                 │ │
│  │  IEventBus.PublishAsync<T>(T @event)                       │ │
│  │  IEventHandler<T>.HandleAsync(T @event)                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │           Azure.Messaging.ServiceBus SDK                    │ │
│  │  ServiceBusClient, ServiceBusSender, ServiceBusProcessor  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                 Custom Outbox Implementation                │ │
│  │  (EF Core + Background Service)                            │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:
- Full control
- No framework overhead
- Direct Azure integration
- No licensing concerns

**Cons**:
- Significant development effort
- Need to implement outbox, retry, etc.
- Missing battle-tested patterns
- Maintenance burden

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Azure Integration | 5 | 1.25 |
| Developer Experience | 2 | 0.40 |
| Cost | 5 | 1.00 |
| Reliability | 2 | 0.30 |
| Performance | 5 | 0.50 |
| Community/Support | 2 | 0.20 |
| **Total** | | **3.65** |

---

### Option 5: Hybrid - Wolverine + MediatR (Transitional)

**Description**: Use Wolverine for distributed messaging while keeping MediatR for in-process commands/queries during transition.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Hybrid Messaging Strategy                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────────────────┐ ┌─────────────────────────────┐│
│  │   In-Process (Commands)     │ │  Distributed (Events)       ││
│  │   ┌─────────────────────┐   │ │  ┌─────────────────────┐   ││
│  │   │      MediatR        │   │ │  │     Wolverine       │   ││
│  │   │  IRequest/Handler   │   │ │  │  IMessageBus        │   ││
│  │   └─────────────────────┘   │ │  └─────────────────────┘   ││
│  └─────────────────────────────┘ └─────────────────────────────┘│
│                                                                  │
│  Phase 2: Migrate MediatR handlers to Wolverine                 │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │               Wolverine (Unified)                         │   │
│  │  • In-process commands/queries                           │   │
│  │  • Distributed events via Azure Service Bus              │   │
│  │  • Transactional outbox                                  │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:
- Gradual migration path
- Keep MediatR patterns initially
- Wolverine for new distributed events
- Eventually unify on Wolverine

**Cons**:
- Two frameworks during transition
- Complexity during migration
- Developer confusion

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Azure Integration | 4 | 1.00 |
| Developer Experience | 3 | 0.60 |
| Cost | 5 | 1.00 |
| Reliability | 4 | 0.60 |
| Performance | 4 | 0.40 |
| Community/Support | 3 | 0.30 |
| **Total** | | **3.90** |

---

## Decision Matrix Summary

| Option | Azure | DevEx | Cost | Reliable | Perf | Community | **Total** |
|--------|-------|-------|------|----------|------|-----------|-----------|
| 1. MassTransit | 5 | 4 | 2 | 5 | 4 | 5 | **4.10** |
| 2. Wolverine | 4 | 5 | 5 | 4 | 5 | 3 | **4.40** |
| 3. CAP | 4 | 4 | 5 | 4 | 4 | 3 | **4.10** |
| 4. Azure SDK Direct | 5 | 2 | 5 | 2 | 5 | 2 | **3.65** |
| 5. Wolverine + MediatR | 4 | 3 | 5 | 4 | 4 | 3 | **3.90** |

---

## Recommendation

### **Option 2: Wolverine**

Adopt Wolverine as the unified messaging framework for both in-process and distributed messaging:

**Rationale**:
1. **Free Forever**: No licensing concerns (MIT license)
2. **Replaces MediatR**: Simplifies stack, one framework for all messaging
3. **Modern Design**: Clean API, compiled handlers, excellent performance
4. **Azure Support**: Full Azure Service Bus integration
5. **Outbox Built-In**: Even for in-memory, ensuring consistency
6. **Interoperability**: Can receive messages from MassTransit/NServiceBus systems

### Migration Strategy

#### Phase 1: Add Wolverine for New Events (Month 1)
```csharp
// Add alongside existing MediatR
builder.Host.UseWolverine(opts =>
{
    // Start with in-memory for testing
    opts.UseAzureServiceBus(connectionString)
        .AutoProvision();

    opts.UseEntityFrameworkCoreTransactions();
});
```

Events to start with:
- `AccountCreatedEvent`
- `ScenarioPublishedEvent`
- `SessionCompletedEvent`

#### Phase 2: Migrate MediatR Commands (Month 2-3)
```csharp
// Before (MediatR)
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, AccountDto>

// After (Wolverine - simpler, convention-based)
public static class CreateAccountHandler
{
    public static async Task<AccountDto> HandleAsync(
        CreateAccountCommand command,
        IAccountRepository repo)
    {
        // Same logic, less boilerplate
    }
}
```

#### Phase 3: Remove MediatR Dependency (Month 3-4)
- All handlers migrated to Wolverine
- Remove MediatR packages
- Single unified messaging framework

### Architecture After Migration

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Wolverine Unified Architecture                    │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                        App API                                  │ │
│  │  IMessageBus.SendAsync(command)   // In-process               │ │
│  │  IMessageBus.PublishAsync(event)  // Distributed              │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                              │                                       │
│                              ▼                                       │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                   Wolverine Runtime                             │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │ │
│  │  │ Command      │  │ Event        │  │ Transactional        │  │ │
│  │  │ Handlers     │  │ Publishers   │  │ Outbox (EF Core)     │  │ │
│  │  └──────────────┘  └──────────────┘  └──────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                              │                                       │
│                              ▼                                       │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │                   Azure Service Bus                             │ │
│  │  Topics: mystira.events, mystira.commands                      │ │
│  │  Subscriptions per service                                     │ │
│  └────────────────────────────────────────────────────────────────┘ │
│                              │                                       │
│              ┌───────────────┼───────────────┐                       │
│              ▼               ▼               ▼                       │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────────┐ │
│  │  Admin API       │ │  Analytics       │ │  Notifications       │ │
│  │  (Subscriber)    │ │  (Subscriber)    │ │  (Subscriber)        │ │
│  └──────────────────┘ └──────────────────┘ └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Consequences

### Positive
- Free, no licensing costs ever
- Unified messaging (replaces MediatR)
- Excellent developer experience
- Built-in outbox for consistency
- Future-proof (active development)

### Negative
- Smaller community than MassTransit
- Less third-party tooling
- Documentation still evolving
- Need to train team on new patterns

### Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Framework maturity | Start with non-critical events |
| Team familiarity | Wolverine syntax similar to MediatR |
| Azure Service Bus issues | Wolverine supports multiple transports |
| Performance concerns | Compiled handlers are faster than reflection |

---

## References

- [Wolverine Documentation](https://wolverine.netlify.app/)
- [Messaging Made Simple - Visual Studio Magazine](https://visualstudiomagazine.com/articles/2025/08/11/messaging-made-simple-choosing-the-right-framework-for-net.aspx)
- [SE Radio: Chris Patterson on MassTransit](https://se-radio.net/2025/02/se-radio-654-chris-patterson-on-masstransit-and-event-driven-systems/)
- [Event-Driven Architecture with MassTransit](https://medium.com/@serhatalftkn/event-driven-architecture-with-net-8-rabbitmq-and-masstransit-using-clean-architecture-559eba2915ec)
- [Wolverine in ASP.NET Core](https://www.nikolatech.net/blogs/wolverine-aspnetcore-messaging)
- [MassTransit Commercial Licensing](https://antondevtips.com/blog/masstransit-rabbitmq-and-azure-service-bus-is-it-worth-a-commercial-license)
