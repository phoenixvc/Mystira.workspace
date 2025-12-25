namespace Mystira.Contracts.App.Models.GameSessions;

/// <summary>
/// Data transfer object representing a player's progress on a compass axis.
/// </summary>
public record PlayerCompassProgressDto
{
    /// <summary>
    /// The unique identifier of the player profile.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the player.
    /// </summary>
    public string? PlayerName { get; set; }

    /// <summary>
    /// The compass axis identifier.
    /// </summary>
    public string CompassAxisId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the compass axis.
    /// </summary>
    public string? AxisName { get; set; }

    /// <summary>
    /// The current score value on this axis.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// The normalized score (0-1 range).
    /// </summary>
    public double NormalizedScore { get; set; }

    /// <summary>
    /// The current level achieved on this axis.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// The total progress accumulated on this axis.
    /// </summary>
    public double TotalProgress { get; set; }

    /// <summary>
    /// The number of choices that contributed to this axis.
    /// </summary>
    public int ChoiceCount { get; set; }
}
