// Re-export domain enums for backward compatibility
global using DomainDifficultyLevel = Mystira.Domain.Enums.DifficultyLevel;
global using DomainSessionLength = Mystira.Domain.Enums.SessionLength;
global using DomainScenarioGameState = Mystira.Domain.Enums.ScenarioGameState;
global using DomainPublicationStatus = Mystira.Domain.Enums.PublicationStatus;
global using DomainSceneType = Mystira.Domain.Enums.SceneType;
global using DomainSessionStatus = Mystira.Domain.Enums.SessionStatus;
global using DomainAchievementType = Mystira.Domain.Enums.AchievementType;

namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Represents the difficulty level of a scenario.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.DifficultyLevel for backward compatibility.
/// </remarks>
public enum DifficultyLevel
{
    /// <summary>Easy difficulty for beginners.</summary>
    Easy = 0,
    /// <summary>Medium difficulty for intermediate players.</summary>
    Medium = 1,
    /// <summary>Hard difficulty for experienced players.</summary>
    Hard = 2,
    /// <summary>Expert difficulty for advanced players.</summary>
    Expert = 3
}

/// <summary>
/// Represents the expected duration of a game session.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.SessionLength for backward compatibility.
/// </remarks>
public enum SessionLength
{
    /// <summary>A quick session, typically 15-30 minutes.</summary>
    Quick = 0,
    /// <summary>A short session, typically 30-60 minutes.</summary>
    Short = 1,
    /// <summary>A medium session, typically 1-2 hours.</summary>
    Medium = 2,
    /// <summary>A long session, typically 2-4 hours.</summary>
    Long = 3,
    /// <summary>An extended session, typically 4+ hours.</summary>
    Extended = 4
}

/// <summary>
/// Represents the current state of a scenario game.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.ScenarioGameState for backward compatibility.
/// </remarks>
public enum ScenarioGameState
{
    /// <summary>The game has not been started yet.</summary>
    NotStarted = 0,
    /// <summary>The game is currently in progress.</summary>
    InProgress = 1,
    /// <summary>The game has been paused.</summary>
    Paused = 2,
    /// <summary>The game has been completed.</summary>
    Completed = 3,
    /// <summary>The game was abandoned before completion.</summary>
    Abandoned = 4
}

/// <summary>
/// Represents the publication status of a scenario.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.PublicationStatus for backward compatibility.
/// </remarks>
public enum PublicationStatus
{
    /// <summary>The scenario is in draft state.</summary>
    Draft = 0,
    /// <summary>The scenario is under review.</summary>
    UnderReview = 1,
    /// <summary>The scenario is published and available.</summary>
    Published = 2,
    /// <summary>The scenario has been archived.</summary>
    Archived = 3,
    /// <summary>The scenario has been rejected.</summary>
    Rejected = 4
}

/// <summary>
/// Represents the type of a scene in a scenario.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.SceneType for backward compatibility.
/// </remarks>
public enum SceneType
{
    /// <summary>Standard narrative scene with choices.</summary>
    Standard = 0,
    /// <summary>Introduction/opening scene.</summary>
    Intro = 1,
    /// <summary>Decision point with multiple branches.</summary>
    Decision = 2,
    /// <summary>Action or challenge scene.</summary>
    Action = 3,
    /// <summary>Dialogue-focused scene.</summary>
    Dialogue = 4,
    /// <summary>Puzzle or problem-solving scene.</summary>
    Puzzle = 5,
    /// <summary>Exploration or discovery scene.</summary>
    Exploration = 6,
    /// <summary>Scene revealing an echo (past event).</summary>
    EchoReveal = 7,
    /// <summary>Ending/conclusion scene.</summary>
    Ending = 8,
    /// <summary>Checkpoint or save point scene.</summary>
    Checkpoint = 9,
    /// <summary>Cutscene or cinematic.</summary>
    Cutscene = 10,
    /// <summary>Mini-game scene.</summary>
    MiniGame = 11,
    /// <summary>Tutorial scene.</summary>
    Tutorial = 12
}

/// <summary>
/// Represents the status of a game session.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.SessionStatus for backward compatibility.
/// </remarks>
public enum SessionStatus
{
    /// <summary>Session is being created/initialized.</summary>
    Creating = 0,
    /// <summary>Session is ready to start, waiting for players.</summary>
    Pending = 1,
    /// <summary>Session is currently active and in progress.</summary>
    Active = 2,
    /// <summary>Session has been paused.</summary>
    Paused = 3,
    /// <summary>Session completed successfully.</summary>
    Completed = 4,
    /// <summary>Session was abandoned before completion.</summary>
    Abandoned = 5,
    /// <summary>Session failed due to an error.</summary>
    Failed = 6,
    /// <summary>Session expired due to inactivity.</summary>
    Expired = 7
}

/// <summary>
/// Represents the type of achievement earned in a session.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.AchievementType for backward compatibility.
/// </remarks>
public enum AchievementType
{
    /// <summary>Achievement for completing a scenario.</summary>
    ScenarioCompletion = 0,
    /// <summary>Achievement for making specific story choices.</summary>
    StoryChoice = 1,
    /// <summary>Achievement for reaching a compass milestone.</summary>
    CompassMilestone = 2,
    /// <summary>Achievement for discovering hidden content.</summary>
    Discovery = 3,
    /// <summary>Achievement for speed/time-based goals.</summary>
    SpeedRun = 4,
    /// <summary>Achievement for perfect completion.</summary>
    Perfect = 5,
    /// <summary>Achievement for helping other players.</summary>
    Cooperative = 6,
    /// <summary>Special event or seasonal achievement.</summary>
    Special = 7,
    /// <summary>Achievement for echo-related discoveries.</summary>
    EchoDiscovery = 8,
    /// <summary>Achievement for character relationship milestones.</summary>
    CharacterBond = 9
}

/// <summary>
/// Extension methods for enum conversions between Contracts and Domain.
/// </summary>
public static class ScenarioEnumExtensions
{
    /// <summary>Converts to Domain DifficultyLevel.</summary>
    public static DomainDifficultyLevel ToDomain(this DifficultyLevel value) => (DomainDifficultyLevel)(int)value;
    /// <summary>Converts to Contracts DifficultyLevel.</summary>
    public static DifficultyLevel ToContracts(this DomainDifficultyLevel value) => (DifficultyLevel)(int)value;

    /// <summary>Converts to Domain SessionLength.</summary>
    public static DomainSessionLength ToDomain(this SessionLength value) => (DomainSessionLength)(int)value;
    /// <summary>Converts to Contracts SessionLength.</summary>
    public static SessionLength ToContracts(this DomainSessionLength value) => (SessionLength)(int)value;

    /// <summary>Converts to Domain ScenarioGameState.</summary>
    public static DomainScenarioGameState ToDomain(this ScenarioGameState value) => (DomainScenarioGameState)(int)value;
    /// <summary>Converts to Contracts ScenarioGameState.</summary>
    public static ScenarioGameState ToContracts(this DomainScenarioGameState value) => (ScenarioGameState)(int)value;

    /// <summary>Converts to Domain SessionStatus.</summary>
    public static DomainSessionStatus ToDomain(this SessionStatus value) => (DomainSessionStatus)(int)value;
    /// <summary>Converts to Contracts SessionStatus.</summary>
    public static SessionStatus ToContracts(this DomainSessionStatus value) => (SessionStatus)(int)value;
}
