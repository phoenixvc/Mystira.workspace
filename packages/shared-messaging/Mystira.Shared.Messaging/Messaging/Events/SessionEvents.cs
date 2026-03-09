namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a game session is started.
/// </summary>
public sealed record SessionStarted : IntegrationEventBase
{
    /// <summary>
    /// The unique session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The account ID of the player.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The scenario being played.
    /// </summary>
    public required string ScenarioId { get; init; }
}

/// <summary>
/// Published when a game session is completed.
/// </summary>
public sealed record SessionCompleted : IntegrationEventBase
{
    /// <summary>
    /// The unique session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The account ID of the player.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The scenario that was played.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Duration of the session in seconds.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// Final outcome or ending reached.
    /// </summary>
    public string? Outcome { get; init; }
}

/// <summary>
/// Published when a session is abandoned (user quit before completion).
/// </summary>
public sealed record SessionAbandoned : IntegrationEventBase
{
    /// <summary>
    /// The unique session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The account ID of the player.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Duration before abandonment in seconds.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// Last known progress point.
    /// </summary>
    public string? LastProgressPoint { get; init; }
}
