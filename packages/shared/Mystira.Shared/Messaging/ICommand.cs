namespace Mystira.Shared.Messaging;

/// <summary>
/// Marker interface for commands (actions that change state).
/// Wolverine uses convention-based handler discovery, so this is optional
/// but useful for documentation and filtering.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that return a result.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public interface ICommand<TResult> : ICommand
{
}

/// <summary>
/// Marker interface for queries (read-only operations).
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IQuery<TResult>
{
}

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
