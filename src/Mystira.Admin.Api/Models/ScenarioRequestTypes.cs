namespace Mystira.Admin.Api.Models;

/// <summary>
/// Request model for scene media references.
/// Used when mapping domain scene media to request format.
/// </summary>
public class SceneMediaRequest
{
    /// <summary>
    /// Image media reference ID
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Audio media reference ID
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Video media reference ID
    /// </summary>
    public string? Video { get; set; }
}

/// <summary>
/// Request model for echo log entries.
/// Used when mapping domain echo logs to request format.
/// </summary>
public class EchoLogRequest
{
    /// <summary>
    /// The type of echo (e.g., "compassion", "courage")
    /// </summary>
    public string? EchoType { get; set; }

    /// <summary>
    /// Description of the echo effect
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Strength of the echo effect
    /// </summary>
    public float Strength { get; set; }
}

/// <summary>
/// Request model for compass changes.
/// Used when mapping domain compass changes to request format.
/// </summary>
public class CompassChangeRequest
{
    /// <summary>
    /// The compass axis affected
    /// </summary>
    public string? Axis { get; set; }

    /// <summary>
    /// The delta change value
    /// </summary>
    public float Delta { get; set; }
}
