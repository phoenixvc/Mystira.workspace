namespace Mystira.Contracts.App.Responses.ProfileScores;

/// <summary>
/// Individual axis score item for a specific scenario/game session.
/// </summary>
public record AxisScoreItem
{
    /// <summary>
    /// The scenario identifier.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// The game session identifier.
    /// </summary>
    public string GameSessionId { get; set; } = string.Empty;

    /// <summary>
    /// When the score was recorded.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Dictionary of axis names to their scores.
    /// </summary>
    public Dictionary<string, float> AxisScores { get; set; } = new();
}

/// <summary>
/// Response containing axis scores for a user profile.
/// </summary>
public record AxisScoresResponse
{
    /// <summary>
    /// The user profile identifier.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// List of axis score items.
    /// </summary>
    public List<AxisScoreItem> Items { get; set; } = new();
}
