namespace Mystira.Domain.Enums;

/// <summary>
/// Represents the difficulty level of a scenario.
/// </summary>
public enum DifficultyLevel
{
    /// <summary>
    /// Easy difficulty for beginners.
    /// </summary>
    Easy = 0,

    /// <summary>
    /// Medium difficulty for intermediate players.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Hard difficulty for experienced players.
    /// </summary>
    Hard = 2,

    /// <summary>
    /// Expert difficulty for advanced players.
    /// </summary>
    Expert = 3
}

/// <summary>
/// Represents the expected duration of a game session.
/// </summary>
public enum SessionLength
{
    /// <summary>
    /// A quick session, typically 15-30 minutes.
    /// </summary>
    Quick = 0,

    /// <summary>
    /// A short session, typically 30-60 minutes.
    /// </summary>
    Short = 1,

    /// <summary>
    /// A medium session, typically 1-2 hours.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// A long session, typically 2-4 hours.
    /// </summary>
    Long = 3,

    /// <summary>
    /// An extended session, typically 4+ hours.
    /// </summary>
    Extended = 4
}

/// <summary>
/// Represents the current state of a scenario game.
/// </summary>
public enum ScenarioGameState
{
    /// <summary>
    /// The game has not been started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// The game is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// The game has been paused.
    /// </summary>
    Paused = 2,

    /// <summary>
    /// The game has been completed.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// The game was abandoned before completion.
    /// </summary>
    Abandoned = 4
}

/// <summary>
/// Represents the publication status of a scenario.
/// </summary>
public enum PublicationStatus
{
    /// <summary>
    /// The scenario is in draft state.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The scenario is under review.
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// The scenario is published and available.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The scenario has been archived.
    /// </summary>
    Archived = 3,

    /// <summary>
    /// The scenario has been rejected.
    /// </summary>
    Rejected = 4
}
