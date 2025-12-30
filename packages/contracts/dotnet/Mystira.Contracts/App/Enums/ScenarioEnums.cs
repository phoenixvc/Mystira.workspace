// Re-export domain enums for backward compatibility
// Consumers can use either Mystira.Contracts.App.Enums or Mystira.Domain.Enums

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
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.DifficultyLevel directly.
/// </remarks>
public enum DifficultyLevel
{
    /// <inheritdoc cref="DomainDifficultyLevel.Easy"/>
    Easy = 0,

    /// <inheritdoc cref="DomainDifficultyLevel.Medium"/>
    Medium = 1,

    /// <inheritdoc cref="DomainDifficultyLevel.Hard"/>
    Hard = 2,

    /// <inheritdoc cref="DomainDifficultyLevel.Expert"/>
    Expert = 3
}

/// <summary>
/// Represents the expected duration of a game session.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.SessionLength directly.
/// </remarks>
public enum SessionLength
{
    /// <inheritdoc cref="DomainSessionLength.Quick"/>
    Quick = 0,

    /// <inheritdoc cref="DomainSessionLength.Short"/>
    Short = 1,

    /// <inheritdoc cref="DomainSessionLength.Medium"/>
    Medium = 2,

    /// <inheritdoc cref="DomainSessionLength.Long"/>
    Long = 3,

    /// <inheritdoc cref="DomainSessionLength.Extended"/>
    Extended = 4
}

/// <summary>
/// Represents the current state of a scenario game.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.ScenarioGameState directly.
/// </remarks>
public enum ScenarioGameState
{
    /// <inheritdoc cref="DomainScenarioGameState.NotStarted"/>
    NotStarted = 0,

    /// <inheritdoc cref="DomainScenarioGameState.InProgress"/>
    InProgress = 1,

    /// <inheritdoc cref="DomainScenarioGameState.Paused"/>
    Paused = 2,

    /// <inheritdoc cref="DomainScenarioGameState.Completed"/>
    Completed = 3,

    /// <inheritdoc cref="DomainScenarioGameState.Abandoned"/>
    Abandoned = 4
}

/// <summary>
/// Represents the publication status of a scenario.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.PublicationStatus directly.
/// </remarks>
public enum PublicationStatus
{
    /// <inheritdoc cref="DomainPublicationStatus.Draft"/>
    Draft = 0,

    /// <inheritdoc cref="DomainPublicationStatus.UnderReview"/>
    UnderReview = 1,

    /// <inheritdoc cref="DomainPublicationStatus.Published"/>
    Published = 2,

    /// <inheritdoc cref="DomainPublicationStatus.Archived"/>
    Archived = 3,

    /// <inheritdoc cref="DomainPublicationStatus.Rejected"/>
    Rejected = 4
}

/// <summary>
/// Represents the type of a scene in a scenario.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.SceneType directly.
/// </remarks>
public enum SceneType
{
    /// <inheritdoc cref="DomainSceneType.Standard"/>
    Standard = 0,

    /// <inheritdoc cref="DomainSceneType.Intro"/>
    Intro = 1,

    /// <inheritdoc cref="DomainSceneType.Decision"/>
    Decision = 2,

    /// <inheritdoc cref="DomainSceneType.Action"/>
    Action = 3,

    /// <inheritdoc cref="DomainSceneType.Dialogue"/>
    Dialogue = 4,

    /// <inheritdoc cref="DomainSceneType.Puzzle"/>
    Puzzle = 5,

    /// <inheritdoc cref="DomainSceneType.Exploration"/>
    Exploration = 6,

    /// <inheritdoc cref="DomainSceneType.EchoReveal"/>
    EchoReveal = 7,

    /// <inheritdoc cref="DomainSceneType.Ending"/>
    Ending = 8,

    /// <inheritdoc cref="DomainSceneType.Checkpoint"/>
    Checkpoint = 9,

    /// <inheritdoc cref="DomainSceneType.Cutscene"/>
    Cutscene = 10,

    /// <inheritdoc cref="DomainSceneType.MiniGame"/>
    MiniGame = 11,

    /// <inheritdoc cref="DomainSceneType.Tutorial"/>
    Tutorial = 12
}

