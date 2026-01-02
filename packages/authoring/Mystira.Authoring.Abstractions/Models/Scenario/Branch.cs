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
    /// </summary>
    public EchoLog? EchoLog { get; set; }

    /// <summary>
    /// Compass change triggered by this choice (if any).
    /// </summary>
    public CompassChange? CompassChange { get; set; }
}

/// <summary>
/// Log entry for an echo (player decision pattern).
/// </summary>
public class EchoLog
{
    /// <summary>
    /// Type of echo being logged.
    /// </summary>
    public string EchoType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the echo event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Strength of the echo (0.0 to 1.0).
    /// </summary>
    public double Strength { get; set; }

    /// <summary>
    /// When the echo was logged.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Change to a compass axis value.
/// </summary>
public class CompassChange
{
    /// <summary>
    /// The axis being changed.
    /// </summary>
    public string Axis { get; set; } = string.Empty;

    /// <summary>
    /// The delta to apply to the axis value.
    /// </summary>
    public double Delta { get; set; }

    /// <summary>
    /// Optional link to a developmental concept.
    /// </summary>
    public string? DevelopmentalLink { get; set; }
}

/// <summary>
/// Reveal condition for an echo.
/// </summary>
public class EchoReveal
{
    /// <summary>
    /// Type of echo that triggers the reveal.
    /// </summary>
    public string EchoType { get; set; } = string.Empty;

    /// <summary>
    /// Minimum echo strength required for reveal.
    /// </summary>
    public double MinStrength { get; set; }

    /// <summary>
    /// Scene that triggers the reveal check.
    /// </summary>
    public string TriggerSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of scenes since the echo for it to still trigger.
    /// </summary>
    public int? MaxAgeScenes { get; set; }

    /// <summary>
    /// Mechanic used for the reveal.
    /// </summary>
    public string? RevealMechanic { get; set; }

    /// <summary>
    /// Whether this reveal is required for story progression.
    /// </summary>
    public bool? Required { get; set; }
}

/// <summary>
/// Tracking state for a compass axis.
/// </summary>
public class CompassTracking
{
    /// <summary>
    /// The axis being tracked.
    /// </summary>
    public string Axis { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the axis.
    /// </summary>
    public double CurrentValue { get; set; } = 0.0;

    /// <summary>
    /// Starting value of the axis.
    /// </summary>
    public double StartingValue { get; set; } = 0.0;

    /// <summary>
    /// History of changes to the axis.
    /// </summary>
    public List<CompassChange> History { get; set; } = new();

    /// <summary>
    /// When the axis was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
