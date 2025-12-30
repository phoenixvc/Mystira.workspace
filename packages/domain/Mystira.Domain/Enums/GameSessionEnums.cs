namespace Mystira.Domain.Enums;

/// <summary>
/// Represents the status of a game session.
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Session is being created/initialized.
    /// </summary>
    Creating = 0,

    /// <summary>
    /// Session is ready to start, waiting for players.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Session is currently active and in progress.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Alias for Active - session is currently in progress.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Session has been paused.
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Session completed successfully.
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Session was abandoned before completion.
    /// </summary>
    Abandoned = 5,

    /// <summary>
    /// Session failed due to an error.
    /// </summary>
    Failed = 6,

    /// <summary>
    /// Session expired due to inactivity.
    /// </summary>
    Expired = 7
}

/// <summary>
/// Represents the type of achievement earned in a session.
/// </summary>
public enum AchievementType
{
    /// <summary>
    /// Achievement for completing a scenario.
    /// </summary>
    ScenarioCompletion = 0,

    /// <summary>
    /// Achievement for making specific story choices.
    /// </summary>
    StoryChoice = 1,

    /// <summary>
    /// Achievement for reaching a compass milestone.
    /// </summary>
    CompassMilestone = 2,

    /// <summary>
    /// Achievement for discovering hidden content.
    /// </summary>
    Discovery = 3,

    /// <summary>
    /// Achievement for speed/time-based goals.
    /// </summary>
    SpeedRun = 4,

    /// <summary>
    /// Achievement for perfect completion.
    /// </summary>
    Perfect = 5,

    /// <summary>
    /// Achievement for helping other players.
    /// </summary>
    Cooperative = 6,

    /// <summary>
    /// Special event or seasonal achievement.
    /// </summary>
    Special = 7,

    /// <summary>
    /// Achievement for echo-related discoveries.
    /// </summary>
    EchoDiscovery = 8,

    /// <summary>
    /// Achievement for character relationship milestones.
    /// </summary>
    CharacterBond = 9
}

/// <summary>
/// Represents the type of a scene in a scenario.
/// </summary>
public enum SceneType
{
    /// <summary>
    /// Standard narrative scene with choices.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Introduction/opening scene.
    /// </summary>
    Intro = 1,

    /// <summary>
    /// Decision point with multiple branches.
    /// </summary>
    Decision = 2,

    /// <summary>
    /// Action or challenge scene.
    /// </summary>
    Action = 3,

    /// <summary>
    /// Dialogue-focused scene.
    /// </summary>
    Dialogue = 4,

    /// <summary>
    /// Puzzle or problem-solving scene.
    /// </summary>
    Puzzle = 5,

    /// <summary>
    /// Exploration or discovery scene.
    /// </summary>
    Exploration = 6,

    /// <summary>
    /// Scene revealing an echo (past event).
    /// </summary>
    EchoReveal = 7,

    /// <summary>
    /// Ending/conclusion scene.
    /// </summary>
    Ending = 8,

    /// <summary>
    /// Checkpoint or save point scene.
    /// </summary>
    Checkpoint = 9,

    /// <summary>
    /// Cutscene or cinematic.
    /// </summary>
    Cutscene = 10,

    /// <summary>
    /// Mini-game scene.
    /// </summary>
    MiniGame = 11,

    /// <summary>
    /// Tutorial scene.
    /// </summary>
    Tutorial = 12,

    /// <summary>
    /// Choice scene (alias for Decision for DTO compatibility).
    /// </summary>
    Choice = 2
}

/// <summary>
/// Represents the status of a blockchain/Story Protocol transaction.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction is pending submission.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction has been submitted to the network.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Transaction is being confirmed.
    /// </summary>
    Confirming = 2,

    /// <summary>
    /// Transaction was confirmed successfully.
    /// </summary>
    Confirmed = 3,

    /// <summary>
    /// Transaction failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Transaction was cancelled/reverted.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Transaction timed out.
    /// </summary>
    TimedOut = 6
}

/// <summary>
/// Represents how a session ended.
/// </summary>
public enum SessionEndReason
{
    /// <summary>
    /// Session completed normally.
    /// </summary>
    Completed = 0,

    /// <summary>
    /// Player chose to quit.
    /// </summary>
    PlayerQuit = 1,

    /// <summary>
    /// Session was abandoned due to inactivity.
    /// </summary>
    Inactivity = 2,

    /// <summary>
    /// Session failed due to an error.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Admin or system terminated the session.
    /// </summary>
    AdminTerminated = 4,

    /// <summary>
    /// Parent/guardian ended the session.
    /// </summary>
    ParentalControl = 5
}

/// <summary>
/// Represents the type of player in a session.
/// </summary>
public enum PlayerType
{
    /// <summary>
    /// Player with a saved profile.
    /// </summary>
    Profile = 0,

    /// <summary>
    /// Guest player without a saved profile.
    /// </summary>
    Guest = 1
}
