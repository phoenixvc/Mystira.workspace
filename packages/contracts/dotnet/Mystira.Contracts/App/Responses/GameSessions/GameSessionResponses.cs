using Mystira.Contracts.App.Models.GameSessions;

namespace Mystira.Contracts.App.Responses.GameSessions;

/// <summary>
/// Response containing game session information.
/// </summary>
public record GameSessionResponse
{
    /// <summary>
    /// The unique identifier of the session.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the scenario being played.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// The account identifier of the session owner.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// The profile identifier of the primary player.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The list of player names participating in the session.
    /// </summary>
    public List<string> PlayerNames { get; set; } = new();

    /// <summary>
    /// The current status of the session (e.g., Active, Paused, Completed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the current scene.
    /// </summary>
    public string CurrentSceneId { get; set; } = string.Empty;

    /// <summary>
    /// The total number of choices made in this session.
    /// </summary>
    public int ChoiceCount { get; set; }

    /// <summary>
    /// The date and time when the session started.
    /// Null indicates the session has not yet started.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// The date and time when the session ended, if completed.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// The target age group for content filtering in this session.
    /// </summary>
    public string TargetAgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Compass progress totals for all players in the session.
    /// </summary>
    public List<PlayerCompassProgressDto>? PlayerCompassProgressTotals { get; set; }

    /// <summary>
    /// The number of echoes earned in this session.
    /// </summary>
    public int EchoCount { get; set; }

    /// <summary>
    /// The number of achievements earned in this session.
    /// </summary>
    public int AchievementCount { get; set; }

    /// <summary>
    /// The elapsed time of the session.
    /// </summary>
    public TimeSpan? ElapsedTime { get; set; }

    /// <summary>
    /// Indicates whether the session is currently paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// The total number of scenes in the session.
    /// </summary>
    public int SceneCount { get; set; }

    /// <summary>
    /// The character assignments for this session.
    /// </summary>
    public List<CharacterAssignmentDto>? CharacterAssignments { get; set; }
}

/// <summary>
/// Response containing session statistics.
/// </summary>
public record SessionStatsResponse
{
    /// <summary>
    /// Dictionary of compass axis values accumulated during the session.
    /// </summary>
    public Dictionary<string, double> CompassValues { get; set; } = new();

    /// <summary>
    /// The total number of choices made during the session.
    /// </summary>
    public int TotalChoices { get; set; }

    /// <summary>
    /// The total duration of the session.
    /// </summary>
    public TimeSpan SessionDuration { get; set; }

    /// <summary>
    /// Compass progress totals for all players in the session.
    /// </summary>
    public List<PlayerCompassProgressDto>? PlayerCompassProgressTotals { get; set; }

    /// <summary>
    /// Recent echoes earned during the session.
    /// </summary>
    public List<object>? RecentEchoes { get; set; }

    /// <summary>
    /// Achievements earned during the session.
    /// </summary>
    public List<object>? Achievements { get; set; }
}