/// <summary>
/// Represents the status of a game session.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.SessionStatus directly.
/// </remarks>
public enum SessionStatus
{
    /// <inheritdoc cref="DomainSessionStatus.Creating"/>
    Creating = 0,

    /// <inheritdoc cref="DomainSessionStatus.Pending"/>
    Pending = 1,

    /// <inheritdoc cref="DomainSessionStatus.Active"/>
    Active = 2,

    /// <inheritdoc cref="DomainSessionStatus.Paused"/>
    Paused = 3,

    /// <inheritdoc cref="DomainSessionStatus.Completed"/>
    Completed = 4,

    /// <inheritdoc cref="DomainSessionStatus.Abandoned"/>
    Abandoned = 5,

    /// <inheritdoc cref="DomainSessionStatus.Failed"/>
    Failed = 6,

    /// <inheritdoc cref="DomainSessionStatus.Expired"/>
    Expired = 7
}

/// <summary>
/// Represents the type of achievement earned in a session.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.AchievementType directly.
/// </remarks>
public enum AchievementType
{
    /// <inheritdoc cref="DomainAchievementType.ScenarioCompletion"/>
    ScenarioCompletion = 0,

    /// <inheritdoc cref="DomainAchievementType.StoryChoice"/>
    StoryChoice = 1,

    /// <inheritdoc cref="DomainAchievementType.CompassMilestone"/>
    CompassMilestone = 2,

    /// <inheritdoc cref="DomainAchievementType.Discovery"/>
    Discovery = 3,

    /// <inheritdoc cref="DomainAchievementType.SpeedRun"/>
    SpeedRun = 4,

    /// <inheritdoc cref="DomainAchievementType.Perfect"/>
    Perfect = 5,

    /// <inheritdoc cref="DomainAchievementType.Cooperative"/>
    Cooperative = 6,

    /// <inheritdoc cref="DomainAchievementType.Special"/>
    Special = 7,

    /// <inheritdoc cref="DomainAchievementType.EchoDiscovery"/>
    EchoDiscovery = 8,

    /// <inheritdoc cref="DomainAchievementType.CharacterBond"/>
    CharacterBond = 9
}

/// <summary>
/// Extension methods for enum conversions between Contracts and Domain.
/// </summary>
public static class ScenarioEnumExtensions
{
    /// <summary>
    /// Converts Contracts DifficultyLevel to Domain DifficultyLevel.
    /// </summary>
    public static DomainDifficultyLevel ToDomain(this DifficultyLevel value)
        => (DomainDifficultyLevel)(int)value;

    /// <summary>
    /// Converts Domain DifficultyLevel to Contracts DifficultyLevel.
    /// </summary>
    public static DifficultyLevel ToContracts(this DomainDifficultyLevel value)
        => (DifficultyLevel)(int)value;

    /// <summary>
    /// Converts Contracts SessionLength to Domain SessionLength.
    /// </summary>
    public static DomainSessionLength ToDomain(this SessionLength value)
        => (DomainSessionLength)(int)value;

    /// <summary>
    /// Converts Domain SessionLength to Contracts SessionLength.
    /// </summary>
    public static SessionLength ToContracts(this DomainSessionLength value)
        => (SessionLength)(int)value;

    /// <summary>
    /// Converts Contracts ScenarioGameState to Domain ScenarioGameState.
    /// </summary>
    public static DomainScenarioGameState ToDomain(this ScenarioGameState value)
        => (DomainScenarioGameState)(int)value;

    /// <summary>
    /// Converts Domain ScenarioGameState to Contracts ScenarioGameState.
    /// </summary>
    public static ScenarioGameState ToContracts(this DomainScenarioGameState value)
        => (ScenarioGameState)(int)value;

    /// <summary>
    /// Converts Contracts SessionStatus to Domain SessionStatus.
    /// </summary>
    public static DomainSessionStatus ToDomain(this SessionStatus value)
        => (DomainSessionStatus)(int)value;

    /// <summary>
    /// Converts Domain SessionStatus to Contracts SessionStatus.
    /// </summary>
    public static SessionStatus ToContracts(this DomainSessionStatus value)
        => (SessionStatus)(int)value;
}
