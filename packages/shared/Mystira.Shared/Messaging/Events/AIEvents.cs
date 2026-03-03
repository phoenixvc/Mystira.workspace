namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when story generation is requested.
/// </summary>
public sealed record StoryGenerationRequested : IntegrationEventBase
{
    /// <summary>
    /// The request ID for tracking.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The scenario being generated for.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The requesting user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The prompt or context for generation.
    /// </summary>
    public string? Prompt { get; init; }
}

/// <summary>
/// Published when story generation completes successfully.
/// </summary>
public sealed record StoryGenerationCompleted : IntegrationEventBase
{
    /// <summary>
    /// The request ID for tracking.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Time taken in milliseconds.
    /// </summary>
    public required int DurationMs { get; init; }

    /// <summary>
    /// Token count used.
    /// </summary>
    public int? TokensUsed { get; init; }

    /// <summary>
    /// Model used for generation.
    /// </summary>
    public string? Model { get; init; }
}

/// <summary>
/// Published when story generation fails.
/// </summary>
public sealed record StoryGenerationFailed : IntegrationEventBase
{
    /// <summary>
    /// The request ID for tracking.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Error code.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Error message.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// Whether the request can be retried.
    /// </summary>
    public bool IsRetryable { get; init; }
}
