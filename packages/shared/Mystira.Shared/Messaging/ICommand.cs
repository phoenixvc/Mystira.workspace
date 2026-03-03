namespace Mystira.Shared.Messaging;

// =============================================================================
// CQRS INTERFACES MOVED
// =============================================================================
// ICommand, ICommand<TResponse>, and IQuery<TResponse> have moved to:
//   Mystira.Shared.CQRS
//
// Update your imports:
//   using Mystira.Shared.CQRS;
//
// This file now only contains event-related interfaces.
// =============================================================================

/// <summary>
/// Marker interface for domain events.
/// Events are published to all subscribers.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}

/// <summary>
/// Marker interface for integration events.
/// Integration events are published to external services via message broker.
/// </summary>
public interface IIntegrationEvent : IDomainEvent
{
    /// <summary>
    /// Unique event ID for idempotency.
    /// </summary>
    Guid EventId { get; }
}

/// <summary>
/// Base class for domain events with common properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Base class for integration events with common properties.
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
