using Mystira.Domain.Models;

namespace Mystira.Authoring.Abstractions.Models.Scenario;

/// <summary>
/// Represents a branching choice in a scene.
/// </summary>
public class Branch
{
    /// <summary>
    /// Text displayed for this choice.
    /// </summary>
    public string Choice { get; set; } = string.Empty;

    /// <summary>
    /// ID of the scene this choice leads to.
    /// </summary>
    public string NextSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Echo log entry for this choice (if any).
    /// Uses Domain EchoLog type.
    /// </summary>
    public EchoLog? EchoLog { get; set; }

    /// <summary>
    /// Compass change triggered by this choice (if any).
    /// Uses Domain CompassChange entity (AxisId, int Delta, Reason).
    /// </summary>
    public CompassChange? CompassChange { get; set; }
}
