namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user makes a choice in a story.
/// </summary>
public sealed record ChoiceMade : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The chapter or node ID where the choice was made.
    /// </summary>
    public required string NodeId { get; init; }

    /// <summary>
    /// The choice ID that was selected.
    /// </summary>
    public required string ChoiceId { get; init; }

    /// <summary>
    /// Time taken to make the choice in seconds.
    /// </summary>
    public int? DecisionTimeSeconds { get; init; }
}

/// <summary>
/// Published when a user starts a chapter.
/// </summary>
public sealed record ChapterStarted : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The chapter ID.
    /// </summary>
    public required string ChapterId { get; init; }

    /// <summary>
    /// Chapter number (1-indexed).
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Whether this is the first time playing this chapter.
    /// </summary>
    public bool IsFirstPlay { get; init; } = true;
}

/// <summary>
/// Published when a user completes a chapter.
/// </summary>
public sealed record ChapterCompleted : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The chapter ID.
    /// </summary>
    public required string ChapterId { get; init; }

    /// <summary>
    /// Chapter number (1-indexed).
    /// </summary>
    public required int ChapterNumber { get; init; }

    /// <summary>
    /// Time spent in the chapter in seconds.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// Number of choices made in the chapter.
    /// </summary>
    public int ChoicesMade { get; init; }

    /// <summary>
    /// The ending/outcome of the chapter if applicable.
    /// </summary>
    public string? Outcome { get; init; }
}

/// <summary>
/// Published when a user reaches a checkpoint/save point.
/// </summary>
public sealed record CheckpointReached : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The checkpoint ID.
    /// </summary>
    public required string CheckpointId { get; init; }

    /// <summary>
    /// The checkpoint name for display.
    /// </summary>
    public string? CheckpointName { get; init; }

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public int ProgressPercent { get; init; }
}

/// <summary>
/// Published when user takes a specific story branch.
/// </summary>
public sealed record StoryBranchTaken : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The branch ID taken.
    /// </summary>
    public required string BranchId { get; init; }

    /// <summary>
    /// The branch name for analytics.
    /// </summary>
    public string? BranchName { get; init; }

    /// <summary>
    /// The source node ID.
    /// </summary>
    public required string FromNodeId { get; init; }

    /// <summary>
    /// The destination node ID.
    /// </summary>
    public required string ToNodeId { get; init; }
}

/// <summary>
/// Published when user explicitly saves their progress.
/// </summary>
public sealed record ProgressSaved : IntegrationEventBase
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The save slot ID.
    /// </summary>
    public required string SaveSlotId { get; init; }

    /// <summary>
    /// Current node/position ID.
    /// </summary>
    public required string CurrentNodeId { get; init; }

    /// <summary>
    /// Overall progress percentage.
    /// </summary>
    public int ProgressPercent { get; init; }

    /// <summary>
    /// Whether this overwrote an existing save.
    /// </summary>
    public bool IsOverwrite { get; init; }
}
