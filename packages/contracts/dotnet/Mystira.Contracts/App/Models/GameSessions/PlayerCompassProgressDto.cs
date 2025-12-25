namespace Mystira.Contracts.App.Models.GameSessions;

/// <summary>
/// Data transfer object representing a player's progress on a compass axis.
/// </summary>
public class PlayerCompassProgressDto
{
    /// <summary>
    /// The unique identifier of the player.
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// The compass axis name.
    /// </summary>
    public string? Axis { get; set; }

    /// <summary>
    /// The total progress value on this axis.
    /// </summary>
    public int Total { get; set; }
}
